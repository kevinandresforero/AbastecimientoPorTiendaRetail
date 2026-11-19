using System.Globalization;
using Abastecimiento.Aplicacion.Configuracion;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Pruebas.Datos;
using Microsoft.Extensions.Options;

namespace Abastecimiento.Pruebas.Aplicacion;

/// <summary>Conversion entidad / resultado del motor → DTO.</summary>
public class MapeoHelperPruebas
{
    [Fact]
    public void TiendaRespuestaIncluyeCatalogosEspaciosYEspaciosTotales()
    {
        var tienda = new TiendasDePrueba().Tiendas.Single(t => t.CodigoHanna == 5503);

        var respuesta = MapeoHelper.ATiendaRespuesta(tienda);

        Assert.Equal("YAHAD", respuesta.MarcaTienda.Nombre);
        Assert.Equal("BOGOTA", respuesta.Regional.Nombre);
        Assert.Null(respuesta.ResponsableVisual);
        Assert.Equal(22, respuesta.EspaciosTotales);
        Assert.Equal(new[] { "Isla", "Casa David", "Mesa", "Perimetral" }, respuesta.Espacios.Select(e => e.TipoEspacio));
        Assert.Equal(CategoriaEspacio.Perimetral, respuesta.Espacios[^1].Categoria);
    }

    [Fact]
    public void CargaRespuestaTraeRutaCompletaYDecimalesSinCerosSobrantes()
    {
        var tienda = new TiendasDePrueba().Tiendas.Single(t => t.CodigoHanna == 5503);
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        // Igual que desde SQL Server decimal(18,6): 0.30 llega como 0.300000.
        foreach (var nodo in nodos)
        {
            nodo.PorcentajeParticipacion = decimal.Parse(nodo.PorcentajeParticipacion.ToString("F6", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        }

        var resultado = new MotorCalculoCarga(Options.Create(new OpcionesCalculo())).Calcular(tienda.EspaciosTotales, nodos, 7);
        var respuesta = MapeoHelper.ACargaTiendaRespuesta(tienda, resultado);

        var labiales = respuesta.Lineas.Single(l => l.TipoProducto == "LABIALES");
        Assert.Equal(7, labiales.Ruta.Count);
        Assert.Equal(("Categoría", "BEAUTY", 0.3m), (labiales.Ruta[1].Nivel, labiales.Ruta[1].Nombre, labiales.Ruta[1].PorcentajeParticipacion));
        Assert.Equal("1.2408", labiales.EspaciosCalculados.ToString(CultureInfo.InvariantCulture));
        Assert.Equal("818.928", labiales.CargaCalculada.ToString(CultureInfo.InvariantCulture));
        Assert.Equal("6.6", respuesta.TotalEspaciosAsignados.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(18, respuesta.Nodos.Count);
        Assert.Equal(2966, respuesta.TotalCarga);
    }
}
