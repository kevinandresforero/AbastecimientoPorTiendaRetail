namespace Abastecimiento.Dominio.Entidades.Tiendas;

/// <summary>Catalogo de tipos de espacio fisico (Isla, Casa David, Mesa, Perimetral...).</summary>
public class TipoEspacio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public CategoriaEspacio Categoria { get; set; }
}
