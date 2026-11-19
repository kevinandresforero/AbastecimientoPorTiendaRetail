using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Porcentaje de participacion de un nodo del arbol comercial (R2).</summary>
[ApiController]
[Route("api/nodos-comerciales")]
[Produces("application/json")]
public class NodosComercialesController(IArbolComercialServicio servicio) : ControllerBase
{
    /// <summary>Actualiza el % de participacion de un nodo respecto a su padre.</summary>
    /// <param name="id">Id del nodo.</param>
    /// <param name="solicitud">Nuevo porcentaje (fraccion en (0, 1]).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Nodo actualizado.</response>
    /// <response code="400">Porcentaje invalido o suma de hermanos de la tienda mayor a 1.</response>
    /// <response code="404">No existe el nodo.</response>
    [HttpPut("{id:int}/porcentaje")]
    [ProducesResponseType<NodoArbolRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<NodoArbolRespuesta>> ActualizarPorcentaje(int id, ActualizarPorcentajeSolicitud solicitud, CancellationToken ct) =>
        Ok(await servicio.ActualizarPorcentajeAsync(id, solicitud, ct));
}
