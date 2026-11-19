using Abastecimiento.Aplicacion.Dtos.Tiendas;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Gestion de tiendas y su mobiliario (R1).</summary>
[ApiController]
[Route("api/tiendas")]
[Produces("application/json")]
public class TiendasController(ITiendaServicio servicio) : ControllerBase
{
    /// <summary>Registra una tienda con su configuracion de espacios y calcula sus Espacios Totales (Regla 1).</summary>
    /// <param name="solicitud">Datos de la tienda: hasta 3 tipos de mobiliario mas espacios perimetrales.</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Tienda creada.</response>
    /// <response code="400">Datos invalidos, catalogo inexistente o regla de espacios violada.</response>
    /// <response code="409">Ya existe una tienda con ese codigo HANNA.</response>
    [HttpPost]
    [ProducesResponseType<TiendaRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<TiendaRespuesta>> Crear(CrearTiendaSolicitud solicitud, CancellationToken ct)
    {
        var tienda = await servicio.CrearAsync(solicitud, ct);
        return CreatedAtAction(nameof(ObtenerPorCodigoHanna), new { codigoHanna = tienda.CodigoHanna }, tienda);
    }

    /// <summary>Lista las tiendas, con filtros opcionales por marca de tienda y regional.</summary>
    /// <param name="marcaTiendaId">Id de la marca de tienda.</param>
    /// <param name="regionalId">Id de la regional.</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Tiendas ordenadas por codigo HANNA.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TiendaRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TiendaRespuesta>>> Listar(
        [FromQuery] int? marcaTiendaId, [FromQuery] int? regionalId, CancellationToken ct) =>
        Ok(await servicio.ListarAsync(marcaTiendaId, regionalId, ct));

    /// <summary>Obtiene una tienda por su codigo HANNA.</summary>
    /// <param name="codigoHanna">Codigo HANNA (ej. 5503).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Tienda encontrada.</response>
    /// <response code="404">No existe la tienda.</response>
    [HttpGet("{codigoHanna:int}")]
    [ProducesResponseType<TiendaRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TiendaRespuesta>> ObtenerPorCodigoHanna(int codigoHanna, CancellationToken ct) =>
        Ok(await servicio.ObtenerPorCodigoHannaAsync(codigoHanna, ct));
}
