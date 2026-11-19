using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Abastecimiento.Infraestructura.Persistencia;
using Abastecimiento.Pruebas.Infraestructura;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Pruebas.Api;

/// <summary>
/// Levanta la API completa contra la base de datos de <c>ABASTECIMIENTO_PRUEBAS_SQL</c> (ej. <c>Abastecimiento_Pruebas</c>),
/// separada de la base de la aplicacion para no alterar la semilla. La API la migra y la siembra al arrancar.
/// </summary>
public sealed class FabricaApi : WebApplicationFactory<Program>
{
    public FabricaApi()
    {
        // Program lee la cadena antes de construir el host: se pasa como variable de entorno (tiene prioridad
        // sobre appsettings y el cargador del .env no la sobrescribe).
        var cadena = Environment.GetEnvironmentVariable(FactSqlServerAttribute.Variable);
        if (!string.IsNullOrWhiteSpace(cadena))
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__Abastecimiento", cadena);
        }
    }
}

[Collection(ColeccionSqlServer.Nombre)]
public class ApiPruebas(FabricaApi fabrica) : IClassFixture<FabricaApi>
{
    private readonly HttpClient cliente = fabrica.CreateClient();

    [FactSqlServer]
    public async Task CargaDe5503Devuelve2966Unidades()
    {
        var respuesta = await cliente.GetAsync("/api/carga/5503");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal(2966, json.RootElement.GetProperty("totalCarga").GetInt32());
        Assert.Equal(6.6m, json.RootElement.GetProperty("totalEspaciosAsignados").GetDecimal());
        Assert.Equal(6, json.RootElement.GetProperty("lineas").GetArrayLength());
    }

    [FactSqlServer]
    public async Task CargaDeTiendaInexistenteDevuelve404ProblemDetails()
    {
        var respuesta = await cliente.GetAsync("/api/carga/9999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain("   at ", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>Rango de codigos HANNA reservado a las pruebas; se borra al terminar para no alterar la semilla.</summary>
    private const int CodigoHannaMinimoPruebas = 100_000;

    /// <summary>Borra primero los nodos: la FK de nodos a tiendas es Restrict.</summary>
    private async Task LimpiarTiendasDePruebaAsync()
    {
        await using var alcance = fabrica.Services.CreateAsyncScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<AbastecimientoDbContext>();
        var ids = await contexto.Tiendas
            .Where(t => t.CodigoHanna >= CodigoHannaMinimoPruebas)
            .Select(t => t.Id)
            .ToListAsync();
        await contexto.NodosComerciales.Where(n => ids.Contains(n.TiendaId)).ExecuteDeleteAsync();
        await contexto.Tiendas.Where(t => ids.Contains(t.Id)).ExecuteDeleteAsync();
    }

    [FactSqlServer]
    public async Task CrearTiendaDevuelve201YRepetidaDevuelve409()
    {
        try
        {
            await CrearYRepetirTiendaAsync(Random.Shared.Next(CodigoHannaMinimoPruebas, int.MaxValue));
        }
        finally
        {
            await LimpiarTiendasDePruebaAsync();
        }
    }

    private async Task CrearYRepetirTiendaAsync(int codigoHanna)
    {
        var solicitud = new
        {
            codigoHanna,
            nombre = "TIENDA PRUEBA API",
            marcaTiendaId = 2,
            regionalId = 1,
            tipoTiendaId = 2,
            espacios = new[] { new { tipoEspacioId = 1, cantidadEspacios = 4 }, new { tipoEspacioId = 7, cantidadEspacios = 6 } }
        };

        var creada = await cliente.PostAsJsonAsync("/api/tiendas", solicitud);
        var repetida = await cliente.PostAsJsonAsync("/api/tiendas", solicitud);

        Assert.Equal(HttpStatusCode.Created, creada.StatusCode);
        Assert.EndsWith($"/api/tiendas/{codigoHanna}", creada.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        using var json = JsonDocument.Parse(await creada.Content.ReadAsStringAsync());
        Assert.Equal(10, json.RootElement.GetProperty("espaciosTotales").GetInt32());
        Assert.Equal(HttpStatusCode.Conflict, repetida.StatusCode);
    }

    [FactSqlServer]
    public async Task ConsolidadoPorRegionalIncluyeBogotaYAntioquia()
    {
        var respuesta = await cliente.GetAsync("/api/reportes/consolidado?agruparPor=regional");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var nombres = json.RootElement.GetProperty("grupos").EnumerateArray().Select(g => g.GetProperty("nombre").GetString()).ToList();
        Assert.Contains("BOGOTA", nombres);
        Assert.Contains("ANTIOQUIA", nombres);
    }

    [FactSqlServer]
    public async Task ConsolidadoPorNivelSinNivelIdDevuelve400()
    {
        var respuesta = await cliente.GetAsync("/api/reportes/consolidado?agruparPor=nivel");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    private async Task<JsonElement> ObtenerJsonAsync(string ruta)
    {
        var respuesta = await cliente.GetAsync(ruta);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        return json.RootElement.Clone();
    }

    [FactSqlServer]
    public async Task NivelesYArbolExponenLosSieteNivelesYElArbolDeCadaTienda()
    {
        var niveles = await ObtenerJsonAsync("/api/niveles-jerarquia");
        var arbol5503 = await ObtenerJsonAsync("/api/tiendas/5503/arbol-comercial");
        var arbol1001 = await ObtenerJsonAsync("/api/tiendas/1001/arbol-comercial");

        Assert.Equal(7, niveles.GetArrayLength());
        Assert.True(niveles[6].GetProperty("esHoja").GetBoolean());
        Assert.Equal(18, arbol5503.GetArrayLength());
        Assert.Equal(18, arbol1001.GetArrayLength());
        Assert.Equal(1, arbol5503[0].GetProperty("nodoId").GetInt32());
        Assert.Equal(2001, arbol1001[0].GetProperty("nodoId").GetInt32());
        Assert.Equal(0.3m, arbol1001[1].GetProperty("porcentajeParticipacion").GetDecimal());
    }

    [FactSqlServer]
    public async Task ArbolDeTiendaInexistenteDevuelve404()
    {
        var respuesta = await cliente.GetAsync("/api/tiendas/9999/arbol-comercial");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    [FactSqlServer]
    public async Task TiendaPorCodigoYFiltroPorRegional()
    {
        var tienda = await ObtenerJsonAsync("/api/tiendas/1001");
        var bogota = await ObtenerJsonAsync("/api/tiendas?regionalId=1");

        Assert.Equal(30, tienda.GetProperty("espaciosTotales").GetInt32());
        var codigos = bogota.EnumerateArray().Select(t => t.GetProperty("codigoHanna").GetInt32()).ToList();
        Assert.Contains(1001, codigos);
        Assert.Contains(5503, codigos);
        Assert.DoesNotContain(2045, codigos);
    }

    [FactSqlServer]
    public async Task ConsolidadoPorMarcaDeProductoYPorTienda()
    {
        var porMarcaProducto = await ObtenerJsonAsync("/api/reportes/consolidado?agruparPor=nivel&nivelId=3");
        var porTienda = await ObtenerJsonAsync("/api/reportes/consolidado?agruparPor=tienda");

        var cargas = porMarcaProducto.GetProperty("grupos").EnumerateArray()
            .ToDictionary(g => g.GetProperty("nombre").GetString()!, g => g.GetProperty("cargaTotal").GetInt32());
        Assert.Equal(8930, cargas["LILI BEAUTY"]);
        Assert.Equal(778, cargas["KARITE"]);
        var tienda5503 = porTienda.GetProperty("grupos").EnumerateArray().Single(g => g.GetProperty("clave").GetString() == "5503");
        Assert.Equal(2966, tienda5503.GetProperty("cargaTotal").GetInt32());
    }

    [FactSqlServer]
    public async Task CopiarArbolCreaUnArbolIndependienteYBloqueaLaSegundaCopia()
    {
        var codigoHanna = Random.Shared.Next(CodigoHannaMinimoPruebas, int.MaxValue);
        try
        {
            await CrearTiendaAsync(codigoHanna);

            var copia = await cliente.PostAsync($"/api/tiendas/{codigoHanna}/arbol-comercial/copiar-desde/5503", null);
            var segunda = await cliente.PostAsync($"/api/tiendas/{codigoHanna}/arbol-comercial/copiar-desde/5503", null);

            Assert.Equal(HttpStatusCode.Created, copia.StatusCode);
            using (var json = JsonDocument.Parse(await copia.Content.ReadAsStringAsync()))
            {
                Assert.Equal(18, json.RootElement.GetProperty("nodosCopiados").GetInt32());
                Assert.Equal(codigoHanna, json.RootElement.GetProperty("codigoHannaDestino").GetInt32());
            }

            Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
            var arbolDestino = await ObtenerJsonAsync($"/api/tiendas/{codigoHanna}/arbol-comercial");
            Assert.Equal(18, arbolDestino.GetArrayLength());

            // El mismo concepto tiene nodos distintos y editar uno no toca el arbol de la otra tienda.
            var kariteDestino = arbolDestino.EnumerateArray().Single(n => n.GetProperty("nombre").GetString() == "KARITE");
            var arbol5503 = await ObtenerJsonAsync("/api/tiendas/5503/arbol-comercial");
            var karite5503 = arbol5503.EnumerateArray().Single(n => n.GetProperty("nombre").GetString() == "KARITE");
            Assert.NotEqual(karite5503.GetProperty("nodoId").GetInt32(), kariteDestino.GetProperty("nodoId").GetInt32());

            var actualizado = await cliente.PutAsJsonAsync(
                $"/api/nodos-comerciales/{kariteDestino.GetProperty("nodoId").GetInt32()}/porcentaje",
                new { porcentajeParticipacion = 0.05m });
            Assert.Equal(HttpStatusCode.OK, actualizado.StatusCode);

            var arbol5503Despues = await ObtenerJsonAsync("/api/tiendas/5503/arbol-comercial");
            var arbolDestinoDespues = await ObtenerJsonAsync($"/api/tiendas/{codigoHanna}/arbol-comercial");
            Assert.Equal(0.06m, arbol5503Despues.EnumerateArray().Single(n => n.GetProperty("nombre").GetString() == "KARITE").GetProperty("porcentajeParticipacion").GetDecimal());
            Assert.Equal(0.05m, arbolDestinoDespues.EnumerateArray().Single(n => n.GetProperty("nombre").GetString() == "KARITE").GetProperty("porcentajeParticipacion").GetDecimal());
        }
        finally
        {
            await LimpiarTiendasDePruebaAsync();
        }
    }

    [FactSqlServer]
    public async Task CopiarArbolDesdeOrigenVacioDevuelve400()
    {
        var destino = Random.Shared.Next(CodigoHannaMinimoPruebas, int.MaxValue);
        var origen = Random.Shared.Next(CodigoHannaMinimoPruebas, int.MaxValue);
        try
        {
            await CrearTiendaAsync(destino);
            await CrearTiendaAsync(origen);

            var respuesta = await cliente.PostAsync($"/api/tiendas/{destino}/arbol-comercial/copiar-desde/{origen}", null);

            Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
            Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        }
        finally
        {
            await LimpiarTiendasDePruebaAsync();
        }
    }

    [FactSqlServer]
    public async Task CrearNodoConPadreDeOtraTiendaDevuelve400()
    {
        var codigoHanna = Random.Shared.Next(CodigoHannaMinimoPruebas, int.MaxValue);
        try
        {
            await CrearTiendaAsync(codigoHanna);

            var padreAjeno = await cliente.PostAsJsonAsync(
                $"/api/tiendas/{codigoHanna}/nodos-comerciales",
                new { nodoPadreId = 1, conceptoComercialId = 2, porcentajeParticipacion = 0.1m });
            var padreInexistente = await cliente.PostAsJsonAsync(
                $"/api/tiendas/{codigoHanna}/nodos-comerciales",
                new { nodoPadreId = 999999, conceptoComercialId = 2, porcentajeParticipacion = 0.1m });

            Assert.Equal(HttpStatusCode.BadRequest, padreAjeno.StatusCode);
            Assert.Equal("application/problem+json", padreAjeno.Content.Headers.ContentType?.MediaType);
            Assert.Equal(HttpStatusCode.NotFound, padreInexistente.StatusCode);
        }
        finally
        {
            await LimpiarTiendasDePruebaAsync();
        }
    }

    private async Task CrearTiendaAsync(int codigoHanna)
    {
        var respuesta = await cliente.PostAsJsonAsync("/api/tiendas", new
        {
            codigoHanna,
            nombre = "TIENDA PRUEBA API",
            marcaTiendaId = 2,
            regionalId = 1,
            tipoTiendaId = 2,
            espacios = new[] { new { tipoEspacioId = 1, cantidadEspacios = 5 }, new { tipoEspacioId = 4, cantidadEspacios = 3 }, new { tipoEspacioId = 7, cantidadEspacios = 14 } }
        });
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
    }

    [FactSqlServer]
    public async Task DatosInvalidosDevuelven400()
    {
        var tiendaInvalida = await cliente.PostAsJsonAsync("/api/tiendas", new { codigoHanna = 0, nombre = "", espacios = Array.Empty<object>() });
        var porcentajeInvalido = await cliente.PutAsJsonAsync("/api/nodos-comerciales/2/porcentaje", new { porcentajeParticipacion = 1.5m });

        Assert.Equal(HttpStatusCode.BadRequest, tiendaInvalida.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, porcentajeInvalido.StatusCode);
    }

    [FactSqlServer]
    public async Task NodoInexistenteDevuelve404YCatalogoDuplicadoDevuelve409()
    {
        var nodo = await cliente.PutAsJsonAsync("/api/nodos-comerciales/999999/porcentaje", new { porcentajeParticipacion = 0.5m });
        var regional = await cliente.PostAsJsonAsync("/api/catalogos/regionales", new { nombre = "BOGOTA" });

        Assert.Equal(HttpStatusCode.NotFound, nodo.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, regional.StatusCode);
    }

    [FactSqlServer]
    public async Task CorsPermiteElOrigenDelFrontYRechazaLosDemas()
    {
        static HttpRequestMessage Preflight(string origen)
        {
            var solicitud = new HttpRequestMessage(HttpMethod.Options, "/api/carga/5503");
            solicitud.Headers.Add("Origin", origen);
            solicitud.Headers.Add("Access-Control-Request-Method", "GET");
            return solicitud;
        }

        var permitido = await cliente.SendAsync(Preflight("http://localhost:5173"));
        var noPermitido = await cliente.SendAsync(Preflight("http://sitio-desconocido.com"));

        Assert.True(permitido.IsSuccessStatusCode);
        Assert.Equal("http://localhost:5173", Assert.Single(permitido.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.False(noPermitido.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [FactSqlServer]
    public async Task CorsTambienAplicaALasRespuestasDeError()
    {
        var solicitud = new HttpRequestMessage(HttpMethod.Get, "/api/carga/9999");
        solicitud.Headers.Add("Origin", "http://localhost:3000");

        var respuesta = await cliente.SendAsync(solicitud);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("http://localhost:3000", Assert.Single(respuesta.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.Contains("Location", Assert.Single(respuesta.Headers.GetValues("Access-Control-Expose-Headers")));
    }

    [FactSqlServer]
    public async Task SwaggerDocumentaLosEndpoints()
    {
        var documento = await ObtenerJsonAsync("/swagger/v1/swagger.json");

        var rutas = documento.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();
        Assert.Contains("/api/carga/{codigoHanna}", rutas);
        Assert.Contains("/api/reportes/consolidado", rutas);
        Assert.Contains("/api/tiendas/{codigoHanna}/arbol-comercial", rutas);
        Assert.Contains("/api/tiendas/{codigoHanna}/arbol-comercial/copiar-desde/{codigoHannaOrigen}", rutas);
        Assert.Contains("/api/tiendas/{codigoHanna}/nodos-comerciales", rutas);
        Assert.DoesNotContain("/api/arbol-comercial", rutas);
        Assert.Equal("API Abastecimiento Retail", documento.GetProperty("info").GetProperty("title").GetString());
    }
}
