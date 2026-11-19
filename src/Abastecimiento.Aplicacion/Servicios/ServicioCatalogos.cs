using Abastecimiento.Aplicacion.Dtos.Catalogos;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Dominio.Entidades;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>
/// Servicio generico de catalogos. El repositorio de cada tipo (<see cref="ICatalogoRepositorio{T}"/>, generico abierto
/// registrado en el contenedor) se resuelve por tipo para no duplicar un servicio por catalogo.
/// </summary>
public sealed class ServicioCatalogos(IServiceProvider proveedor, IUnidadDeTrabajo unidadDeTrabajo) : ICatalogoServicio
{
    private const int LongitudNombre = 50;
    private const int LongitudNombreResponsableVisual = 100;

    public async Task<IReadOnlyList<CatalogoRespuesta>> ListarAsync<T>(CancellationToken ct) where T : EntidadCatalogo
    {
        var elementos = await Repositorio<T>().ObtenerTodosAsync(ct);
        return elementos.Select(MapeoHelper.ACatalogoRespuesta).ToList();
    }

    public async Task<CatalogoRespuesta> CrearAsync<T>(CrearCatalogoSolicitud solicitud, CancellationToken ct)
        where T : EntidadCatalogo, new()
    {
        var nombre = solicitud.Nombre.Trim();
        var longitudMaxima = typeof(T) == typeof(ResponsableVisual) ? LongitudNombreResponsableVisual : LongitudNombre;

        if (nombre.Length == 0 || nombre.Length > longitudMaxima)
        {
            throw new ValidacionDominioExcepcion($"El nombre es obligatorio y admite maximo {longitudMaxima} caracteres.");
        }

        var repositorio = Repositorio<T>();
        if (await repositorio.ExisteNombreAsync(nombre, ct))
        {
            throw new ConflictoExcepcion($"Ya existe un elemento con el nombre '{nombre}'.");
        }

        var entidad = new T { Nombre = nombre };
        await repositorio.AgregarAsync(entidad, ct);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return MapeoHelper.ACatalogoRespuesta(entidad);
    }

    private ICatalogoRepositorio<T> Repositorio<T>() where T : EntidadCatalogo =>
        proveedor.GetRequiredService<ICatalogoRepositorio<T>>();
}
