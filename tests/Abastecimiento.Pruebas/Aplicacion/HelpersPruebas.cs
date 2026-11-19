using System.Globalization;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Pruebas.Datos;

namespace Abastecimiento.Pruebas.Aplicacion;

public class HelpersPruebas
{
    [Fact]
    public void PorcentajeAcumuladoDeLabialesEs0_0564()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var arbol = ArbolComercialHelper.Indexar(nodos, 7);

        var labiales = ArbolComercialHelper.RecorrerEnCascada(arbol, 1m).Single(r => r.Nodo.Id == 10);

        // 1 x 0.30 x 0.94 x 1 x 1 x 0.40 x 0.50
        Assert.Equal(0.0564m, labiales.Valor);
    }

    [Fact]
    public void RutaLegibleUneLosNombresDesdeLaRaiz()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var arbol = ArbolComercialHelper.Indexar(nodos, 7);

        var labiales = ArbolComercialHelper.RecorrerEnCascada(arbol, 1m).Single(r => r.Nodo.Id == 10);

        Assert.Equal(
            "BELLEZA Y BIENESTAR > BEAUTY > LILI BEAUTY > GIRLS > MAKEUP > LABIOS > LABIALES",
            ArbolComercialHelper.RutaLegible(labiales.Camino));
    }

    [Fact]
    public void ElRecorridoEsEnPreorden()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var arbol = ArbolComercialHelper.Indexar(nodos, 7);

        var orden = ArbolComercialHelper.RecorrerEnCascada(arbol, 1m).Select(r => r.Nodo.Id).ToArray();

        Assert.Equal(new[] { 1, 2, 3, 5, 6, 7, 10, 11, 8, 12, 13, 9, 14, 4, 15, 16, 17, 18 }, orden);
    }

    [Fact]
    public void IndexarUsaLaProfundidadHojaDeLosNiveles()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();

        var arbol = ArbolComercialHelper.Indexar(nodos, 7);

        Assert.Equal(7, arbol.ProfundidadHoja);
        Assert.Equal(1, Assert.Single(arbol.Raices).Id);
    }

    [Theory]
    [InlineData("818.928", MidpointRounding.AwayFromZero, 819)]
    [InlineData("237.6", MidpointRounding.AwayFromZero, 238)]
    [InlineData("2.5", MidpointRounding.AwayFromZero, 3)]
    [InlineData("2.5", MidpointRounding.ToEven, 2)]
    public void RedondeoSoloAlFinal(string carga, MidpointRounding modo, int esperado)
    {
        var valor = decimal.Parse(carga, CultureInfo.InvariantCulture);

        Assert.Equal(esperado, RedondeoHelper.RedondearCarga(valor, modo));
    }

    [Fact]
    public void NormalizarQuitaCerosSinAlterarElValor()
    {
        var conEscala = 1.240800000000m;

        var normalizado = MapeoHelper.Normalizar(conEscala);

        Assert.Equal(conEscala, normalizado);
        Assert.Equal("1.2408", normalizado.ToString(CultureInfo.InvariantCulture));
    }
}
