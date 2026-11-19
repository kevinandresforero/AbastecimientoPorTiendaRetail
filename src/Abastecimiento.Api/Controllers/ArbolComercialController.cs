using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Arbol comercial de una tienda (R2).</summary>
[ApiController]
[Route("api/tiendas/{codigoHanna:int}/arbol-comercial")]
[Produces("application/json")]
public class ArbolComercialController(IArbolComercialServicio servicio) : ControllerBase
{
    /// <summary>
    /// Devuelve el arbol de la tienda en preorden con el % de participacion de cada nodo, el % acumulado desde
    /// la raiz y su ruta. Una tienda sin arbol devuelve una lista vacia.
    /// </summary>
    /// <param name="codigoHanna">Codigo HANNA de la tienda.</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Nodos del arbol de la tienda.</response>
    /// <response code="404">No existe la tienda.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NodoArbolRespuesta>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<IReadOnlyList<NodoArbolRespuesta>>> Obtener(int codigoHanna, CancellationToken ct) =>
        Ok(await servicio.ObtenerArbolAsync(codigoHanna, ct));

    /// <summary>
    /// Copia el arbol de la tienda origen en la tienda destino. La copia es independiente: crea nodos nuevos con
    /// los mismos conceptos y porcentajes. Solo se permite si la tienda destino no tiene nodos.
    /// </summary>
    /// <param name="codigoHanna">Codigo HANNA de la tienda destino (vacia).</param>
    /// <param name="codigoHannaOrigen">Codigo HANNA de la tienda de la que se copia.</param>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="201">Arbol copiado.</response>
    /// <response code="400">La tienda origen no tiene arbol o su arbol no es valido.</response>
    /// <response code="404">No existe la tienda destino u origen.</response>
    /// <response code="409">La tienda destino ya tiene arbol.</response>
    [HttpPost("copiar-desde/{codigoHannaOrigen:int}")]
    [ProducesResponseType<CopiaArbolRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<CopiaArbolRespuesta>> Copiar(
        int codigoHanna, int codigoHannaOrigen, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await servicio.CopiarArbolAsync(codigoHanna, codigoHannaOrigen, ct));
}
