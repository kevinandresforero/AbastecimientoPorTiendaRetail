using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Abastecimiento.Front.Formularios;

/// <summary>
/// Lee un porcentaje escrito por el usuario en % ("30", "30,5" o "30.5") y lo convierte a la fraccion que espera la API
/// (30 → 0.30). Es conversion de unidades, no logica de negocio.
/// </summary>
public static class LectorPorcentaje
{
    /// <summary>Decimales maximos en % (la BD guarda 6 decimales de fraccion).</summary>
    public const int MaximoDecimales = 4;

    /// <summary>Intenta leer el texto como porcentaje; devuelve tambien cuantos decimales trae.</summary>
    public static bool TryLeer(string? texto, out decimal porcentaje, out int decimales)
    {
        porcentaje = 0;
        decimales = 0;
        if (string.IsNullOrWhiteSpace(texto))
        {
            return false;
        }

        var limpio = texto.Trim().TrimEnd('%').Trim();
        if (limpio.Contains(',') && limpio.Contains('.'))
        {
            return false;
        }

        limpio = limpio.Replace(',', '.');
        if (!decimal.TryParse(limpio, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out porcentaje))
        {
            return false;
        }

        var punto = limpio.IndexOf('.');
        decimales = punto < 0 ? 0 : limpio[(punto + 1)..].TrimEnd('0').Length;
        return true;
    }

    /// <summary>Convierte el porcentaje escrito a fraccion (30 → 0.30). Requiere un texto valido.</summary>
    public static decimal AFraccion(string? texto) =>
        TryLeer(texto, out var porcentaje, out _)
            ? porcentaje / 100m
            : throw new FormatException($"'{texto}' no es un porcentaje válido.");
}

/// <summary>Porcentaje en %: numero mayor que 0 y menor o igual a 100, con maximo 4 decimales.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PorcentajeValidoAttribute : ValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string texto || string.IsNullOrWhiteSpace(texto))
        {
            return ValidationResult.Success;
        }

        var miembros = MaximoMobiliarioAttribute.Miembros(validationContext);
        if (!LectorPorcentaje.TryLeer(texto, out var porcentaje, out var decimales))
        {
            return new ValidationResult("Ingrese un número en %, por ejemplo 30 o 30,5.", miembros);
        }

        if (porcentaje <= 0 || porcentaje > 100)
        {
            return new ValidationResult("El porcentaje debe ser mayor que 0 y menor o igual a 100.", miembros);
        }

        return decimales > LectorPorcentaje.MaximoDecimales
            ? new ValidationResult($"El porcentaje admite máximo {LectorPorcentaje.MaximoDecimales} decimales.", miembros)
            : ValidationResult.Success;
    }
}

/// <summary>Formulario "Editar porcentaje" de un nodo (spec §6.4).</summary>
public sealed class FormularioPorcentaje
{
    /// <summary>Porcentaje escrito en %.</summary>
    [Required(ErrorMessage = "Ingrese el porcentaje.")]
    [PorcentajeValido]
    public string? Porcentaje { get; set; }

    /// <summary>Fraccion a enviar (30 → 0.30). Requiere un formulario valido.</summary>
    public decimal Fraccion => LectorPorcentaje.AFraccion(Porcentaje);
}

/// <summary>Formulario "Agregar nodo": concepto del nivel siguiente y porcentaje.</summary>
public sealed class FormularioNodo
{
    /// <summary>Concepto comercial del nodo.</summary>
    [Required(ErrorMessage = "Seleccione el concepto.")]
    public int? ConceptoComercialId { get; set; }

    /// <summary>Porcentaje escrito en %.</summary>
    [Required(ErrorMessage = "Ingrese el porcentaje.")]
    [PorcentajeValido]
    public string? Porcentaje { get; set; }

    /// <summary>Fraccion a enviar (30 → 0.30). Requiere un formulario valido.</summary>
    public decimal Fraccion => LectorPorcentaje.AFraccion(Porcentaje);
}
