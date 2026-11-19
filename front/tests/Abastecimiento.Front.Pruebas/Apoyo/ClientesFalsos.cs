using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Pruebas.Apoyo;

/// <summary>Clientes en memoria para las pruebas de componentes (sin API real).</summary>
public sealed class CatalogosFalsos : IClienteCatalogos
{
    public Dictionary<TipoCatalogo, List<CatalogoRespuesta>> Datos { get; } = new()
    {
        [TipoCatalogo.MarcasTienda] = [new(1, "LILI PINK"), new(2, "YOI")],
        [TipoCatalogo.Regionales] = [new(1, "BOGOTA")],
        [TipoCatalogo.TiposTienda] = [new(1, "CALLE")]
    };

    public Task<IReadOnlyList<CatalogoRespuesta>> ListarAsync(TipoCatalogo tipo, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<CatalogoRespuesta>>(Datos[tipo]);

    public Task<CatalogoRespuesta> CrearAsync(TipoCatalogo tipo, CrearCatalogoSolicitud solicitud, CancellationToken ct)
    {
        var nuevo = new CatalogoRespuesta(Datos[tipo].Count + 1, solicitud.Nombre);
        Datos[tipo].Add(nuevo);
        return Task.FromResult(nuevo);
    }
}

public sealed class TiposEspacioFalsos : IClienteTiposEspacio
{
    public List<TipoEspacioRespuesta> Datos { get; } =
    [
        new(1, "Isla", CategoriaEspacio.Mobiliario),
        new(2, "Casa David", CategoriaEspacio.Mobiliario),
        new(3, "Mesa", CategoriaEspacio.Mobiliario),
        new(4, "Góndola", CategoriaEspacio.Mobiliario),
        new(5, "Perimetral", CategoriaEspacio.Perimetral)
    ];

    public Task<IReadOnlyList<TipoEspacioRespuesta>> ListarAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<TipoEspacioRespuesta>>(Datos);

    public Task<TipoEspacioRespuesta> CrearAsync(CrearTipoEspacioSolicitud solicitud, CancellationToken ct) =>
        Task.FromResult(new TipoEspacioRespuesta(Datos.Count + 1, solicitud.Nombre, solicitud.Categoria));
}

public sealed class TiendasFalsas : IClienteTiendas
{
    public List<CrearTiendaSolicitud> Creadas { get; } = [];

    /// <summary>Si no es null, CrearAsync lanza este error.</summary>
    public ExcepcionApi? ErrorAlCrear { get; set; }

    public Task<IReadOnlyList<TiendaRespuesta>> ListarAsync(int? marcaTiendaId, int? regionalId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<TiendaRespuesta>>([]);

    public Task<TiendaRespuesta> ObtenerAsync(int codigoHanna, CancellationToken ct) =>
        throw new ExcepcionApi(404, "Recurso no encontrado", $"No existe la tienda {codigoHanna}.");

    public Task<TiendaRespuesta> CrearAsync(CrearTiendaSolicitud solicitud, CancellationToken ct)
    {
        Creadas.Add(solicitud);
        if (ErrorAlCrear is not null)
        {
            throw ErrorAlCrear;
        }

        var catalogo = new CatalogoRespuesta(1, "X");
        return Task.FromResult(new TiendaRespuesta(
            99, solicitud.CodigoHanna, solicitud.Nombre, catalogo, catalogo, catalogo, [],
            solicitud.Espacios.Sum(e => e.CantidadEspacios)));
    }
}

public sealed class ArbolFalso : IClienteArbolComercial
{
    public List<CrearConceptoSolicitud> ConceptosCreados { get; } = [];

    public List<(int NodoId, decimal Porcentaje)> PorcentajesActualizados { get; } = [];

    public List<CrearNodoSolicitud> NodosCreados { get; } = [];

    public Task<IReadOnlyList<NivelRespuesta>> ObtenerNivelesAsync(CancellationToken ct) =>
        Task.FromResult(Datos5503.Niveles);

    public Task<IReadOnlyList<NodoArbolRespuesta>> ObtenerArbolAsync(CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<NodoArbolRespuesta>>([]);

    public Task<IReadOnlyList<ConceptoRespuesta>> ListarConceptosAsync(int? nivelJerarquiaId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<ConceptoRespuesta>>([]);

    public Task<ConceptoRespuesta> CrearConceptoAsync(CrearConceptoSolicitud solicitud, CancellationToken ct)
    {
        ConceptosCreados.Add(solicitud);
        var nivel = Datos5503.Niveles.First(n => n.Id == solicitud.NivelJerarquiaId);
        return Task.FromResult(new ConceptoRespuesta(100, solicitud.Nombre, nivel, solicitud.UnidadesPorEspacio));
    }

    public Task<NodoArbolRespuesta> CrearNodoAsync(CrearNodoSolicitud solicitud, CancellationToken ct)
    {
        NodosCreados.Add(solicitud);
        return Task.FromResult(Nodo(200, solicitud.PorcentajeParticipacion));
    }

    public Task<NodoArbolRespuesta> ActualizarPorcentajeAsync(int nodoId, decimal porcentajeParticipacion, CancellationToken ct)
    {
        PorcentajesActualizados.Add((nodoId, porcentajeParticipacion));
        return Task.FromResult(Nodo(nodoId, porcentajeParticipacion));
    }

    public static NodoArbolRespuesta Nodo(int id, decimal porcentaje) => new(
        id, 6, 7, "LABIOS", "Perfil", 6, porcentaje, porcentaje, "BELLEZA Y BIENESTAR > BEAUTY > LABIOS", null, false);
}
