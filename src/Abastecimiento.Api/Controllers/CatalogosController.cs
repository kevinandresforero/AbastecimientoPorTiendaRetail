using Abastecimiento.Aplicacion.Dtos.Catalogos;
using Abastecimiento.Aplicacion.Servicios;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Catalogos de tienda: marcas, regionales, tipos de tienda y responsables visuales (R1).</summary>
[ApiController]
[Route("api/catalogos")]
[Produces("application/json")]
public class CatalogosController(ICatalogoServicio servicio) : ControllerBase
{
    /// <summary>Lista las marcas de tienda (YAHAD, LILI PINK, YOI).</summary>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Marcas ordenadas por nombre.</response>
    [HttpGet("marcas-tienda")]
    [ProducesResponseType<IReadOnlyList<CatalogoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoRespuesta>>> ListarMarcasTienda(CancellationToken ct) =>
        Ok(await servicio.ListarAsync<MarcaTienda>(ct));

    /// <summary>Crea una marca de tienda.</summary>
    /// <param name="solicitud">Nombre (maximo 50 caracteres).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Marca creada.</response>
    /// <response code="400">Nombre invalido.</response>
    /// <response code="409">Nombre duplicado.</response>
    [HttpPost("marcas-tienda")]
    [ProducesResponseType<CatalogoRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<CatalogoRespuesta>> CrearMarcaTienda(CrearCatalogoSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearAsync<MarcaTienda>(solicitud, ct));

    /// <summary>Lista las regionales.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Regionales ordenadas por nombre.</response>
    [HttpGet("regionales")]
    [ProducesResponseType<IReadOnlyList<CatalogoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoRespuesta>>> ListarRegionales(CancellationToken ct) =>
        Ok(await servicio.ListarAsync<Regional>(ct));

    /// <summary>Crea una regional.</summary>
    /// <param name="solicitud">Nombre (maximo 50 caracteres).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Regional creada.</response>
    /// <response code="400">Nombre invalido.</response>
    /// <response code="409">Nombre duplicado.</response>
    [HttpPost("regionales")]
    [ProducesResponseType<CatalogoRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<CatalogoRespuesta>> CrearRegional(CrearCatalogoSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearAsync<Regional>(solicitud, ct));

    /// <summary>Lista los tipos de tienda.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Tipos de tienda ordenados por nombre.</response>
    [HttpGet("tipos-tienda")]
    [ProducesResponseType<IReadOnlyList<CatalogoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoRespuesta>>> ListarTiposTienda(CancellationToken ct) =>
        Ok(await servicio.ListarAsync<TipoTienda>(ct));

    /// <summary>Crea un tipo de tienda.</summary>
    /// <param name="solicitud">Nombre (maximo 50 caracteres).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Tipo de tienda creado.</response>
    /// <response code="400">Nombre invalido.</response>
    /// <response code="409">Nombre duplicado.</response>
    [HttpPost("tipos-tienda")]
    [ProducesResponseType<CatalogoRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<CatalogoRespuesta>> CrearTipoTienda(CrearCatalogoSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearAsync<TipoTienda>(solicitud, ct));

    /// <summary>Lista los responsables visuales.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Responsables ordenados por nombre.</response>
    [HttpGet("responsables-visuales")]
    [ProducesResponseType<IReadOnlyList<CatalogoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoRespuesta>>> ListarResponsablesVisuales(CancellationToken ct) =>
        Ok(await servicio.ListarAsync<ResponsableVisual>(ct));

    /// <summary>Crea un responsable visual.</summary>
    /// <param name="solicitud">Nombre (maximo 100 caracteres).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Responsable creado.</response>
    /// <response code="400">Nombre invalido.</response>
    /// <response code="409">Nombre duplicado.</response>
    [HttpPost("responsables-visuales")]
    [ProducesResponseType<CatalogoRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<CatalogoRespuesta>> CrearResponsableVisual(CrearCatalogoSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearAsync<ResponsableVisual>(solicitud, ct));
}
