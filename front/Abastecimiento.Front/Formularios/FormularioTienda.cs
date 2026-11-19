using System.ComponentModel.DataAnnotations;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Formularios;

/// <summary>Formulario que conoce los tipos de espacio disponibles (para validar categorias).</summary>
public interface IConTiposEspacio
{
    /// <summary>Tipos de espacio de <c>GET /api/tipos-espacio</c>.</summary>
    IReadOnlyList<TipoEspacioRespuesta> TiposEspacio { get; }
}

/// <summary>Modelo del formulario "Nueva tienda" (spec §6.1). Los nombres de campo coinciden con la API.</summary>
public sealed class FormularioTienda : IConTiposEspacio
{
    /// <summary>Codigo HANNA de la tienda.</summary>
    [Required(ErrorMessage = "Ingrese el código HANNA.")]
    [Range(1, int.MaxValue, ErrorMessage = "El código HANNA debe ser un entero mayor que 0.")]
    public int? CodigoHanna { get; set; }

    /// <summary>Nombre; se recorta al enviar.</summary>
    [Required(ErrorMessage = "Ingrese el nombre de la tienda.")]
    [StringLength(150, ErrorMessage = "El nombre admite máximo {1} caracteres.")]
    public string? Nombre { get; set; }

    /// <summary>Marca de tienda seleccionada.</summary>
    [Required(ErrorMessage = "Seleccione la marca de tienda.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la marca de tienda.")]
    public int? MarcaTiendaId { get; set; }

    /// <summary>Regional seleccionada.</summary>
    [Required(ErrorMessage = "Seleccione la regional.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione la regional.")]
    public int? RegionalId { get; set; }

    /// <summary>Tipo de tienda seleccionado.</summary>
    [Required(ErrorMessage = "Seleccione el tipo de tienda.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el tipo de tienda.")]
    public int? TipoTiendaId { get; set; }

    /// <summary>Responsable visual (opcional).</summary>
    public int? ResponsableVisualId { get; set; }

    /// <summary>Filas de espacios (tipo + cantidad).</summary>
    [MinLength(1, ErrorMessage = "Agregue al menos un espacio.")]
    [FilasValidas]
    [MaximoMobiliario]
    [SinTiposRepetidos]
    public List<FilaEspacio> Espacios { get; } = [new()];

    /// <inheritdoc />
    public IReadOnlyList<TipoEspacioRespuesta> TiposEspacio { get; set; } = [];

    /// <summary>Espacios Totales en vivo (Regla 1: suma de cantidades); el valor oficial lo calcula la API.</summary>
    public int EspaciosTotales => Espacios.Sum(f => f.CantidadEspacios ?? 0);

    /// <summary>Tipos de mobiliario distintos elegidos (los perimetrales no cuentan).</summary>
    public int TiposMobiliario => MaximoMobiliarioAttribute.ContarMobiliario(Espacios, TiposEspacio);

    /// <summary>Convierte a la solicitud de la API. Requiere un formulario valido.</summary>
    public CrearTiendaSolicitud ASolicitud() => new(
        CodigoHanna ?? 0,
        (Nombre ?? string.Empty).Trim(),
        MarcaTiendaId ?? 0,
        RegionalId ?? 0,
        TipoTiendaId ?? 0,
        ResponsableVisualId,
        Espacios.Select(f => new EspacioSolicitud(f.TipoEspacioId ?? 0, f.CantidadEspacios ?? 0)).ToList());
}

/// <summary>Fila de espacios del formulario de tienda.</summary>
public sealed class FilaEspacio
{
    /// <summary>Tipo de espacio.</summary>
    [Required(ErrorMessage = "Seleccione el tipo de espacio.")]
    public int? TipoEspacioId { get; set; }

    /// <summary>Cantidad de espacios (entero &gt;= 0).</summary>
    [Required(ErrorMessage = "Ingrese la cantidad de espacios.")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser un entero mayor o igual a 0.")]
    public int? CantidadEspacios { get; set; }
}
