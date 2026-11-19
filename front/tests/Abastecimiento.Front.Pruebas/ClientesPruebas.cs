using System.Net;
using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Pruebas.Apoyo;

namespace Abastecimiento.Front.Pruebas;

public class ClientesPruebas
{
    [Fact]
    public async Task Deserializa_camelCase_y_enums_en_texto()
    {
        var manejador = new ManejadorFalso(_ => ManejadorFalso.Json(HttpStatusCode.OK,
            """[{"id":1,"nombre":"Isla","categoria":"mobiliario"},{"id":7,"nombre":"Perimetral","categoria":"perimetral"}]"""));
        var cliente = new ClienteTiposEspacio(ManejadorFalso.Cliente(manejador));

        var tipos = await cliente.ListarAsync(CancellationToken.None);

        Assert.Equal(2, tipos.Count);
        Assert.Equal(new TipoEspacioRespuesta(1, "Isla", CategoriaEspacio.Mobiliario), tipos[0]);
        Assert.Equal(CategoriaEspacio.Perimetral, tipos[1].Categoria);
        Assert.Equal("api/tipos-espacio", manejador.Recibidas.Single().Solicitud.RequestUri!.PathAndQuery.TrimStart('/'));
    }

    [Fact]
    public async Task Consolidado_envia_agruparPor_en_camelCase()
    {
        var manejador = new ManejadorFalso(_ => ManejadorFalso.Json(HttpStatusCode.OK,
            """
            {"agruparPor":"marcaTienda","nivel":null,
             "grupos":[{"clave":"1","nombre":"LILI PINK","cantidadTiendas":1,"espaciosTotales":30,"espaciosAsignados":9,"cargaTotal":100}],
             "total":{"clave":"TOTAL","nombre":"Total","cantidadTiendas":1,"espaciosTotales":30,"espaciosAsignados":9,"cargaTotal":100}}
            """));
        var cliente = new ClienteReportes(ManejadorFalso.Cliente(manejador));

        var consolidado = await cliente.ConsolidadoAsync(AgruparPor.MarcaTienda, null, CancellationToken.None);

        Assert.Equal("/api/reportes/consolidado?agruparPor=marcaTienda", manejador.Recibidas.Single().Solicitud.RequestUri!.PathAndQuery);
        Assert.Equal(AgruparPor.MarcaTienda, consolidado.AgruparPor);
        Assert.Equal(100, consolidado.Total.CargaTotal);
    }

    [Fact]
    public async Task Consolidado_por_nivel_envia_nivelId()
    {
        var manejador = new ManejadorFalso(_ => ManejadorFalso.Json(HttpStatusCode.OK,
            """{"agruparPor":"nivel","nivel":{"id":3,"nombre":"Marca de Producto","profundidad":3,"esHoja":false},"grupos":[],"total":{"clave":"TOTAL","nombre":"Total","cantidadTiendas":0,"espaciosTotales":null,"espaciosAsignados":0,"cargaTotal":0}}"""));
        var cliente = new ClienteReportes(ManejadorFalso.Cliente(manejador));

        var consolidado = await cliente.ConsolidadoAsync(AgruparPor.Nivel, 3, CancellationToken.None);

        Assert.Equal("/api/reportes/consolidado?agruparPor=nivel&nivelId=3", manejador.Recibidas.Single().Solicitud.RequestUri!.PathAndQuery);
        Assert.Null(consolidado.Total.EspaciosTotales);
    }

    [Fact]
    public async Task Respuesta_404_problem_json_lanza_ExcepcionApi_con_estado_y_detalle()
    {
        var manejador = new ManejadorFalso(_ => ManejadorFalso.Problema(HttpStatusCode.NotFound,
            """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Recurso no encontrado","status":404,"detail":"No existe una tienda con codigo HANNA 9999."}"""));
        var cliente = new ClienteCarga(ManejadorFalso.Cliente(manejador));

        var error = await Assert.ThrowsAsync<ExcepcionApi>(() => cliente.CalcularAsync(9999, CancellationToken.None));

        Assert.Equal(404, error.Estado);
        Assert.True(error.EsNoEncontrado);
        Assert.Equal("Recurso no encontrado", error.Titulo);
        Assert.Equal("No existe una tienda con codigo HANNA 9999.", error.Detalle);
    }

    [Fact]
    public async Task Respuesta_400_con_errors_llena_ErroresPorCampo()
    {
        var manejador = new ManejadorFalso(_ => ManejadorFalso.Problema(HttpStatusCode.BadRequest,
            """{"title":"One or more validation errors occurred.","status":400,"errors":{"Nombre":["The Nombre field is required."],"Espacios[0].CantidadEspacios":["Debe ser >= 0"]}}"""));
        var cliente = new ClienteTiendas(ManejadorFalso.Cliente(manejador));

        var error = await Assert.ThrowsAsync<ExcepcionApi>(() => cliente.CrearAsync(
            new CrearTiendaSolicitud(1, "", 1, 1, 1, [new EspacioSolicitud(1, -1)]), CancellationToken.None));

        Assert.Equal(400, error.Estado);
        Assert.Equal(["The Nombre field is required."], error.ErroresPorCampo["Nombre"]);
        Assert.True(error.ErroresPorCampo.ContainsKey("Espacios[0].CantidadEspacios"));
        Assert.Empty(error.Errores);
    }

    [Fact]
    public async Task Respuesta_400_con_errores_de_negocio_llena_Errores()
    {
        var manejador = new ManejadorFalso(_ => ManejadorFalso.Problema(HttpStatusCode.BadRequest,
            """{"title":"Regla de negocio violada","status":400,"detail":"El arbol comercial no es valido.","errores":["La suma de porcentajes de los nodos hermanos (1.010000) supera 1."]}"""));
        var cliente = new ClienteArbolComercial(ManejadorFalso.Cliente(manejador));

        var error = await Assert.ThrowsAsync<ExcepcionApi>(() => cliente.CrearNodoAsync(
            new CrearNodoSolicitud(6, 9, 0.5m), CancellationToken.None));

        Assert.Equal(400, error.Estado);
        Assert.Equal("La suma de porcentajes de los nodos hermanos (1.010000) supera 1.", Assert.Single(error.Errores));
        Assert.Empty(error.ErroresPorCampo);
    }

    [Fact]
    public async Task Error_de_red_lanza_ExcepcionApi_con_estado_0()
    {
        var manejador = new ManejadorFalso(_ => throw new HttpRequestException("Connection refused"));
        var cliente = new ClienteTiendas(ManejadorFalso.Cliente(manejador));

        var error = await Assert.ThrowsAsync<ExcepcionApi>(() => cliente.ListarAsync(null, null, CancellationToken.None));

        Assert.Equal(0, error.Estado);
        Assert.Equal("No se pudo conectar con la API", error.Message);
    }

    [Fact]
    public async Task Cancelacion_del_llamador_se_propaga()
    {
        using var cancelacion = new CancellationTokenSource();
        await cancelacion.CancelAsync();
        var manejador = new ManejadorFalso(_ => ManejadorFalso.Json(HttpStatusCode.OK, "[]"));
        var cliente = new ClienteTiendas(ManejadorFalso.Cliente(manejador));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cliente.ListarAsync(null, null, cancelacion.Token));
    }

    [Fact]
    public async Task Envia_porcentaje_como_fraccion_y_filtros_de_tiendas_en_la_query()
    {
        var manejador = new ManejadorFalso(solicitud => solicitud.Method == HttpMethod.Put
            ? ManejadorFalso.Json(HttpStatusCode.OK,
                """{"nodoId":7,"nodoPadreId":6,"conceptoComercialId":7,"nombre":"LABIOS","nivel":"Perfil","profundidad":6,"porcentajeParticipacion":0.3,"porcentajeAcumulado":0.0846,"ruta":"A > B","unidadesPorEspacio":null,"esHoja":false}""")
            : ManejadorFalso.Json(HttpStatusCode.OK, "[]"));
        var http = ManejadorFalso.Cliente(manejador);

        await new ClienteArbolComercial(http).ActualizarPorcentajeAsync(7, 0.3m, CancellationToken.None);
        await new ClienteTiendas(http).ListarAsync(2, 1, CancellationToken.None);

        var (put, cuerpo) = manejador.Recibidas[0];
        Assert.Equal("/api/nodos-comerciales/7/porcentaje", put.RequestUri!.PathAndQuery);
        Assert.Equal("""{"porcentajeParticipacion":0.3}""", cuerpo);
        Assert.Equal("/api/tiendas?marcaTiendaId=2&regionalId=1", manejador.Recibidas[1].Solicitud.RequestUri!.PathAndQuery);
    }
}
