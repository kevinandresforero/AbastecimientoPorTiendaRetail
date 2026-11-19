using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Infraestructura.Persistencia.Semilla;

namespace Abastecimiento.Pruebas.Datos;

/// <summary>Construye en memoria los arboles de la semilla con sus navegaciones enlazadas (como los entrega el repositorio).</summary>
internal static class ArbolDePrueba
{
    /// <summary>
    /// Arbol de una tienda de la semilla (18 nodos). Por defecto la tienda 1 (5503), de modo que las pruebas del motor y
    /// de los servicios siguen calculando la tabla del enunciado (2,966).
    /// </summary>
    public static (List<NodoComercial> Nodos, List<NivelJerarquia> Niveles) DesdeSemilla(int tiendaId = 1) =>
        Enlazar(DatosSemilla.NodosComercialesDeTienda(tiendaId));

    /// <summary>Los arboles de las 3 tiendas de la semilla (54 nodos), como los devuelve <c>ObtenerArbolCompletoAsync</c>.</summary>
    public static (List<NodoComercial> Nodos, List<NivelJerarquia> Niveles) DesdeSemillaTodasLasTiendas() =>
        Enlazar(DatosSemilla.NodosComerciales);

    private static (List<NodoComercial> Nodos, List<NivelJerarquia> Niveles) Enlazar(IEnumerable<NodoComercial> origen)
    {
        var niveles = DatosSemilla.NivelesJerarquia.ToList();
        var nivelPorId = niveles.ToDictionary(n => n.Id);

        var conceptos = DatosSemilla.ConceptosComerciales.ToList();
        foreach (var concepto in conceptos)
        {
            concepto.NivelJerarquia = nivelPorId[concepto.NivelJerarquiaId];
        }

        var conceptoPorId = conceptos.ToDictionary(c => c.Id);
        var nodos = origen.ToList();
        foreach (var nodo in nodos)
        {
            nodo.ConceptoComercial = conceptoPorId[nodo.ConceptoComercialId];
        }

        return (nodos, niveles);
    }
}
