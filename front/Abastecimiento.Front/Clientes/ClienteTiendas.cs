using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Implementacion HTTP de <see cref="IClienteTiendas"/>.</summary>
public sealed class ClienteTiendas(HttpClient http) : ClienteApiBase(http), IClienteTiendas
{
    /// <inheritdoc />
    public Task<IReadOnlyList<TiendaRespuesta>> ListarAsync(int? marcaTiendaId, int? regionalId, CancellationToken ct)
    {
        var filtros = new List<string>();
        if (marcaTiendaId is int marca)
        {
            filtros.Add($"marcaTiendaId={marca}");
        }

        if (regionalId is int regional)
        {
            filtros.Add($"regionalId={regional}");
        }

        var ruta = filtros.Count == 0 ? "api/tiendas" : $"api/tiendas?{string.Join('&', filtros)}";
        return GetAsync<IReadOnlyList<TiendaRespuesta>>(ruta, ct);
    }

    /// <inheritdoc />
    public Task<TiendaRespuesta> ObtenerAsync(int codigoHanna, CancellationToken ct) =>
        GetAsync<TiendaRespuesta>($"api/tiendas/{codigoHanna}", ct);

    /// <inheritdoc />
    public Task<TiendaRespuesta> CrearAsync(CrearTiendaSolicitud solicitud, CancellationToken ct) =>
        PostAsync<CrearTiendaSolicitud, TiendaRespuesta>("api/tiendas", solicitud, ct);
}
