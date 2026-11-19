using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Motor de calculo: <c>/api/carga</c>.</summary>
public interface IClienteCarga
{
    /// <summary>GET <c>/api/carga/{codigoHanna}</c>; 404 si la tienda no existe.</summary>
    Task<CargaTiendaRespuesta> CalcularAsync(int codigoHanna, CancellationToken ct);
}
