using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Tiendas: <c>/api/tiendas</c>.</summary>
public interface IClienteTiendas
{
    /// <summary>GET <c>/api/tiendas?marcaTiendaId=&amp;regionalId=</c>.</summary>
    Task<IReadOnlyList<TiendaRespuesta>> ListarAsync(int? marcaTiendaId, int? regionalId, CancellationToken ct);

    /// <summary>GET <c>/api/tiendas/{codigoHanna}</c>; 404 si no existe.</summary>
    Task<TiendaRespuesta> ObtenerAsync(int codigoHanna, CancellationToken ct);

    /// <summary>POST <c>/api/tiendas</c>; 400 datos invalidos, 409 codigo HANNA duplicado.</summary>
    Task<TiendaRespuesta> CrearAsync(CrearTiendaSolicitud solicitud, CancellationToken ct);
}
