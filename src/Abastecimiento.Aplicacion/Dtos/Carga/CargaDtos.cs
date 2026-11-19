namespace Abastecimiento.Aplicacion.Dtos.Carga;

/// <summary>Desglose de carga de una tienda (R3 / R4a). Decimales completos salvo las cargas totales.</summary>
/// <param name="CodigoHanna">Codigo HANNA de la tienda.</param>
/// <param name="NombreTienda">Nombre de la tienda.</param>
/// <param name="EspaciosTotales">Espacios fisicos totales (Regla 1).</param>
/// <param name="Lineas">Una linea por tipo de producto (nivel hoja).</param>
/// <param name="Nodos">Espacios calculados de todos los nodos del arbol (preorden).</param>
/// <param name="TotalEspaciosAsignados">Suma de espacios de las lineas, sin redondear.</param>
/// <param name="TotalCarga">Suma de las cargas totales de las lineas (ya redondeadas).</param>
/// <param name="Advertencias">Ramas que no llegan al nivel hoja.</param>
public sealed record CargaTiendaRespuesta(
    int CodigoHanna,
    string NombreTienda,
    int EspaciosTotales,
    IReadOnlyList<LineaCargaRespuesta> Lineas,
    IReadOnlyList<NodoCalculadoRespuesta> Nodos,
    decimal TotalEspaciosAsignados,
    int TotalCarga,
    IReadOnlyList<string> Advertencias);

/// <summary>Carga de un tipo de producto.</summary>
/// <param name="NodoId">Nodo hoja.</param>
/// <param name="TipoProducto">Nombre del tipo de producto.</param>
/// <param name="Ruta">Camino desde la raiz con el porcentaje de cada nivel.</param>
/// <param name="EspaciosCalculados">Espacios con todos sus decimales (Regla 2).</param>
/// <param name="UnidadesPorEspacio">Capacidad estandar del tipo de producto.</param>
/// <param name="CargaCalculada">Espacios x unidades, sin redondear.</param>
/// <param name="CargaTotal">Carga calculada redondeada a unidades (Regla 3).</param>
public sealed record LineaCargaRespuesta(
    int NodoId,
    string TipoProducto,
    IReadOnlyList<PasoRutaRespuesta> Ruta,
    decimal EspaciosCalculados,
    int UnidadesPorEspacio,
    decimal CargaCalculada,
    int CargaTotal);

/// <summary>Un nivel dentro de la ruta de una linea.</summary>
public sealed record PasoRutaRespuesta(string Nivel, string Nombre, decimal PorcentajeParticipacion);

/// <summary>Espacios calculados de un nodo cualquiera del arbol.</summary>
public sealed record NodoCalculadoRespuesta(
    int NodoId,
    int? NodoPadreId,
    string Nivel,
    string Nombre,
    decimal PorcentajeParticipacion,
    decimal EspaciosCalculados);
