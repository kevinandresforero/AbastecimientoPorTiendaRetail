using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Implementacion HTTP de <see cref="IClienteCarga"/>.</summary>
public sealed class ClienteCarga(HttpClient http) : ClienteApiBase(http), IClienteCarga
{
    /// <inheritdoc />
    public Task<CargaTiendaRespuesta> CalcularAsync(int codigoHanna, CancellationToken ct) =>
        GetAsync<CargaTiendaRespuesta>($"api/carga/{codigoHanna}", ct);
}
