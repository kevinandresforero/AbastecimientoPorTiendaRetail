using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Infraestructura.Persistencia.Semilla;

namespace Abastecimiento.Pruebas.Datos;

/// <summary>Catalogos y tiendas de la semilla en memoria, con navegaciones enlazadas (como las entrega el repositorio).</summary>
internal sealed class TiendasDePrueba
{
    public List<MarcaTienda> MarcasTienda { get; } = DatosSemilla.MarcasTienda.ToList();

    public List<Regional> Regionales { get; } = DatosSemilla.Regionales.ToList();

    public List<TipoTienda> TiposTienda { get; } = DatosSemilla.TiposTienda.ToList();

    public List<ResponsableVisual> ResponsablesVisuales { get; } = [];

    public List<TipoEspacio> TiposEspacio { get; } = DatosSemilla.TiposEspacio.ToList();

    public List<Tienda> Tiendas { get; } = [];

    public TiendasDePrueba()
    {
        var espacios = DatosSemilla.EspaciosTienda;
        foreach (var tienda in DatosSemilla.Tiendas)
        {
            tienda.Espacios = espacios.Where(e => e.TiendaId == tienda.Id).ToList();
            Enlazar(tienda);
            Tiendas.Add(tienda);
        }
    }

    /// <summary>Asigna las navegaciones de catalogos y tipos de espacio a partir de las FK.</summary>
    public void Enlazar(Tienda tienda)
    {
        tienda.MarcaTienda = MarcasTienda.Single(m => m.Id == tienda.MarcaTiendaId);
        tienda.Regional = Regionales.Single(r => r.Id == tienda.RegionalId);
        tienda.TipoTienda = TiposTienda.Single(t => t.Id == tienda.TipoTiendaId);
        tienda.ResponsableVisual = ResponsablesVisuales.SingleOrDefault(r => r.Id == tienda.ResponsableVisualId);
        foreach (var espacio in tienda.Espacios)
        {
            espacio.TipoEspacio = TiposEspacio.Single(t => t.Id == espacio.TipoEspacioId);
        }
    }

    public int EspaciosTotales(int codigoHanna) => Tiendas.Single(t => t.CodigoHanna == codigoHanna).EspaciosTotales;
}
