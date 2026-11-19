using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Nodos del arbol: "donde esta" cada concepto y con que % de participacion respecto a su padre (R2).</summary>
[ApiController]
[Route("api/nodos-comerciales")]
[Produces("application/json")]
public class NodosComercialesController(IArbolComercialServicio servicio) : ControllerBase
{
    /// <summary>Ubica un concepto bajo un nodo padre (o como raiz) con su % de participacion.</summary>
    /// <param name="solicitud">Padre, concepto y porcentaje (fraccion en (0, 1]).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Nodo creado.</response>
    /// <response code="400">Concepto inexistente, salto de nivel o suma de hermanos mayor a 1.</response>
    /// <response code="404">No existe el nodo padre.</response>
    /// <response code="409">El concepto ya esta bajo ese padre.</response>
    [HttpPost]
    [ProducesResponseType<NodoArbolRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<NodoArbolRespuesta>> Crear(CrearNodoSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearNodoAsync(solicitud, ct));

    /// <summary>Actualiza el % de participacion de un nodo respecto a su padre.</summary>
    /// <param name="id">Id del nodo.</param>
    /// <param name="solicitud">Nuevo porcentaje (fraccion en (0, 1]).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Nodo actualizado.</response>
    /// <response code="400">Porcentaje invalido o suma de hermanos mayor a 1.</response>
    /// <response code="404">No existe el nodo.</response>
    [HttpPut("{id:int}/porcentaje")]
    [ProducesResponseType<NodoArbolRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<NodoArbolRespuesta>> ActualizarPorcentaje(int id, ActualizarPorcentajeSolicitud solicitud, CancellationToken ct) =>
        Ok(await servicio.ActualizarPorcentajeAsync(id, solicitud, ct));
}
