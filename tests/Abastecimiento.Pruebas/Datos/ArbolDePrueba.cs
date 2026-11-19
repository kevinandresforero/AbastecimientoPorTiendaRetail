using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Infraestructura.Persistencia.Semilla;

namespace Abastecimiento.Pruebas.Datos;

/// <summary>Construye en memoria el arbol de la semilla con sus navegaciones enlazadas (como lo entrega el repositorio).</summary>
internal static class ArbolDePrueba
{
    public static (List<NodoComercial> Nodos, List<NivelJerarquia> Niveles) DesdeSemilla()
    {
        var niveles = DatosSemilla.NivelesJerarquia.ToList();
        var nivelPorId = niveles.ToDictionary(n => n.Id);

        var conceptos = DatosSemilla.ConceptosComerciales.ToList();
        foreach (var concepto in conceptos)
        {
            concepto.NivelJerarquia = nivelPorId[concepto.NivelJerarquiaId];
        }

        var conceptoPorId = conceptos.ToDictionary(c => c.Id);
        var nodos = DatosSemilla.NodosComerciales.ToList();
        foreach (var nodo in nodos)
        {
            nodo.ConceptoComercial = conceptoPorId[nodo.ConceptoComercialId];
        }

        return (nodos, niveles);
    }
}
