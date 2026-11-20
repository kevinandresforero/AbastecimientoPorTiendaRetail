using System.Collections.Concurrent;
using Abastecimiento.Pruebas.Infraestructura;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Abastecimiento.Pruebas.Api;

/// <summary>
/// Log de peticiones HTTP: una linea por peticion <c>/api</c> con metodo, ruta y estado de la respuesta, sin cuerpos.
/// El proveedor de logs de prueba se inyecta en el host de la API (mismo mecanismo de DI que usa la aplicacion).
/// </summary>
[Collection(ColeccionSqlServer.Nombre)]
public sealed class RegistroPeticionesPruebas(FabricaApi fabrica) : IClassFixture<FabricaApi>
{
    private const string CategoriaHttpLogging = "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware";

    private (HttpClient Cliente, CapturadorLogs Capturador) CrearCliente()
    {
        var capturador = new CapturadorLogs();
        var cliente = fabrica
            .WithWebHostBuilder(host => host.ConfigureLogging(logs => logs.AddProvider(capturador)))
            .CreateClient();
        return (cliente, capturador);
    }

    [FactSqlServer]
    public async Task RegistraMetodoRutaYEstadoSinCuerpo()
    {
        var (cliente, capturador) = CrearCliente();

        await cliente.GetAsync("/api/carga/9999");

        var entrada = Assert.Single(capturador.De(CategoriaHttpLogging), m => m.Contains("/api/carga/9999"));
        Assert.Contains("GET", entrada);
        Assert.Contains("404", entrada);
        // Sin cuerpo de la respuesta (problem+json) ni headers.
        Assert.DoesNotContain("detail", entrada);
        Assert.DoesNotContain("Content-Type", entrada);
    }

    [FactSqlServer]
    public async Task RegistraRespuestasExitosasConSuQuery()
    {
        var (cliente, capturador) = CrearCliente();

        await cliente.GetAsync("/api/reportes/consolidado?agruparPor=regional");

        var entrada = Assert.Single(capturador.De(CategoriaHttpLogging), m => m.Contains("/api/reportes/consolidado"));
        Assert.Contains("agruparPor=regional", entrada);
        Assert.Contains("200", entrada);
    }

    [FactSqlServer]
    public async Task NoRegistraRutasFueraDeApi()
    {
        var (cliente, capturador) = CrearCliente();

        await cliente.GetAsync("/swagger/v1/swagger.json");

        Assert.DoesNotContain(capturador.De(CategoriaHttpLogging), m => m.Contains("swagger"));
    }

    /// <summary>Proveedor de logs en memoria: guarda categoria y mensaje de cada entrada.</summary>
    private sealed class CapturadorLogs : ILoggerProvider
    {
        private readonly ConcurrentQueue<(string Categoria, string Mensaje)> entradas = new();

        public IReadOnlyList<string> De(string categoria) =>
            entradas.Where(e => e.Categoria == categoria).Select(e => e.Mensaje).ToList();

        public ILogger CreateLogger(string categoryName) => new Registrador(categoryName, entradas);

        public void Dispose()
        {
        }

        private sealed class Registrador(string categoria, ConcurrentQueue<(string, string)> entradas) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                entradas.Enqueue((categoria, formatter(state, exception)));
        }
    }
}
