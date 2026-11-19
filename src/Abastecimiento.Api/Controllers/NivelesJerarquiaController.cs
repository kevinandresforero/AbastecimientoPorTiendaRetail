using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Abastecimiento.Api.Controllers;

/// <summary>Niveles de la jerarquia comercial (R2).</summary>
[ApiController]
[Route("api/niveles-jerarquia")]
[Produces("application/json")]
public class NivelesJerarquiaController(IArbolComercialServicio servicio) : ControllerBase
{
    /// <summary>Lista los niveles ordenados por profundidad; el de mayor profundidad es el nivel hoja (Tipo de Producto).</summary>
    /// <param name="ct">Token de cancelacion.</param>
    /// <response code="200">Niveles de la jerarquia.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<NivelRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NivelRespuesta>>> Listar(CancellationToken ct) =>
        Ok(await servicio.ObtenerNivelesAsync(ct));
}
