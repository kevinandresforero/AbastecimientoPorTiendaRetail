namespace Abastecimiento.Dominio.Entidades.Tiendas;

/// <summary>Tienda fisica con su capacidad instalada de exhibicion.</summary>
public class Tienda
{
    public int Id { get; set; }

    public int CodigoHanna { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int MarcaTiendaId { get; set; }

    public int RegionalId { get; set; }

    public int TipoTiendaId { get; set; }

    public int? ResponsableVisualId { get; set; }

    public MarcaTienda MarcaTienda { get; set; } = null!;

    public Regional Regional { get; set; } = null!;

    public TipoTienda TipoTienda { get; set; } = null!;

    public ResponsableVisual? ResponsableVisual { get; set; }

    public ICollection<EspacioTienda> Espacios { get; set; } = new List<EspacioTienda>();

    /// <summary>Regla 1: suma de todos los espacios (mobiliario + perimetrales). Derivado, no se persiste.</summary>
    public int EspaciosTotales => Espacios.Sum(e => e.CantidadEspacios);
}
