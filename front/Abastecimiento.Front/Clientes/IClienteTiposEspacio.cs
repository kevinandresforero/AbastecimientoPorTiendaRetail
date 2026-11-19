using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Tipos de espacio: <c>/api/tipos-espacio</c>.</summary>
public interface IClienteTiposEspacio
{
    /// <summary>GET <c>/api/tipos-espacio</c>.</summary>
    Task<IReadOnlyList<TipoEspacioRespuesta>> ListarAsync(CancellationToken ct);

    /// <summary>POST <c>/api/tipos-espacio</c>; 400 datos invalidos, 409 duplicado.</summary>
    Task<TipoEspacioRespuesta> CrearAsync(CrearTipoEspacioSolicitud solicitud, CancellationToken ct);
}
