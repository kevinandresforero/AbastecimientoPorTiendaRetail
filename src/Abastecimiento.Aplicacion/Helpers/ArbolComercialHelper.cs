using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Reglas;

namespace Abastecimiento.Aplicacion.Helpers;

/// <summary>Arbol comercial indexado en memoria a partir de la lista plana de nodos.</summary>
/// <param name="PorId">Nodos por Id.</param>
/// <param name="HijosDe">Hijos de cada nodo (clave null = raices), ordenados por Id.</param>
/// <param name="Raices">Nodos sin padre.</param>
/// <param name="ProfundidadHoja">Mayor profundidad de los niveles presentes (0 si el arbol esta vacio).</param>
public sealed record ArbolIndexado(
    IReadOnlyDictionary<int, NodoComercial> PorId,
    ILookup<int?, NodoComercial> HijosDe,
    IReadOnlyList<NodoComercial> Raices,
    int ProfundidadHoja);

/// <summary>Un paso del recorrido en cascada.</summary>
/// <param name="Nodo">Nodo visitado.</param>
/// <param name="Valor">Valor heredado: valor del padre x porcentaje del nodo (sin redondeo).</param>
/// <param name="Camino">Nodos desde la raiz hasta este nodo, inclusive.</param>
public sealed record NodoRecorrido(NodoComercial Nodo, decimal Valor, IReadOnlyList<NodoComercial> Camino);

/// <summary>
/// Operaciones puras sobre el arbol comercial. Recibe los nodos como los entrega el repositorio
/// (con <c>ConceptoComercial.NivelJerarquia</c> cargado) y no depende de <c>NodoPadre</c>/<c>NodosHijos</c>.
/// </summary>
public static class ArbolComercialHelper
{
    public const string SeparadorRuta = " > ";

    public static ArbolIndexado Indexar(IReadOnlyList<NodoComercial> nodos)
    {
        var ordenados = nodos.OrderBy(n => n.Id).ToList();
        var hijosDe = ordenados.ToLookup(n => n.NodoPadreId);
        var profundidadHoja = ordenados.Count == 0
            ? 0
            : ValidadorArbolComercial.ObtenerProfundidadHoja(ordenados.Select(n => n.ConceptoComercial.NivelJerarquia));

        return new ArbolIndexado(ordenados.ToDictionary(n => n.Id), hijosDe, hijosDe[null].ToList(), profundidadHoja);
    }

    /// <summary>
    /// Regla 2: recorrido en profundidad (preorden) desde las raices, con
    /// <c>Valor(hijo) = Valor(padre) x PorcentajeParticipacion</c> en <see cref="decimal"/> y sin redondeo.
    /// Con <paramref name="valorRaiz"/> = 1 el valor es el porcentaje acumulado; con los espacios totales de la tienda
    /// son los espacios calculados.
    /// </summary>
    public static IEnumerable<NodoRecorrido> RecorrerEnCascada(ArbolIndexado arbol, decimal valorRaiz)
    {
        var pila = new Stack<NodoRecorrido>();
        foreach (var raiz in arbol.Raices.Reverse())
        {
            pila.Push(new NodoRecorrido(raiz, valorRaiz * raiz.PorcentajeParticipacion, [raiz]));
        }

        while (pila.Count > 0)
        {
            var actual = pila.Pop();
            yield return actual;

            foreach (var hijo in arbol.HijosDe[actual.Nodo.Id].Reverse())
            {
                pila.Push(new NodoRecorrido(hijo, actual.Valor * hijo.PorcentajeParticipacion, [.. actual.Camino, hijo]));
            }
        }
    }

    /// <summary>Ruta legible, ej. <c>"BELLEZA Y BIENESTAR &gt; BEAUTY &gt; LILI BEAUTY"</c>.</summary>
    public static string RutaLegible(IEnumerable<NodoComercial> camino) =>
        string.Join(SeparadorRuta, camino.Select(n => n.ConceptoComercial.Nombre));

    public static bool EsHoja(NodoComercial nodo, int profundidadHoja) =>
        nodo.ConceptoComercial.NivelJerarquia.Profundidad == profundidadHoja;
}
