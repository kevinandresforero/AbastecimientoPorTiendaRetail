namespace Abastecimiento.Front.Contratos;

/// <summary>Criterio de agrupacion del consolidado; en la query va en camelCase (<c>marcaTienda</c>).</summary>
public enum AgruparPor
{
    /// <summary>Una fila por tienda.</summary>
    Tienda,

    /// <summary>Por marca de tienda (Lili Pink, Yoi, Yahad).</summary>
    MarcaTienda,

    /// <summary>Por regional.</summary>
    Regional,

    /// <summary>Por concepto de un nivel del arbol (requiere <c>nivelId</c>).</summary>
    Nivel
}

/// <summary>Consolidado de espacios y unidades (<c>GET /api/reportes/consolidado</c>).</summary>
public sealed record ConsolidadoRespuesta(
    AgruparPor AgruparPor,
    NivelRespuesta? Nivel,
    IReadOnlyList<GrupoConsolidadoRespuesta> Grupos,
    GrupoConsolidadoRespuesta Total);

/// <summary>Fila del consolidado; <see cref="EspaciosTotales"/> es null al agrupar por nivel del arbol.</summary>
public sealed record GrupoConsolidadoRespuesta(
    string Clave,
    string Nombre,
    int CantidadTiendas,
    int? EspaciosTotales,
    decimal EspaciosAsignados,
    int CargaTotal);
