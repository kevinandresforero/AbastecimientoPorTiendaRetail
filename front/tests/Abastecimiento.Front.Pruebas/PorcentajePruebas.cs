using System.ComponentModel.DataAnnotations;
using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Componentes.Arbol;
using Abastecimiento.Front.Formularios;
using Abastecimiento.Front.Pruebas.Apoyo;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Front.Pruebas;

public class PorcentajePruebas : TestContext
{
    private readonly ArbolFalso arbol = new();

    public PorcentajePruebas() => Services.AddSingleton<IClienteArbolComercial>(arbol);

    private static bool EsValido(string? texto)
    {
        var formulario = new FormularioPorcentaje { Porcentaje = texto };
        return Validator.TryValidateObject(formulario, new ValidationContext(formulario), [], validateAllProperties: true);
    }

    [Theory]
    [InlineData("30", 0.30)]
    [InlineData("30,5", 0.305)]
    [InlineData("30.5", 0.305)]
    [InlineData("12,3456", 0.123456)]
    [InlineData("100", 1)]
    public void Convierte_el_porcentaje_escrito_a_fraccion(string texto, double esperado)
    {
        Assert.True(EsValido(texto));
        Assert.Equal((decimal)esperado, LectorPorcentaje.AFraccion(texto));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("101")]
    [InlineData("-5")]
    [InlineData("30,12345")]
    [InlineData("abc")]
    [InlineData("1.000,5")]
    [InlineData("")]
    [InlineData(null)]
    public void Valores_fuera_de_rango_o_mal_escritos_son_invalidos(string? texto) =>
        Assert.False(EsValido(texto));

    [Fact]
    public void Editar_porcentaje_30_envia_0_30()
    {
        var cut = RenderComponent<DialogoEditarPorcentaje>(p => p
            .Add(c => c.Nodo, ArbolFalso.Nodo(7, 0.4m))
            .Add(c => c.SumaHermanos, 0.6m));

        Assert.Equal("40", cut.Find("#porcentaje-editar").GetAttribute("value"));
        cut.Find("#porcentaje-editar").Change("30");
        cut.Find("form").Submit();

        var (nodoId, porcentaje) = Assert.Single(arbol.PorcentajesActualizados);
        Assert.Equal(7, nodoId);
        Assert.Equal(0.30m, porcentaje);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("101")]
    public void Editar_porcentaje_invalido_no_llama_a_la_API(string texto)
    {
        var cut = RenderComponent<DialogoEditarPorcentaje>(p => p.Add(c => c.Nodo, ArbolFalso.Nodo(7, 0.4m)));

        cut.Find("#porcentaje-editar").Change(texto);
        cut.Find("form").Submit();

        Assert.Contains("mayor que 0 y menor o igual a 100", cut.Markup);
        Assert.Empty(arbol.PorcentajesActualizados);
    }

    [Fact]
    public void Muestra_la_suma_de_hermanos_y_lo_disponible()
    {
        var cut = RenderComponent<DialogoEditarPorcentaje>(p => p
            .Add(c => c.Nodo, ArbolFalso.Nodo(7, 0.4m))
            .Add(c => c.SumaHermanos, 0.6m));

        var ayuda = cut.Find(".ayuda-porcentaje").TextContent;
        Assert.Contains("60,0000 %", ayuda);
        Assert.Contains("40,0000 %", ayuda);
    }
}
