using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Arbol comercial completo (R2).</summary>
[ApiController]
[Route("api/arbol-comercial")]
[Produces("application/json")]
public class ArbolComercialController(IArbolComercialServicio servicio) : ControllerBase
{
    /// <summary>Devuelve el arbol en preorden con el % de participacion de cada nodo, el % acumulado desde la raiz y su ruta.</summary>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Nodos del arbol.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NodoArbolRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NodoArbolRespuesta>>> Obtener(CancellationToken ct) =>
        Ok(await servicio.ObtenerArbolAsync(ct));
}
