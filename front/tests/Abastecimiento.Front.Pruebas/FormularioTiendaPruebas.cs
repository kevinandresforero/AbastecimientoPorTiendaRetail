using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Componentes.Pages;
using Abastecimiento.Front.Pruebas.Apoyo;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Front.Pruebas;

public class FormularioTiendaPruebas : TestContext
{
    private readonly TiendasFalsas tiendas = new();

    public FormularioTiendaPruebas()
    {
        Services.AddSingleton<IClienteTiendas>(tiendas);
        Services.AddSingleton<IClienteCatalogos>(new CatalogosFalsos());
        Services.AddSingleton<IClienteTiposEspacio>(new TiposEspacioFalsos());
    }

    private IRenderedComponent<NuevaTienda> RenderizarConDatosBasicos()
    {
        var cut = RenderComponent<NuevaTienda>();
        cut.Find("#codigo-hanna").Change("9001");
        cut.Find("#nombre").Change("  TIENDA PRUEBA  ");
        cut.Find("#marca").Change("1");
        cut.Find("#regional").Change("1");
        cut.Find("#tipo-tienda").Change("1");
        return cut;
    }

    private static void LlenarEspacios(IRenderedComponent<NuevaTienda> cut, params (int Tipo, int Cantidad)[] filas)
    {
        for (var i = 0; i < filas.Length; i++)
        {
            if (i > 0)
            {
                cut.Find("#agregar-espacio").Click();
            }

            cut.Find($"#espacio-tipo-{i}").Change(filas[i].Tipo.ToString(System.Globalization.CultureInfo.InvariantCulture));
            cut.Find($"#espacio-cantidad-{i}").Change(filas[i].Cantidad.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    [Fact]
    public void Cuatro_tipos_de_mobiliario_muestran_el_mensaje_y_no_se_envia()
    {
        var cut = RenderizarConDatosBasicos();
        LlenarEspacios(cut, (1, 7), (2, 4), (3, 4), (4, 2));

        Assert.Equal("4 de 3", cut.Find("#tipos-mobiliario").TextContent.Trim());
        cut.Find("form").Submit();

        Assert.Contains("máximo 3 tipos de mobiliario", cut.Markup);
        Assert.Empty(tiendas.Creadas);
    }

    [Fact]
    public void Tipo_de_espacio_repetido_muestra_el_mensaje_y_no_se_envia()
    {
        var cut = RenderizarConDatosBasicos();
        LlenarEspacios(cut, (1, 7), (1, 3));

        cut.Find("form").Submit();

        Assert.Contains("no puede repetirse: Isla", cut.Markup);
        Assert.Empty(tiendas.Creadas);
    }

    [Fact]
    public void Formulario_valido_llama_a_CrearAsync_una_vez_y_navega_al_detalle()
    {
        var cut = RenderizarConDatosBasicos();
        LlenarEspacios(cut, (1, 7), (2, 4), (3, 4), (5, 7));

        Assert.Equal("22", cut.Find("#espacios-totales").TextContent.Trim());
        Assert.Equal("3 de 3", cut.Find("#tipos-mobiliario").TextContent.Trim());
        cut.Find("form").Submit();

        var solicitud = Assert.Single(tiendas.Creadas);
        Assert.Equal(9001, solicitud.CodigoHanna);
        Assert.Equal("TIENDA PRUEBA", solicitud.Nombre);
        Assert.Equal(4, solicitud.Espacios.Count);
        Assert.EndsWith("/tiendas/9001?creada=true", Services.GetRequiredService<FakeNavigationManager>().Uri);
    }

    [Fact]
    public void Formulario_vacio_no_se_envia()
    {
        var cut = RenderComponent<NuevaTienda>();

        cut.Find("form").Submit();

        Assert.Contains("Ingrese el código HANNA.", cut.Markup);
        Assert.Contains("Seleccione el tipo de espacio.", cut.Markup);
        Assert.Empty(tiendas.Creadas);
    }

    [Fact]
    public void Error_400_de_la_API_aparece_junto_al_campo_Nombre()
    {
        tiendas.ErrorAlCrear = new ExcepcionApi(400, "One or more validation errors occurred.",
            erroresPorCampo: new Dictionary<string, string[]> { ["Nombre"] = ["El nombre no es válido para la API."] });
        var cut = RenderizarConDatosBasicos();
        LlenarEspacios(cut, (1, 7));

        cut.Find("form").Submit();

        Assert.Contains("El nombre no es válido para la API.", cut.Find("#campo-nombre .validation-message").TextContent);
    }

    [Fact]
    public void Error_409_de_la_API_aparece_en_el_campo_codigo_HANNA()
    {
        tiendas.ErrorAlCrear = new ExcepcionApi(409, "Conflicto", "Ya existe una tienda con el codigo HANNA 9001.");
        var cut = RenderizarConDatosBasicos();
        LlenarEspacios(cut, (1, 7));

        cut.Find("form").Submit();

        Assert.Contains("Ya existe una tienda con el codigo HANNA 9001.", cut.Find("#campo-codigo .validation-message").TextContent);
    }

    [Fact]
    public void Error_400_por_fila_de_espacios_aparece_junto_a_la_cantidad()
    {
        tiendas.ErrorAlCrear = new ExcepcionApi(400, "One or more validation errors occurred.",
            erroresPorCampo: new Dictionary<string, string[]> { ["Espacios[0].CantidadEspacios"] = ["Cantidad rechazada."] });
        var cut = RenderizarConDatosBasicos();
        LlenarEspacios(cut, (1, 7));

        cut.Find("form").Submit();

        var fila = cut.FindAll(".fila-espacio")[0];
        Assert.Contains("Cantidad rechazada.", fila.TextContent);
    }

    [Fact]
    public void Ruta_de_detalle_no_choca_con_nueva()
    {
        var atributo = typeof(DetalleTienda).GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>().Single();
        Assert.Equal("/tiendas/{CodigoHanna:int}", atributo.Template);
    }
}
