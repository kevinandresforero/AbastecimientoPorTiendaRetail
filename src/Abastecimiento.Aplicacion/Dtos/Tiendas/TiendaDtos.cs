using System.ComponentModel.DataAnnotations;
using Abastecimiento.Aplicacion.Dtos.Catalogos;
using Abastecimiento.Dominio.Entidades.Tiendas;

namespace Abastecimiento.Aplicacion.Dtos.Tiendas;

/// <summary>Datos para registrar una tienda con su configuracion de espacios.</summary>
public sealed record CrearTiendaSolicitud
{
    /// <summary>Codigo HANNA unico de la tienda.</summary>
    [Range(1, int.MaxValue)]
    public int CodigoHanna { get; init; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MarcaTiendaId { get; init; }

    [Range(1, int.MaxValue)]
    public int RegionalId { get; init; }

    [Range(1, int.MaxValue)]
    public int TipoTiendaId { get; init; }

    [Range(1, int.MaxValue)]
    public int? ResponsableVisualId { get; init; }

    /// <summary>Hasta 3 tipos de mobiliario mas los espacios perimetrales, sin tipos repetidos.</summary>
    [Required]
    [MinLength(1)]
    public IReadOnlyList<EspacioSolicitud> Espacios { get; init; } = [];
}

/// <summary>Cantidad de espacios de un tipo de espacio en la tienda.</summary>
public sealed record EspacioSolicitud
{
    [Range(1, int.MaxValue)]
    public int TipoEspacioId { get; init; }

    [Range(0, int.MaxValue)]
    public int CantidadEspacios { get; init; }
}

/// <summary>Tienda con sus catalogos, espacios y Espacios Totales (Regla 1).</summary>
public sealed record TiendaRespuesta(
    int Id,
    int CodigoHanna,
    string Nombre,
    CatalogoRespuesta MarcaTienda,
    CatalogoRespuesta Regional,
    CatalogoRespuesta TipoTienda,
    CatalogoRespuesta? ResponsableVisual,
    IReadOnlyList<EspacioTiendaRespuesta> Espacios,
    int EspaciosTotales);

/// <summary>Espacios de un tipo dentro de la tienda.</summary>
public sealed record EspacioTiendaRespuesta(string TipoEspacio, CategoriaEspacio Categoria, int CantidadEspacios);

/// <summary>Tipo de espacio fisico (mueble o perimetral).</summary>
public sealed record TipoEspacioRespuesta(int Id, string Nombre, CategoriaEspacio Categoria);

/// <summary>Datos para crear un tipo de espacio.</summary>
public sealed record CrearTipoEspacioSolicitud
{
    [Required]
    [StringLength(50)]
    public string Nombre { get; init; } = string.Empty;

    /// <summary><c>mobiliario</c> (cuenta para el tope de 3 por tienda) o <c>perimetral</c>.</summary>
    [EnumDataType(typeof(CategoriaEspacio))]
    public CategoriaEspacio Categoria { get; init; }
}
