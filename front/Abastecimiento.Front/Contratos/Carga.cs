namespace Abastecimiento.Front.Contratos;

/// <summary>Resultado del motor de calculo para una tienda (<c>GET /api/carga/{codigoHanna}</c>).</summary>
public sealed record CargaTiendaRespuesta(
    int CodigoHanna,
    string NombreTienda,
    int EspaciosTotales,
    IReadOnlyList<LineaCargaRespuesta> Lineas,
    IReadOnlyList<NodoCalculadoRespuesta> Nodos,
    decimal TotalEspaciosAsignados,
    int TotalCarga,
    IReadOnlyList<string> Advertencias);

/// <summary>Carga de un tipo de producto (hoja del arbol) con la ruta desde la raiz.</summary>
public sealed record LineaCargaRespuesta(
    int NodoId,
    string TipoProducto,
    IReadOnlyList<PasoRutaRespuesta> Ruta,
    decimal EspaciosCalculados,
    int UnidadesPorEspacio,
    decimal CargaCalculada,
    int CargaTotal);

/// <summary>Paso de la ruta de una linea: nombre del nivel, concepto y % de participacion (fraccion).</summary>
public sealed record PasoRutaRespuesta(string Nivel, string Nombre, decimal PorcentajeParticipacion);

/// <summary>Espacios calculados de cada nodo del arbol para la tienda.</summary>
public sealed record NodoCalculadoRespuesta(
    int NodoId,
    int? NodoPadreId,
    string Nivel,
    string Nombre,
    decimal PorcentajeParticipacion,
    decimal EspaciosCalculados);
