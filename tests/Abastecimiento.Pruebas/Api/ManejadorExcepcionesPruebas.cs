using System.Text.Json;
using Abastecimiento.Api.Middleware;
using Abastecimiento.Dominio.Excepciones;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Abastecimiento.Pruebas.Api;

/// <summary>Traduccion de excepciones a ProblemDetails (RFC 7807) sin levantar la API.</summary>
public class ManejadorExcepcionesPruebas
{
    private static async Task<(int Estado, string? TipoContenido, JsonElement Cuerpo)> ManejarAsync(Exception excepcion)
    {
        var servicios = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var contexto = new DefaultHttpContext { RequestServices = servicios };
        contexto.Response.Body = new MemoryStream();
        var manejador = new ManejadorExcepciones(
            servicios.GetRequiredService<IProblemDetailsService>(), NullLogger<ManejadorExcepciones>.Instance);

        var manejada = await manejador.TryHandleAsync(contexto, excepcion, CancellationToken.None);

        Assert.True(manejada);
        contexto.Response.Body.Position = 0;
        using var json = await JsonDocument.ParseAsync(contexto.Response.Body);
        return (contexto.Response.StatusCode, contexto.Response.ContentType, json.RootElement.Clone());
    }

    [Fact]
    public async Task ValidacionDevuelve400ConLaListaDeErrores()
    {
        var (estado, tipo, cuerpo) = await ManejarAsync(new ValidacionDominioExcepcion(new[] { "error 1", "error 2" }));

        Assert.Equal(StatusCodes.Status400BadRequest, estado);
        Assert.StartsWith("application/problem+json", tipo);
        Assert.Equal("Regla de negocio violada", cuerpo.GetProperty("title").GetString());
        Assert.Equal(2, cuerpo.GetProperty("errores").GetArrayLength());
    }

    [Theory]
    [InlineData("no-encontrado", StatusCodes.Status404NotFound)]
    [InlineData("conflicto", StatusCodes.Status409Conflict)]
    public async Task ExcepcionesDeDominioDevuelvenSuCodigoYMensaje(string tipoExcepcion, int estadoEsperado)
    {
        Exception excepcion = tipoExcepcion == "no-encontrado"
            ? new NoEncontradoExcepcion("No existe la tienda 9999.")
            : new ConflictoExcepcion("Ya existe la tienda 5503.");

        var (estado, tipo, cuerpo) = await ManejarAsync(excepcion);

        Assert.Equal(estadoEsperado, estado);
        Assert.StartsWith("application/problem+json", tipo);
        Assert.Equal(excepcion.Message, cuerpo.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task RestriccionDeBaseDeDatosDevuelve409()
    {
        var (estado, _, cuerpo) = await ManejarAsync(new DbUpdateException("UQ_Tiendas_CodigoHanna"));

        Assert.Equal(StatusCodes.Status409Conflict, estado);
        Assert.Equal("El registro viola una restriccion de la base de datos.", cuerpo.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ErrorInesperadoDevuelve500SinExponerElDetalle()
    {
        var (estado, _, cuerpo) = await ManejarAsync(new InvalidOperationException("cadena de conexion secreta"));

        Assert.Equal(StatusCodes.Status500InternalServerError, estado);
        Assert.DoesNotContain("secreta", cuerpo.GetRawText());
        Assert.DoesNotContain("InvalidOperationException", cuerpo.GetRawText());
    }
}
