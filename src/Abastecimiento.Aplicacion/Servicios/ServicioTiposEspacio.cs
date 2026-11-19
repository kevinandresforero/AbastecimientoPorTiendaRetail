using Abastecimiento.Aplicacion.Dtos.Tiendas;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;

namespace Abastecimiento.Aplicacion.Servicios;

public sealed class ServicioTiposEspacio(ITipoEspacioRepositorio tiposEspacio, IUnidadDeTrabajo unidadDeTrabajo) : ITipoEspacioServicio
{
    public async Task<IReadOnlyList<TipoEspacioRespuesta>> ListarAsync(CancellationToken ct)
    {
        var tipos = await tiposEspacio.ObtenerTodosAsync(ct);
        return tipos.Select(MapeoHelper.ATipoEspacioRespuesta).ToList();
    }

    public async Task<TipoEspacioRespuesta> CrearAsync(CrearTipoEspacioSolicitud solicitud, CancellationToken ct)
    {
        var nombre = solicitud.Nombre.Trim();
        var errores = new List<string>();

        if (nombre.Length == 0)
        {
            errores.Add("El nombre del tipo de espacio es obligatorio.");
        }

        if (!Enum.IsDefined(solicitud.Categoria))
        {
            errores.Add("La categoria debe ser Mobiliario o Perimetral.");
        }

        if (errores.Count > 0)
        {
            throw new ValidacionDominioExcepcion(errores);
        }

        if (await tiposEspacio.ExisteNombreAsync(nombre, ct))
        {
            throw new ConflictoExcepcion($"Ya existe un tipo de espacio con el nombre '{nombre}'.");
        }

        var tipo = new TipoEspacio { Nombre = nombre, Categoria = solicitud.Categoria };
        await tiposEspacio.AgregarAsync(tipo, ct);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return MapeoHelper.ATipoEspacioRespuesta(tipo);
    }
}
