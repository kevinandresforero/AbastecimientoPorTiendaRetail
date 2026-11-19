using Abastecimiento.Aplicacion.Dtos.Reportes;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>Reportes consolidados de espacios y unidades (R4b).</summary>
public interface IReporteServicio
{
    /// <summary>Consolidado por tienda, marca de tienda, regional o nivel del arbol (este ultimo requiere <paramref name="nivelId"/>).</summary>
    Task<ConsolidadoRespuesta> ObtenerConsolidadoAsync(AgruparPor agruparPor, int? nivelId, CancellationToken ct);
}
