using System.Net;
using System.Text;

namespace Abastecimiento.Front.Pruebas.Apoyo;

/// <summary>HttpMessageHandler falso: responde con una funcion y guarda las solicitudes recibidas.</summary>
public sealed class ManejadorFalso(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    public List<(HttpRequestMessage Solicitud, string? Cuerpo)> Recibidas { get; } = [];

    public static HttpClient Cliente(ManejadorFalso manejador) => new(manejador) { BaseAddress = new Uri("http://api.prueba/") };

    public static HttpResponseMessage Json(HttpStatusCode estado, string json, string tipo = "application/json") =>
        new(estado) { Content = new StringContent(json, Encoding.UTF8, tipo) };

    public static HttpResponseMessage Problema(HttpStatusCode estado, string json) =>
        Json(estado, json, "application/problem+json");

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var cuerpo = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        Recibidas.Add((request, cuerpo));
        return responder(request);
    }
}
