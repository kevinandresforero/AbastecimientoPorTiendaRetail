using System.Collections;
using System.ComponentModel.DataAnnotations;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Formularios;

/// <summary>
/// Regla de forma: maximo <see cref="Maximo"/> tipos de espacio de categoria mobiliario (los perimetrales no cuentan).
/// Requiere que el formulario implemente <see cref="IConTiposEspacio"/> para conocer la categoria de cada tipo.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaximoMobiliarioAttribute : ValidationAttribute
{
    /// <summary>Maximo de tipos de mobiliario por tienda.</summary>
    public const int Maximo = 3;

    /// <summary>Cuenta los tipos de mobiliario distintos de las filas.</summary>
    public static int ContarMobiliario(IEnumerable<FilaEspacio> filas, IEnumerable<TipoEspacioRespuesta> tipos)
    {
        var mobiliario = tipos.Where(t => t.Categoria == CategoriaEspacio.Mobiliario).Select(t => t.Id).ToHashSet();
        return filas
            .Where(f => f.TipoEspacioId is int id && mobiliario.Contains(id))
            .Select(f => f.TipoEspacioId)
            .Distinct()
            .Count();
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<FilaEspacio> filas || validationContext.ObjectInstance is not IConTiposEspacio formulario)
        {
            return ValidationResult.Success;
        }

        var cantidad = ContarMobiliario(filas, formulario.TiposEspacio);
        return cantidad <= Maximo
            ? ValidationResult.Success
            : new ValidationResult(
                $"Una tienda admite máximo {Maximo} tipos de mobiliario; seleccionó {cantidad}. Los perimetrales no cuentan.",
                Miembros(validationContext));
    }

    internal static string[]? Miembros(ValidationContext contexto) =>
        contexto.MemberName is { } miembro ? [miembro] : null;
}

/// <summary>Regla de forma: un tipo de espacio no puede repetirse entre las filas.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SinTiposRepetidosAttribute : ValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable<FilaEspacio> filas)
        {
            return ValidationResult.Success;
        }

        var repetidos = filas
            .Where(f => f.TipoEspacioId is not null)
            .GroupBy(f => f.TipoEspacioId!.Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        if (repetidos.Count == 0)
        {
            return ValidationResult.Success;
        }

        var tipos = (validationContext.ObjectInstance as IConTiposEspacio)?.TiposEspacio ?? [];
        var nombres = repetidos.Select(id => tipos.FirstOrDefault(t => t.Id == id)?.Nombre ?? $"#{id}");
        return new ValidationResult(
            $"Un tipo de espacio no puede repetirse: {string.Join(", ", nombres)}.",
            MaximoMobiliarioAttribute.Miembros(validationContext));
    }
}

/// <summary>
/// Valida cada elemento de una lista con sus DataAnnotations (el <c>DataAnnotationsValidator</c> de Blazor no valida
/// objetos anidados al enviar). Los mensajes se prefijan con el numero de fila.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FilasValidasAttribute : ValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IEnumerable filas)
        {
            return ValidationResult.Success;
        }

        var mensajes = new List<string>();
        var numero = 0;
        foreach (var fila in filas)
        {
            numero++;
            if (fila is null)
            {
                continue;
            }

            var resultados = new List<ValidationResult>();
            if (!Validator.TryValidateObject(fila, new ValidationContext(fila), resultados, validateAllProperties: true))
            {
                mensajes.AddRange(resultados.Select(r => $"Espacio {numero}: {r.ErrorMessage}"));
            }
        }

        return mensajes.Count == 0
            ? ValidationResult.Success
            : new ValidationResult(string.Join(" ", mensajes), MaximoMobiliarioAttribute.Miembros(validationContext));
    }
}
