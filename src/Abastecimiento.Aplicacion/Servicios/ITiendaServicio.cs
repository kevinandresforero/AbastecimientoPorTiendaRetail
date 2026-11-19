using Abastecimiento.Aplicacion.Dtos.Tiendas;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>Gestion de tiendas y su mobiliario (R1).</summary>
public interface ITiendaServicio
{
    /// <summary>Registra una tienda con sus espacios. HANNA duplicado → 409; catalogos inexistentes o reglas de espacios → 400.</summary>
    Task<TiendaRespuesta> CrearAsync(CrearTiendaSolicitud solicitud, CancellationToken ct);

    /// <summary>Tienda por codigo HANNA; no existe → 404.</summary>
    Task<TiendaRespuesta> ObtenerPorCodigoHannaAsync(int codigoHanna, CancellationToken ct);

    /// <summary>Tiendas con filtros opcionales, ordenadas por codigo HANNA.</summary>
    Task<IReadOnlyList<TiendaRespuesta>> ListarAsync(int? marcaTiendaId, int? regionalId, CancellationToken ct);
}
