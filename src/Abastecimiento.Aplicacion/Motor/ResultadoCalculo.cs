using Abastecimiento.Dominio.Entidades.ArbolComercial;

namespace Abastecimiento.Aplicacion.Motor;

/// <summary>Espacios calculados para un nodo cualquiera del arbol (Regla 2).</summary>
public sealed record NodoCalculado(NodoComercial Nodo, decimal EspaciosCalculados);

/// <summary>Linea de carga de un tipo de producto (nodo hoja, Regla 3).</summary>
/// <param name="Nodo">Nodo hoja.</param>
/// <param name="Camino">Nodos desde la raiz hasta la hoja.</param>
/// <param name="EspaciosCalculados">Espacios con todos sus decimales.</param>
/// <param name="UnidadesPorEspacio">Capacidad estandar del tipo de producto.</param>
/// <param name="CargaCalculada">Espacios x unidades, sin redondear.</param>
/// <param name="CargaTotal">Carga calculada redondeada (unico redondeo).</param>
public sealed record LineaCalculada(
    NodoComercial Nodo,
    IReadOnlyList<NodoComercial> Camino,
    decimal EspaciosCalculados,
    int UnidadesPorEspacio,
    decimal CargaCalculada,
    int CargaTotal);

/// <summary>Resultado del motor para una tienda.</summary>
/// <param name="Nodos">Todos los nodos recorridos (preorden) con sus espacios.</param>
/// <param name="Lineas">Lineas de los nodos hoja.</param>
/// <param name="TotalEspaciosAsignados">Suma de espacios de las hojas (sin redondear).</param>
/// <param name="TotalCarga">Suma de las cargas de linea ya redondeadas.</param>
/// <param name="Advertencias">Ramas que no alcanzan el nivel hoja u hojas sin unidades por espacio.</param>
public sealed record ResultadoCalculo(
    IReadOnlyList<NodoCalculado> Nodos,
    IReadOnlyList<LineaCalculada> Lineas,
    decimal TotalEspaciosAsignados,
    int TotalCarga,
    IReadOnlyList<string> Advertencias);
