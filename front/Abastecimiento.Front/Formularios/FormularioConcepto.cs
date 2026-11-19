using System.ComponentModel.DataAnnotations;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Formularios;

/// <summary>Formulario que sabe si el nivel elegido es el nivel hoja.</summary>
public interface IConNivelHoja
{
    /// <summary>True si el nivel seleccionado es hoja (Tipo de Producto).</summary>
    bool NivelEsHoja { get; }
}

/// <summary>Modelo de "Nuevo concepto" (spec §6.3).</summary>
public sealed class FormularioConcepto : IConNivelHoja
{
    /// <summary>Nivel de la jerarquia.</summary>
    [Required(ErrorMessage = "Seleccione el nivel.")]
    public int? NivelJerarquiaId { get; set; }

    /// <summary>Nombre del concepto.</summary>
    [Required(ErrorMessage = "Ingrese el nombre.")]
    [StringLength(100, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Unidades por espacio: obligatorio y &gt; 0 solo en el nivel hoja; en otro nivel se envia null.</summary>
    [RequeridoSiNivelHoja]
    public int? UnidadesPorEspacio { get; set; }

    /// <summary>Niveles de <c>GET /api/niveles-jerarquia</c>.</summary>
    public IReadOnlyList<NivelRespuesta> Niveles { get; set; } = [];

    /// <inheritdoc />
    public bool NivelEsHoja => Niveles.FirstOrDefault(n => n.Id == NivelJerarquiaId)?.EsHoja ?? false;

    /// <summary>Convierte a la solicitud de la API. Requiere un formulario valido.</summary>
    public CrearConceptoSolicitud ASolicitud() => new(
        NivelJerarquiaId ?? 0,
        (Nombre ?? string.Empty).Trim(),
        NivelEsHoja ? UnidadesPorEspacio : null);
}

/// <summary>Validador condicional: el valor es obligatorio y mayor que 0 solo si el nivel elegido es hoja.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequeridoSiNivelHojaAttribute : ValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not IConNivelHoja { NivelEsHoja: true })
        {
            return ValidationResult.Success;
        }

        return value is int unidades && unidades > 0
            ? ValidationResult.Success
            : new ValidationResult(
                "En el nivel hoja las unidades por espacio son obligatorias y deben ser mayores que 0.",
                MaximoMobiliarioAttribute.Miembros(validationContext));
    }
}
