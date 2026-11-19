namespace Abastecimiento.Dominio.Entidades.ArbolComercial;

/// <summary>
/// "Donde esta" un concepto en el arbol y con que porcentaje de participacion respecto a su padre.
/// Relacion auto-referenciada (lista de adyacencia).
/// </summary>
public class NodoComercial
{
    public int Id { get; set; }

    public int? NodoPadreId { get; set; }

    public int ConceptoComercialId { get; set; }

    /// <summary>Fraccion respecto al padre: 0.300000 = 30%.</summary>
    public decimal PorcentajeParticipacion { get; set; }

    public NodoComercial? NodoPadre { get; set; }

    public ICollection<NodoComercial> NodosHijos { get; set; } = new List<NodoComercial>();

    public ConceptoComercial ConceptoComercial { get; set; } = null!;
}
