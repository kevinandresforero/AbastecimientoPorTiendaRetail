using Abastecimiento.Aplicacion.Dtos.Tiendas;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Tipos de espacio fisico: muebles (Isla, Casa David, Mesa...) y perimetrales (R1).</summary>
[ApiController]
[Route("api/tipos-espacio")]
[Produces("application/json")]
public class TiposEspacioController(ITipoEspacioServicio servicio) : ControllerBase
{
    /// <summary>Lista los tipos de espacio.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Tipos de espacio.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TipoEspacioRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TipoEspacioRespuesta>>> Listar(CancellationToken ct) =>
        Ok(await servicio.ListarAsync(ct));

    /// <summary>Crea un tipo de espacio (nuevo mueble o tipo de pared) sin cambiar el esquema.</summary>
    /// <param name="solicitud">Nombre y categoria (<c>mobiliario</c> o <c>perimetral</c>).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Tipo de espacio creado.</response>
    /// <response code="400">Datos invalidos.</response>
    /// <response code="409">Nombre duplicado.</response>
    [HttpPost]
    [ProducesResponseType<TipoEspacioRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<TipoEspacioRespuesta>> Crear(CrearTipoEspacioSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearAsync(solicitud, ct));
}
