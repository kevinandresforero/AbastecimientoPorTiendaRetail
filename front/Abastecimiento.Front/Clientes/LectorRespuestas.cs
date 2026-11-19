using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>
/// Envia la solicitud y traduce la respuesta: 2xx → el tipo esperado; otro codigo → <see cref="ExcepcionApi"/> leida del
/// problem+json; falla de red o timeout → <see cref="ExcepcionApi"/> con Estado 0. La cancelacion pedida por el
/// llamador se propaga como <see cref="OperationCanceledException"/>.
/// </summary>
public static class LectorRespuestas
{
    /// <summary>Opciones JSON compartidas: camelCase, sin distinguir mayusculas y enums como texto camelCase.</summary>
    public static JsonSerializerOptions OpcionesJson { get; } = CrearOpciones();

    /// <summary>Ejecuta <paramref name="envio"/> y lee la respuesta como <typeparamref name="T"/>.</summary>
    public static async Task<T> EnviarAsync<T>(
        Func<CancellationToken, Task<HttpResponseMessage>> envio, CancellationToken ct)
    {
        try
        {
            using var respuesta = await envio(ct);
            return await LeerAsync<T>(respuesta, ct);
        }
        catch (HttpRequestException ex)
        {
            throw ExcepcionApi.SinConexion(ex);
        }
        catch (IOException ex)
        {
            throw ExcepcionApi.SinConexion(ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // HttpClient.Timeout se reporta como TaskCanceledException sin que el llamador haya cancelado.
            throw ExcepcionApi.SinConexion(ex);
        }
    }

    /// <summary>Lee una respuesta: deserializa el exito o lanza la <see cref="ExcepcionApi"/> del problema.</summary>
    public static async Task<T> LeerAsync<T>(HttpResponseMessage respuesta, CancellationToken ct)
    {
        var estado = (int)respuesta.StatusCode;
        if (!respuesta.IsSuccessStatusCode)
        {
            throw ExcepcionApi.DesdeProblema(estado, await LeerProblemaAsync(respuesta, ct), respuesta.ReasonPhrase);
        }

        try
        {
            var valor = await respuesta.Content.ReadFromJsonAsync<T>(OpcionesJson, ct);
            return valor ?? throw RespuestaInvalida(estado, null);
        }
        catch (JsonException ex)
        {
            throw RespuestaInvalida(estado, ex);
        }
    }

    private static async Task<ProblemaApi?> LeerProblemaAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        var tipo = respuesta.Content.Headers.ContentType?.MediaType;
        if (tipo is not ("application/problem+json" or "application/json"))
        {
            return null;
        }

        try
        {
            return await respuesta.Content.ReadFromJsonAsync<ProblemaApi>(OpcionesJson, ct);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ExcepcionApi RespuestaInvalida(int estado, Exception? interna) => new(
        estado,
        "Respuesta inválida de la API",
        "La API respondió con un contenido que no se pudo interpretar.",
        excepcionInterna: interna);

    private static JsonSerializerOptions CrearOpciones()
    {
        var opciones = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opciones.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        opciones.MakeReadOnly(populateMissingResolver: true);
        return opciones;
    }
}
