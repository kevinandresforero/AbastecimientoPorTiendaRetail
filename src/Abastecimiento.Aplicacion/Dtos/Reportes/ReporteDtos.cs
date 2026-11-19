using Abastecimiento.Aplicacion.Dtos.ArbolComercial;

namespace Abastecimiento.Aplicacion.Dtos.Reportes;

/// <summary>Criterio de agrupacion del consolidado (R4b).</summary>
public enum AgruparPor
{
    /// <summary>Una fila por tienda.</summary>
    Tienda,

    /// <summary>Por marca de tienda (YAHAD, LILI PINK, YOI).</summary>
    MarcaTienda,

    /// <summary>Por regional (BOGOTA, ANTIOQUIA).</summary>
    Regional,

    /// <summary>Por concepto de un nivel del arbol (requiere <c>nivelId</c>; ej. 3 = marca de producto).</summary>
    Nivel
}

/// <summary>Consolidado de espacios y unidades.</summary>
/// <param name="AgruparPor">Criterio aplicado.</param>
/// <param name="Nivel">Nivel usado cuando se agrupa por nivel.</param>
/// <param name="Grupos">Filas del consolidado.</param>
/// <param name="Total">Fila de total general.</param>
public sealed record ConsolidadoRespuesta(
    AgruparPor AgruparPor,
    NivelRespuesta? Nivel,
    IReadOnlyList<GrupoConsolidadoRespuesta> Grupos,
    GrupoConsolidadoRespuesta Total);

/// <summary>Fila del consolidado.</summary>
/// <param name="Clave">Identificador del grupo (codigo HANNA o Id del catalogo/concepto).</param>
/// <param name="Nombre">Nombre del grupo.</param>
/// <param name="CantidadTiendas">Tiendas incluidas en el grupo.</param>
/// <param name="EspaciosTotales">Espacios fisicos (Regla 1); null al agrupar por nivel.</param>
/// <param name="EspaciosAsignados">Espacios asignados a tipos de producto, sin redondear.</param>
/// <param name="CargaTotal">Suma de cargas totales de linea (unidades).</param>
public sealed record GrupoConsolidadoRespuesta(
    string Clave,
    string Nombre,
    int CantidadTiendas,
    int? EspaciosTotales,
    decimal EspaciosAsignados,
    int CargaTotal);
