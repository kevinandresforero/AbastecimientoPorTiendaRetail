using Abastecimiento.Aplicacion.Dtos.Tiendas;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>Tipos de espacio fisico: muebles y perimetrales (R1). Permite agregar nuevos sin cambiar el esquema.</summary>
public interface ITipoEspacioServicio
{
    Task<IReadOnlyList<TipoEspacioRespuesta>> ListarAsync(CancellationToken ct);

    /// <summary>Crea un tipo de espacio; nombre duplicado → 409.</summary>
    Task<TipoEspacioRespuesta> CrearAsync(CrearTipoEspacioSolicitud solicitud, CancellationToken ct);
}
