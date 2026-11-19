using Abastecimiento.Aplicacion.Dtos.Carga;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Motor de calculo de carga por tienda (R3) y desglose por producto (R4a).</summary>
[ApiController]
[Route("api/carga")]
[Produces("application/json")]
public class CargaController(ICargaServicio servicio) : ControllerBase
{
    /// <summary>
    /// Calcula en cascada la carga de la tienda: espacios totales → % de cada nivel → espacios por tipo de producto
    /// → x unidades por espacio. Los espacios conservan todos sus decimales; solo la carga total se redondea.
    /// </summary>
    /// <param name="codigoHanna">Codigo HANNA de la tienda (ej. 5503).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Desglose de carga por tipo de producto y totales.</response>
    /// <response code="404">No existe la tienda.</response>
    [HttpGet("{codigoHanna:int}")]
    [ProducesResponseType<CargaTiendaRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<CargaTiendaRespuesta>> Calcular(int codigoHanna, CancellationToken ct) =>
        Ok(await servicio.CalcularPorCodigoHannaAsync(codigoHanna, ct));
}
