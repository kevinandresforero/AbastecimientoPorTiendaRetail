using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Implementacion HTTP de <see cref="IClienteTiposEspacio"/>.</summary>
public sealed class ClienteTiposEspacio(HttpClient http) : ClienteApiBase(http), IClienteTiposEspacio
{
    private const string Ruta = "api/tipos-espacio";

    /// <inheritdoc />
    public Task<IReadOnlyList<TipoEspacioRespuesta>> ListarAsync(CancellationToken ct) =>
        GetAsync<IReadOnlyList<TipoEspacioRespuesta>>(Ruta, ct);

    /// <inheritdoc />
    public Task<TipoEspacioRespuesta> CrearAsync(CrearTipoEspacioSolicitud solicitud, CancellationToken ct) =>
        PostAsync<CrearTipoEspacioSolicitud, TipoEspacioRespuesta>(Ruta, solicitud, ct);
}
