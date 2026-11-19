using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Reglas;
using Abastecimiento.Infraestructura.Persistencia.Semilla;

namespace Abastecimiento.Pruebas.Dominio;

public class TiendasPruebas
{
    private static List<Tienda> TiendasSemilla()
    {
        var tipos = DatosSemilla.TiposEspacio.ToDictionary(t => t.Id);
        var tiendas = DatosSemilla.Tiendas.ToList();
        foreach (var espacio in DatosSemilla.EspaciosTienda)
        {
            espacio.TipoEspacio = tipos[espacio.TipoEspacioId];
            tiendas.Single(t => t.Id == espacio.TiendaId).Espacios.Add(espacio);
        }

        return tiendas;
    }

    [Theory]
    [InlineData(5503, 22)]
    [InlineData(1001, 30)]
    [InlineData(2045, 20)]
    public void EspaciosTotalesCumplenLaRegla1(int codigoHanna, int esperado)
    {
        var tienda = TiendasSemilla().Single(t => t.CodigoHanna == codigoHanna);

        Assert.Equal(esperado, tienda.EspaciosTotales);
    }

    [Fact]
    public void LasTiendasSemillaCumplenLasReglasDeEspacios()
    {
        foreach (var tienda in TiendasSemilla())
        {
            ReglasEspaciosTienda.Validar(tienda.Espacios.ToList());
        }
    }

    [Fact]
    public void RechazaMasDeTresTiposDeMobiliario()
    {
        var tipos = DatosSemilla.TiposEspacio;
        var espacios = tipos.Where(t => t.Categoria == CategoriaEspacio.Mobiliario).Take(4)
            .Select(t => new EspacioTienda { TipoEspacioId = t.Id, TipoEspacio = t, CantidadEspacios = 2 })
            .ToList();

        var ex = Assert.Throws<ValidacionDominioExcepcion>(() => ReglasEspaciosTienda.Validar(espacios));

        Assert.Contains(ex.Errores, e => e.Contains("maximo 3"));
    }

    [Fact]
    public void ElPerimetralNoCuentaParaElTopeDeMobiliario()
    {
        var tipos = DatosSemilla.TiposEspacio;
        var espacios = tipos.Where(t => t.Categoria == CategoriaEspacio.Mobiliario).Take(3)
            .Append(tipos.Single(t => t.Categoria == CategoriaEspacio.Perimetral))
            .Select(t => new EspacioTienda { TipoEspacioId = t.Id, TipoEspacio = t, CantidadEspacios = 2 })
            .ToList();

        ReglasEspaciosTienda.Validar(espacios);
    }

    [Fact]
    public void RechazaTiposDeEspacioRepetidos()
    {
        var isla = DatosSemilla.TiposEspacio[0];
        var espacios = new List<EspacioTienda>
        {
            new() { TipoEspacioId = isla.Id, TipoEspacio = isla, CantidadEspacios = 1 },
            new() { TipoEspacioId = isla.Id, TipoEspacio = isla, CantidadEspacios = 2 }
        };

        Assert.Throws<ValidacionDominioExcepcion>(() => ReglasEspaciosTienda.Validar(espacios));
    }
}
