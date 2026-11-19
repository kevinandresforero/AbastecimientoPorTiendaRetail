using Abastecimiento.Aplicacion.Dtos.Carga;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;

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

        var nodos = await arbolRepositorio.ObtenerArbolCompletoAsync(ct);
        var resultado = motor.Calcular(tienda.EspaciosTotales, nodos);

        return MapeoHelper.ACargaTiendaRespuesta(tienda, resultado);
    }
}
