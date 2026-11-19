namespace Abastecimiento.Dominio.Entidades.ArbolComercial;

/// <summary>
/// "Que es" un elemento del arbol (GIRLS, LABIALES...), definido una sola vez por nivel.
/// <see cref="UnidadesPorEspacio"/> solo aplica al nivel hoja (Tipo de Producto).
/// </summary>
public class ConceptoComercial
{
    public int Id { get; set; }

    public int NivelJerarquiaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int? UnidadesPorEspacio { get; set; }

    public NivelJerarquia NivelJerarquia { get; set; } = null!;
}
