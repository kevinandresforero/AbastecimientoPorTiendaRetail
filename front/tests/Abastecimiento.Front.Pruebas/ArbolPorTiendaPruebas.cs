using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Componentes.Pages;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Pruebas.Apoyo;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Front.Pruebas;

/// <summary>Arbol comercial por tienda: selector, copia solo en tiendas vacias y enlaces desde tienda/nueva tienda.</summary>
public class ArbolPorTiendaPruebas : TestContext
{
    private readonly ArbolFalso arbol = new();
    private readonly TiendasFalsas tiendas = new();

    public ArbolPorTiendaPruebas()
    {
        Services.AddSingleton<IClienteArbolComercial>(arbol);
        Services.AddSingleton<IClienteTiendas>(tiendas);
        Services.AddSingleton<IClienteCatalogos>(new CatalogosFalsos());
        Services.AddSingleton<IClienteTiposEspacio>(new TiposEspacioFalsos());
        Services.AddSingleton<IClienteCarga>(new CargaFalsa());

        tiendas.Listado.AddRange([Tienda(5503), Tienda(1001)]);
        tiendas.PorCodigo[5503] = Tienda(5503);
        tiendas.PorCodigo[1001] = Tienda(1001);
    }

    private static TiendaRespuesta Tienda(int codigoHanna) => new(
        codigoHanna,
        codigoHanna,
        $"TIENDA {codigoHanna}",
        new CatalogoRespuesta(1, "YAHAD"),
        new CatalogoRespuesta(1, "BOGOTA"),
        new CatalogoRespuesta(1, "MIXTA"),
        [],
        22);

    private static NodoArbolRespuesta Nodo(int nodoId, string nombre) => new(
        nodoId, null, nodoId, nombre, "Unidad de Negocio", 1, 1m, 1m, nombre, null, false);

    /// <summary>
    /// Renderiza el arbol navegando antes a la URL: <c>[SupplyParameterFromQuery]</c> solo se llena desde
    /// <see cref="NavigationManager"/> (bUnit rechaza pasarlo como parametro).
    /// </summary>
    private IRenderedComponent<ArbolComercial> RenderizarArbol(int? tienda = null)
    {
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo(tienda is int codigo ? $"arbol?tienda={codigo}" : "arbol");
        return RenderComponent<ArbolComercial>();
    }

    [Fact]
    public void Carga_el_arbol_de_la_tienda_pedida_en_la_url()
    {
        arbol.Arboles[1001] = [Nodo(2001, "BELLEZA Y BIENESTAR")];

        var cut = RenderizarArbol(1001);

        cut.WaitForAssertion(() =>
        {
            Assert.Contains(1001, arbol.ConsultasArbol);
            Assert.Equal("1001", cut.Find("#tienda-arbol").GetAttribute("value"));
            Assert.Contains("BELLEZA Y BIENESTAR", cut.Markup);
        });
    }

    [Fact]
    public void Cambiar_el_selector_consulta_el_arbol_de_la_otra_tienda()
    {
        arbol.Arboles[1001] = [Nodo(2001, "BELLEZA Y BIENESTAR")];
        arbol.Arboles[5503] = [Nodo(1, "BELLEZA Y BIENESTAR DE 5503")];
        var cut = RenderizarArbol(1001);
        cut.WaitForAssertion(() => Assert.Contains("BELLEZA Y BIENESTAR", cut.Markup));

        cut.Find("#tienda-arbol").Change("5503");

        cut.WaitForAssertion(() =>
        {
            Assert.Contains(5503, arbol.ConsultasArbol);
            Assert.Equal("5503", cut.Find("#tienda-arbol").GetAttribute("value"));
            Assert.Contains("BELLEZA Y BIENESTAR DE 5503", cut.Markup);
        });
    }

    [Fact]
    public void Copiar_arbol_solo_aparece_en_tienda_vacia_y_envia_el_origen()
    {
        var cut = RenderizarArbol(1001);
        cut.WaitForAssertion(() => Assert.Contains("no tiene árbol", cut.Markup));

        cut.FindAll("button").Single(b => b.TextContent.Contains("Copiar árbol") && b.ClassList.Contains("btn-outline-primary")).Click();
        cut.Find("#tienda-origen").Change("5503");
        cut.FindAll("button").Single(b => b.TextContent.Contains("Copiar árbol") && b.ClassList.Contains("btn-primary")).Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Equal((1001, 5503), Assert.Single(arbol.CopiasSolicitadas));
            Assert.Contains("Árbol copiado: 18 nodos desde HANNA 5503", cut.Markup);
        });
    }

    [Fact]
    public void No_muestra_copiar_cuando_la_tienda_ya_tiene_arbol()
    {
        arbol.Arboles[1001] = [Nodo(2001, "BELLEZA Y BIENESTAR")];

        var cut = RenderizarArbol(1001);

        cut.WaitForAssertion(() => Assert.Contains("BELLEZA Y BIENESTAR", cut.Markup));
        Assert.DoesNotContain("Copiar árbol", cut.Markup);
    }

    [Fact]
    public void Nueva_tienda_avisa_que_nace_sin_arbol()
    {
        var cut = RenderComponent<NuevaTienda>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("sin árbol comercial", cut.Markup);
            Assert.Contains("href=\"arbol\"", cut.Markup);
        });
    }

    [Fact]
    public void Detalle_de_tienda_enlaza_al_arbol_de_esa_tienda()
    {
        var cut = RenderComponent<DetalleTienda>(p => p.Add(c => c.CodigoHanna, 5503));

        cut.WaitForAssertion(() =>
        {
            var enlace = cut.FindAll("a").Single(a => a.TextContent.Contains("Ver árbol comercial"));
            Assert.Equal("arbol?tienda=5503", enlace.GetAttribute("href"));
        });
    }

    private sealed class CargaFalsa : IClienteCarga
    {
        public Task<CargaTiendaRespuesta> CalcularAsync(int codigoHanna, CancellationToken ct) =>
            Task.FromResult(Datos5503.Carga());
    }
}
