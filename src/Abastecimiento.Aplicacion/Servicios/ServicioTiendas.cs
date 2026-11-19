using Abastecimiento.Aplicacion.Dtos.Tiendas;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;
using Abastecimiento.Dominio.Reglas;

namespace Abastecimiento.Aplicacion.Servicios;

public sealed class ServicioTiendas(
    ITiendaRepositorio tiendas,
    ITipoEspacioRepositorio tiposEspacio,
    ICatalogoRepositorio<MarcaTienda> marcasTienda,
    ICatalogoRepositorio<Regional> regionales,
    ICatalogoRepositorio<TipoTienda> tiposTienda,
    ICatalogoRepositorio<ResponsableVisual> responsablesVisuales,
    IUnidadDeTrabajo unidadDeTrabajo) : ITiendaServicio
{
    public async Task<TiendaRespuesta> CrearAsync(CrearTiendaSolicitud solicitud, CancellationToken ct)
    {
        if (await tiendas.ExisteCodigoHannaAsync(solicitud.CodigoHanna, ct))
        {
            throw new ConflictoExcepcion($"Ya existe una tienda con el codigo HANNA {solicitud.CodigoHanna}.");
        }

        var errores = new List<string>();

        if (!await marcasTienda.ExisteAsync(solicitud.MarcaTiendaId, ct))
        {
            errores.Add($"La marca de tienda {solicitud.MarcaTiendaId} no existe.");
        }

        if (!await regionales.ExisteAsync(solicitud.RegionalId, ct))
        {
            errores.Add($"La regional {solicitud.RegionalId} no existe.");
        }

        if (!await tiposTienda.ExisteAsync(solicitud.TipoTiendaId, ct))
        {
            errores.Add($"El tipo de tienda {solicitud.TipoTiendaId} no existe.");
        }

        if (solicitud.ResponsableVisualId is int responsableId && !await responsablesVisuales.ExisteAsync(responsableId, ct))
        {
            errores.Add($"El responsable visual {responsableId} no existe.");
        }

        var idsTipoEspacio = solicitud.Espacios.Select(e => e.TipoEspacioId).Distinct().ToList();
        var tiposEncontrados = (await tiposEspacio.ObtenerPorIdsAsync(idsTipoEspacio, ct)).ToDictionary(t => t.Id);
        var tiposFaltantes = idsTipoEspacio.Where(id => !tiposEncontrados.ContainsKey(id)).ToList();
        if (tiposFaltantes.Count > 0)
        {
            errores.Add($"Tipos de espacio inexistentes: {string.Join(", ", tiposFaltantes)}.");
        }

        if (errores.Count > 0)
        {
            throw new ValidacionDominioExcepcion(errores);
        }

        var tienda = new Tienda
        {
            CodigoHanna = solicitud.CodigoHanna,
            Nombre = solicitud.Nombre.Trim(),
            MarcaTiendaId = solicitud.MarcaTiendaId,
            RegionalId = solicitud.RegionalId,
            TipoTiendaId = solicitud.TipoTiendaId,
            ResponsableVisualId = solicitud.ResponsableVisualId,
            Espacios = solicitud.Espacios
                .Select(e => new EspacioTienda
                {
                    TipoEspacioId = e.TipoEspacioId,
                    CantidadEspacios = e.CantidadEspacios,
                    TipoEspacio = tiposEncontrados[e.TipoEspacioId]
                })
                .ToList()
        };

        // Maximo 3 tipos de mobiliario, sin repetidos y cantidades >= 0 (regla de dominio existente).
        ReglasEspaciosTienda.Validar(tienda.Espacios.ToList());

        await tiendas.AgregarAsync(tienda, ct);
        await unidadDeTrabajo.GuardarCambiosAsync(ct);

        return await ObtenerPorCodigoHannaAsync(tienda.CodigoHanna, ct);
    }

    public async Task<TiendaRespuesta> ObtenerPorCodigoHannaAsync(int codigoHanna, CancellationToken ct)
    {
        var tienda = await tiendas.ObtenerPorCodigoHannaAsync(codigoHanna, ct)
            ?? throw new NoEncontradoExcepcion($"No existe una tienda con el codigo HANNA {codigoHanna}.");

        return MapeoHelper.ATiendaRespuesta(tienda);
    }

    public async Task<IReadOnlyList<TiendaRespuesta>> ListarAsync(int? marcaTiendaId, int? regionalId, CancellationToken ct)
    {
        var resultado = await tiendas.ObtenerTodasAsync(marcaTiendaId, regionalId, ct);
        return resultado.Select(MapeoHelper.ATiendaRespuesta).ToList();
    }
}
