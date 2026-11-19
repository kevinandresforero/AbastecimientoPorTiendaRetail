using System.Text.Json;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Implementacion HTTP de <see cref="IClienteReportes"/>.</summary>
public sealed class ClienteReportes(HttpClient http) : ClienteApiBase(http), IClienteReportes
{
    /// <inheritdoc />
    public Task<ConsolidadoRespuesta> ConsolidadoAsync(AgruparPor agruparPor, int? nivelId, CancellationToken ct)
    {
        // La API recibe el enum como texto camelCase: tienda, marcaTienda, regional, nivel.
        var ruta = $"api/reportes/consolidado?agruparPor={JsonNamingPolicy.CamelCase.ConvertName(agruparPor.ToString())}";
        if (nivelId is int nivel)
        {
            ruta += $"&nivelId={nivel}";
        }

        return GetAsync<ConsolidadoRespuesta>(ruta, ct);
    }
}
