using Abastecimiento.Dominio.Entidades.Tiendas;

namespace Abastecimiento.Dominio.Entidades.ArbolComercial;

/// <summary>
/// "Donde esta" un concepto en el arbol comercial de una tienda y con que porcentaje de participacion respecto a su padre.
/// Relacion auto-referenciada (lista de adyacencia). Las raices son las unidades de negocio de la tienda y su porcentaje es
/// la fraccion de los espacios de la tienda que reciben.
/// </summary>
public class NodoComercial
{
    public int Id { get; set; }

    /// <summary>
    /// Tienda duena del arbol. Se repite en todos los nodos (no solo en la raiz) para leer el arbol de una tienda con un
    /// filtro por indice; padre e hijo siempre tienen el mismo valor (regla 5 de <c>ValidadorArbolComercial</c>).
    /// </summary>
    public int TiendaId { get; set; }

    public int? NodoPadreId { get; set; }

    public int ConceptoComercialId { get; set; }

    /// <summary>Fraccion respecto al padre (o a los espacios de la tienda, si es raiz): 0.300000 = 30%.</summary>
    public decimal PorcentajeParticipacion { get; set; }

    public Tienda Tienda { get; set; } = null!;

    public NodoComercial? NodoPadre { get; set; }

    public ICollection<NodoComercial> NodosHijos { get; set; } = new List<NodoComercial>();

    public ConceptoComercial ConceptoComercial { get; set; } = null!;
}
