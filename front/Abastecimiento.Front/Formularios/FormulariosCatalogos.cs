using System.ComponentModel.DataAnnotations;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Formularios;

/// <summary>Formulario de alta con un solo campo Nombre.</summary>
public interface IFormularioNombre
{
    /// <summary>Nombre a crear; se recorta al enviar.</summary>
    string? Nombre { get; set; }
}

/// <summary>Marca de tienda, regional o tipo de tienda (spec §6.2: maximo 50 caracteres).</summary>
public sealed class FormularioCatalogo : IFormularioNombre
{
    /// <inheritdoc />
    [Required(ErrorMessage = "Ingrese el nombre.")]
    [StringLength(50, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
    public string? Nombre { get; set; }
}

/// <summary>Tipo de espacio: nombre (50) y categoria mobiliario o perimetral.</summary>
public sealed class FormularioTipoEspacio
{
    /// <summary>Nombre del tipo de espacio.</summary>
    [Required(ErrorMessage = "Ingrese el nombre.")]
    [StringLength(50, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Categoria del tipo de espacio.</summary>
    [Required(ErrorMessage = "Seleccione la categoría.")]
    public CategoriaEspacio? Categoria { get; set; }

    /// <summary>Convierte a la solicitud de la API. Requiere un formulario valido.</summary>
    public CrearTipoEspacioSolicitud ASolicitud() =>
        new((Nombre ?? string.Empty).Trim(), Categoria ?? CategoriaEspacio.Mobiliario);
}

/// <summary>Buscador "Calcular carga por codigo HANNA" de la pagina de inicio.</summary>
public sealed class FormularioBusquedaHanna
{
    /// <summary>Codigo HANNA a consultar.</summary>
    [Required(ErrorMessage = "Ingrese el código HANNA.")]
    [Range(1, int.MaxValue, ErrorMessage = "El código HANNA debe ser un entero mayor que 0.")]
    public int? CodigoHanna { get; set; }
}

/// <summary>Filtros del listado de tiendas (vacio = todas).</summary>
public sealed class FormularioFiltroTiendas
{
    /// <summary>Marca de tienda.</summary>
    public int? MarcaTiendaId { get; set; }

    /// <summary>Regional.</summary>
    public int? RegionalId { get; set; }
}

/// <summary>Criterio del reporte consolidado; con <see cref="AgruparPor.Nivel"/> el nivel es obligatorio.</summary>
public sealed class FormularioReporte : IValidatableObject
{
    /// <summary>Criterio de agrupacion.</summary>
    public AgruparPor AgruparPor { get; set; } = AgruparPor.Tienda;

    /// <summary>Nivel del arbol (solo con <see cref="AgruparPor.Nivel"/>).</summary>
    public int? NivelId { get; set; }

    /// <summary>Nivel a enviar a la API (null si no se agrupa por nivel).</summary>
    public int? NivelAEnviar => AgruparPor == AgruparPor.Nivel ? NivelId : null;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AgruparPor == AgruparPor.Nivel && NivelId is null)
        {
            yield return new ValidationResult("Seleccione el nivel del árbol.", [nameof(NivelId)]);
        }
    }
}
