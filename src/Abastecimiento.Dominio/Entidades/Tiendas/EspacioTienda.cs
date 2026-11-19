namespace Abastecimiento.Dominio.Entidades.Tiendas;

/// <summary>Cantidad de espacios de un tipo dado en una tienda.</summary>
public class EspacioTienda
{
    public int TiendaId { get; set; }

    public int TipoEspacioId { get; set; }

    public int CantidadEspacios { get; set; }

    public Tienda Tienda { get; set; } = null!;

    public TipoEspacio TipoEspacio { get; set; } = null!;
}
