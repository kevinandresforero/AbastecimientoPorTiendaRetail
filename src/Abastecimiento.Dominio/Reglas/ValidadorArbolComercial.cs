using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Excepciones;

namespace Abastecimiento.Dominio.Reglas;

/// <summary>
/// Reglas del arbol comercial que cruzan filas (no expresables como CHECK en la BD).
/// Los nodos recibidos deben traer cargados <c>ConceptoComercial.NivelJerarquia</c>.
/// </summary>
public static class ValidadorArbolComercial
{
    /// <summary>Profundidad del nivel hoja: la mayor profundidad definida (derivada, no se persiste).</summary>
    public static int ObtenerProfundidadHoja(IEnumerable<NivelJerarquia> niveles) =>
        niveles.Select(n => n.Profundidad).DefaultIfEmpty(0).Max();

    /// <summary>Regla 2: nivel hoja exige UnidadesPorEspacio (> 0); otros niveles no lo admiten.</summary>
    public static void ValidarConcepto(ConceptoComercial concepto, NivelJerarquia nivel, int profundidadHoja)
    {
        var errores = ErroresConcepto(concepto, nivel, profundidadHoja).ToList();
        if (errores.Count > 0)
        {
            throw new ValidacionDominioExcepcion(errores);
        }
    }

    /// <summary>
    /// Valida un nodo nuevo o modificado frente a su padre y sus hermanos
    /// (Regla 1: profundidad; Regla 3: suma de porcentajes de hermanos menor o igual a 1;
    /// Regla 5: padre e hijo de la misma tienda).
    /// </summary>
    /// <param name="nodo">Nodo a validar, con concepto y nivel cargados.</param>
    /// <param name="padre">Nodo padre con concepto y nivel cargados, o null si es raiz (unidad de negocio).</param>
    /// <param name="hermanos">
    /// Nodos de la misma tienda con el mismo padre (para una raiz: las demas unidades de negocio de la tienda),
    /// excluyendo al propio nodo.
    /// </param>
    public static void ValidarNodo(NodoComercial nodo, NodoComercial? padre, IEnumerable<NodoComercial> hermanos)
    {
        var errores = new List<string>();

        if (nodo.PorcentajeParticipacion <= 0m || nodo.PorcentajeParticipacion > 1m)
        {
            errores.Add("El porcentaje de participacion debe ser mayor que 0 y menor o igual a 1.");
        }

        errores.AddRange(ErroresTienda(nodo, padre));
        errores.AddRange(ErroresProfundidad(nodo, padre));

        var suma = hermanos.Sum(h => h.PorcentajeParticipacion) + nodo.PorcentajeParticipacion;
        if (suma > 1m)
        {
            errores.Add($"La suma de porcentajes de los nodos hermanos ({suma}) supera 1.");
        }

        if (errores.Count > 0)
        {
            throw new ValidacionDominioExcepcion(errores);
        }
    }

    /// <summary>
    /// Valida uno o varios arboles (reglas 1, 2, 3 y 5) y devuelve todos los errores encontrados. Acepta los nodos de
    /// varias tiendas: los hermanos se agrupan por (TiendaId, NodoPadreId).
    /// </summary>
    public static IReadOnlyList<string> ValidarArbol(IReadOnlyCollection<NodoComercial> nodos, int profundidadHoja)
    {
        var errores = new List<string>();
        var porId = nodos.ToDictionary(n => n.Id);

        foreach (var nodo in nodos)
        {
            NodoComercial? padre = null;
            if (nodo.NodoPadreId is int padreId && !porId.TryGetValue(padreId, out padre))
            {
                errores.Add($"Nodo {nodo.Id}: el padre {padreId} no existe.");
                continue;
            }

            errores.AddRange(ErroresTienda(nodo, padre).Select(e => $"Nodo {nodo.Id}: {e}"));
            errores.AddRange(ErroresProfundidad(nodo, padre));
            errores.AddRange(ErroresConcepto(nodo.ConceptoComercial, nodo.ConceptoComercial.NivelJerarquia, profundidadHoja)
                .Select(e => $"Nodo {nodo.Id}: {e}"));
        }

        foreach (var grupo in nodos.GroupBy(n => (n.TiendaId, n.NodoPadreId)))
        {
            var suma = grupo.Sum(n => n.PorcentajeParticipacion);
            if (suma > 1m)
            {
                var hermanos = grupo.Key.NodoPadreId is int id ? $"los hijos del nodo {id}" : "las unidades de negocio";
                errores.Add($"Tienda {grupo.Key.TiendaId}: {hermanos} suman {suma} (> 1).");
            }
        }

        return errores;
    }

    /// <summary>Regla 5: padre e hijo pertenecen a la misma tienda.</summary>
    private static IEnumerable<string> ErroresTienda(NodoComercial nodo, NodoComercial? padre)
    {
        if (padre is not null && padre.TiendaId != nodo.TiendaId)
        {
            yield return $"El nodo pertenece a la tienda {nodo.TiendaId} y su padre a la tienda {padre.TiendaId}; deben ser la misma.";
        }
    }

    private static IEnumerable<string> ErroresProfundidad(NodoComercial nodo, NodoComercial? padre)
    {
        var profundidad = nodo.ConceptoComercial.NivelJerarquia.Profundidad;

        if (padre is null)
        {
            if (profundidad != 1)
            {
                yield return $"Un nodo raiz debe ser de profundidad 1 (recibido: {profundidad}).";
            }

            yield break;
        }

        var esperada = padre.ConceptoComercial.NivelJerarquia.Profundidad + 1;
        if (profundidad != esperada)
        {
            yield return $"El nodo debe ser de profundidad {esperada} (padre + 1); su concepto es de profundidad {profundidad}.";
        }
    }

    private static IEnumerable<string> ErroresConcepto(ConceptoComercial concepto, NivelJerarquia nivel, int profundidadHoja)
    {
        var esHoja = nivel.Profundidad == profundidadHoja;

        if (esHoja && concepto.UnidadesPorEspacio is null or <= 0)
        {
            yield return $"El concepto '{concepto.Nombre}' es de nivel hoja y requiere UnidadesPorEspacio mayor que 0.";
        }

        if (!esHoja && concepto.UnidadesPorEspacio is not null)
        {
            yield return $"El concepto '{concepto.Nombre}' no es de nivel hoja y no admite UnidadesPorEspacio.";
        }
    }
}
