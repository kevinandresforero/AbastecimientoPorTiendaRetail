using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Componentes.Compartidos;
using Abastecimiento.Front.Formularios;

namespace Abastecimiento.Front.Pruebas;

public class ErroresApiPruebas : TestContext
{
    [Fact]
    public void MensajeError_muestra_titulo_detalle_y_errores()
    {
        var error = new ExcepcionApi(400, "Regla de negocio violada", "El arbol no es valido.",
            ["La suma de porcentajes de los nodos hermanos (1.010000) supera 1."]);

        var cut = RenderComponent<MensajeError>(p => p.Add(c => c.Error, error));

        Assert.Contains("Error: Regla de negocio violada", cut.Markup);
        Assert.Contains("El arbol no es valido.", cut.Markup);
        Assert.Equal("La suma de porcentajes de los nodos hermanos (1.010000) supera 1.", cut.Find("li").TextContent);
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void MensajeError_con_reintentar_invoca_la_accion()
    {
        var reintentos = 0;
        var cut = RenderComponent<MensajeError>(p => p
            .Add(c => c.Error, new ExcepcionApi(0, ExcepcionApi.MensajeSinConexion))
            .Add(c => c.AlReintentar, () => reintentos++));

        cut.Find("button").Click();

        Assert.Equal(1, reintentos);
        Assert.Contains("No se pudo conectar con la API", cut.Markup);
    }

    [Fact]
    public void MensajeError_sin_error_no_muestra_nada()
    {
        var cut = RenderComponent<MensajeError>();
        Assert.Equal(string.Empty, cut.Markup.Trim());
    }

    [Fact]
    public void ResumenErroresApi_ubica_claves_PascalCase_en_el_modelo()
    {
        var formulario = new FormularioTienda();
        formulario.Espacios.Add(new FilaEspacio());

        var nombre = ResumenErroresApi.BuscarCampo(formulario, "Nombre");
        var cantidad = ResumenErroresApi.BuscarCampo(formulario, "Espacios[1].CantidadEspacios");

        Assert.Equal(nameof(FormularioTienda.Nombre), nombre!.Value.FieldName);
        Assert.Same(formulario, nombre.Value.Model);
        Assert.Same(formulario.Espacios[1], cantidad!.Value.Model);
        Assert.Equal(nameof(FilaEspacio.CantidadEspacios), cantidad.Value.FieldName);
        Assert.Null(ResumenErroresApi.BuscarCampo(formulario, "Espacios[5].CantidadEspacios"));
        Assert.Null(ResumenErroresApi.BuscarCampo(formulario, "NoExiste"));
    }
}
