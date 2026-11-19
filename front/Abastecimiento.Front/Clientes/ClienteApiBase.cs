using System.Net.Http.Json;

namespace Abastecimiento.Front.Clientes;

/// <summary>Base de los clientes tipados: GET/POST/PUT con las opciones JSON y el manejo de errores comunes.</summary>
public abstract class ClienteApiBase(HttpClient http)
{
    /// <summary>GET y deserializa la respuesta.</summary>
    protected Task<T> GetAsync<T>(string ruta, CancellationToken ct) =>
        LectorRespuestas.EnviarAsync<T>(token => http.GetAsync(ruta, token), ct);

    /// <summary>POST con cuerpo JSON y deserializa la respuesta.</summary>
    protected Task<TRespuesta> PostAsync<TSolicitud, TRespuesta>(string ruta, TSolicitud cuerpo, CancellationToken ct) =>
        LectorRespuestas.EnviarAsync<TRespuesta>(
            token => http.PostAsJsonAsync(ruta, cuerpo, LectorRespuestas.OpcionesJson, token), ct);

    /// <summary>PUT con cuerpo JSON y deserializa la respuesta.</summary>
    protected Task<TRespuesta> PutAsync<TSolicitud, TRespuesta>(string ruta, TSolicitud cuerpo, CancellationToken ct) =>
        LectorRespuestas.EnviarAsync<TRespuesta>(
            token => http.PutAsJsonAsync(ruta, cuerpo, LectorRespuestas.OpcionesJson, token), ct);
}
