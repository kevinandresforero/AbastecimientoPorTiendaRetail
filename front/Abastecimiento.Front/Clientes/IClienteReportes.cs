using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Reportes consolidados: <c>/api/reportes</c>.</summary>
public interface IClienteReportes
{
    /// <summary>
    /// GET <c>/api/reportes/consolidado?agruparPor=&amp;nivelId=</c>; <paramref name="nivelId"/> es obligatorio con
    /// <see cref="AgruparPor.Nivel"/> (400 si falta).
    /// </summary>
    Task<ConsolidadoRespuesta> ConsolidadoAsync(AgruparPor agruparPor, int? nivelId, CancellationToken ct);
}
