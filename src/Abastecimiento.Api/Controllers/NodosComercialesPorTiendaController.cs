using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Nodos del arbol de una tienda: "donde esta" cada concepto y con que % de participacion (R2).</summary>
[ApiController]
[Route("api/tiendas/{codigoHanna:int}/nodos-comerciales")]
[Produces("application/json")]
public class NodosComercialesPorTiendaController(IArbolComercialServicio servicio) : ControllerBase
{
    /// <summary>Ubica un concepto bajo un nodo padre de la tienda (o como unidad de negocio raiz).</summary>
    /// <param name="codigoHanna">Codigo HANNA de la tienda.</param>
    /// <param name="solicitud">Padre, concepto y porcentaje (fraccion en (0, 1]).</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Nodo creado.</response>
    /// <response code="400">Concepto inexistente, padre de otra tienda, salto de nivel o suma de hermanos mayor a 1.</response>
    /// <response code="404">No existe la tienda o el nodo padre.</response>
    /// <response code="409">El concepto ya esta bajo ese padre en la tienda.</response>
    [HttpPost]
    [ProducesResponseType<NodoArbolRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<NodoArbolRespuesta>> Crear(
        int codigoHanna, CrearNodoSolicitud solicitud, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CrearNodoAsync(codigoHanna, solicitud, ct));
}
