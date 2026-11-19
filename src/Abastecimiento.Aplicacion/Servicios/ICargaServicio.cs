using Abastecimiento.Aplicacion.Dtos.Carga;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>Motor de calculo de carga por tienda (R3) y desglose por producto (R4a).</summary>
public interface ICargaServicio
{
    /// <summary>Calcula la carga en cascada para la tienda; no existe → 404.</summary>
    Task<CargaTiendaRespuesta> CalcularPorCodigoHannaAsync(int codigoHanna, CancellationToken ct);
}
