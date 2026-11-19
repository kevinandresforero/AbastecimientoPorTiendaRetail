using Abastecimiento.Aplicacion.Dtos.Carga;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;
using Abastecimiento.Dominio.Reglas;

namespace Abastecimiento.Aplicacion.Servicios;

public sealed class ServicioCarga(
    ITiendaRepositorio tiendas,
    IArbolComercialRepositorio arbolRepositorio,
    IMotorCalculoCarga motor) : ICargaServicio
{
    public async Task<CargaTiendaRespuesta> CalcularPorCodigoHannaAsync(int codigoHanna, CancellationToken ct)
    {
        var tienda = await tiendas.ObtenerPorCodigoHannaAsync(codigoHanna, ct)
            ?? throw new NoEncontradoExcepcion($"No existe una tienda con el codigo HANNA {codigoHanna}.");

        // Las consultas comparten el DbContext del request: se ejecutan una tras otra.
        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        var nodos = await arbolRepositorio.ObtenerArbolPorTiendaAsync(tienda.Id, ct);
        var profundidadHoja = ValidadorArbolComercial.ObtenerProfundidadHoja(niveles);
        var resultado = motor.Calcular(tienda.EspaciosTotales, nodos, profundidadHoja);

        if (nodos.Count == 0)
        {
            resultado = resultado with
            {
                Advertencias = [.. resultado.Advertencias, "La tienda no tiene unidades de negocio."]
            };
        }

        return MapeoHelper.ACargaTiendaRespuesta(tienda, resultado);
    }
}
