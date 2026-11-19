using Abastecimiento.Aplicacion.Dtos.Reportes;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Reportes consolidados (R4b).</summary>
[ApiController]
[Route("api/reportes")]
[Produces("application/json")]
public class ReportesController(IReporteServicio servicio) : ControllerBase
{
    /// <summary>Consolidado de espacios y unidades agrupado por tienda, marca de tienda, regional o nivel del arbol.</summary>
    /// <param name="agruparPor"><c>tienda</c>, <c>marcaTienda</c>, <c>regional</c> o <c>nivel</c>.</param>
    /// <param name="nivelId">Obligatorio con <c>agruparPor=nivel</c> (ej. 3 = marca de producto: LILI BEAUTY / KARITE).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Grupos y total general.</response>
    /// <response code="400">Agrupacion invalida o nivel faltante/inexistente.</response>
    [HttpGet("consolidado")]
    [ProducesResponseType<ConsolidadoRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<ConsolidadoRespuesta>> Consolidado(
        [FromQuery] AgruparPor agruparPor, [FromQuery] int? nivelId, CancellationToken ct) =>
        Ok(await servicio.ObtenerConsolidadoAsync(agruparPor, nivelId, ct));
}
