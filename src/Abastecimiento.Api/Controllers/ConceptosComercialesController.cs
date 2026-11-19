using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Conceptos comerciales: "que es" cada elemento del arbol (R2).</summary>
[ApiController]
[Route("api/conceptos-comerciales")]
[Produces("application/json")]
public class ConceptosComercialesController(IArbolComercialServicio servicio) : ControllerBase
{
    /// <summary>Lista los conceptos, opcionalmente de un nivel.</summary>
    /// <param name="nivelJerarquiaId">Id del nivel (opcional).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Conceptos con su nivel.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ConceptoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ConceptoRespuesta>>> Listar([FromQuery] int? nivelJerarquiaId, CancellationToken ct) =>
        Ok(await servicio.ListarConceptosAsync(nivelJerarquiaId, ct));

    /// <summary>Crea un concepto. En el nivel hoja (Tipo de Producto) las unidades por espacio son obligatorias; en los demas no se admiten.</summary>
    /// <param name="solicitud">Nivel, nombre y unidades por espacio.</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Concepto creado.</response>
    /// <response code="400">Nivel inexistente o regla de unidades por espacio violada.</response>
    /// <response code="409">Ya existe el concepto en ese nivel.</response>
    [HttpPost]
    [ProducesResponseType<ConceptoRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<ConceptoRespuesta>> Crear(CrearConceptoSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearConceptoAsync(solicitud, ct));
}
