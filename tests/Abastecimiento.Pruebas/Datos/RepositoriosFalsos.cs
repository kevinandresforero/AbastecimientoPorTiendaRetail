using Abastecimiento.Dominio.Entidades;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Interfaces;

namespace Abastecimiento.Pruebas.Datos;

/// <summary>Repositorios en memoria para probar los servicios sin base de datos.</summary>
internal sealed class TiendaRepositorioFalso(TiendasDePrueba datos) : ITiendaRepositorio
{
    public Task<Tienda?> ObtenerPorCodigoHannaAsync(int codigoHanna, CancellationToken ct = default) =>
        Task.FromResult(datos.Tiendas.SingleOrDefault(t => t.CodigoHanna == codigoHanna));

    public Task<IReadOnlyList<Tienda>> ObtenerTodasAsync(int? marcaTiendaId = null, int? regionalId = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Tienda>>(datos.Tiendas
            .Where(t => marcaTiendaId is null || t.MarcaTiendaId == marcaTiendaId)
            .Where(t => regionalId is null || t.RegionalId == regionalId)
            .OrderBy(t => t.CodigoHanna)
            .ToList());

    public Task<bool> ExisteCodigoHannaAsync(int codigoHanna, CancellationToken ct = default) =>
        Task.FromResult(datos.Tiendas.Any(t => t.CodigoHanna == codigoHanna));

    public Task AgregarAsync(Tienda tienda, CancellationToken ct = default)
    {
        tienda.Id = datos.Tiendas.Max(t => t.Id) + 1;
        datos.Enlazar(tienda);
        datos.Tiendas.Add(tienda);
        return Task.CompletedTask;
    }
}

internal sealed class CatalogoRepositorioFalso<T>(List<T> elementos) : ICatalogoRepositorio<T> where T : EntidadCatalogo
{
    public Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<T>>(elementos.OrderBy(e => e.Nombre).ToList());

    public Task AgregarAsync(T entidad, CancellationToken ct = default)
    {
        entidad.Id = elementos.Count == 0 ? 1 : elementos.Max(e => e.Id) + 1;
        elementos.Add(entidad);
        return Task.CompletedTask;
    }

    public Task<bool> ExisteAsync(int id, CancellationToken ct = default) => Task.FromResult(elementos.Any(e => e.Id == id));

    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default) =>
        Task.FromResult(elementos.Any(e => string.Equals(e.Nombre, nombre, StringComparison.OrdinalIgnoreCase)));
}

/// <summary>Fabrica en memoria sobre los catalogos de <see cref="TiendasDePrueba"/>.</summary>
internal sealed class CatalogoRepositorioFabricaFalsa(TiendasDePrueba datos) : ICatalogoRepositorioFabrica
{
    private readonly Dictionary<Type, object> repositorios = new()
    {
        [typeof(MarcaTienda)] = new CatalogoRepositorioFalso<MarcaTienda>(datos.MarcasTienda),
        [typeof(Regional)] = new CatalogoRepositorioFalso<Regional>(datos.Regionales),
        [typeof(TipoTienda)] = new CatalogoRepositorioFalso<TipoTienda>(datos.TiposTienda),
        [typeof(ResponsableVisual)] = new CatalogoRepositorioFalso<ResponsableVisual>(datos.ResponsablesVisuales)
    };

    public ICatalogoRepositorio<T> Obtener<T>() where T : EntidadCatalogo => (ICatalogoRepositorio<T>)repositorios[typeof(T)];
}

internal sealed class TipoEspacioRepositorioFalso(List<TipoEspacio> tipos) : ITipoEspacioRepositorio
{
    public Task<IReadOnlyList<TipoEspacio>> ObtenerTodosAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<TipoEspacio>>(tipos.OrderBy(t => t.Id).ToList());

    public Task<IReadOnlyList<TipoEspacio>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<TipoEspacio>>(tipos.Where(t => ids.Contains(t.Id)).ToList());

    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default) =>
        Task.FromResult(tipos.Any(t => string.Equals(t.Nombre, nombre, StringComparison.OrdinalIgnoreCase)));

    public Task AgregarAsync(TipoEspacio tipoEspacio, CancellationToken ct = default)
    {
        tipoEspacio.Id = tipos.Max(t => t.Id) + 1;
        tipos.Add(tipoEspacio);
        return Task.CompletedTask;
    }
}

internal sealed class ArbolComercialRepositorioFalso : IArbolComercialRepositorio
{
    private readonly List<NodoComercial> nodos;
    private readonly List<NivelJerarquia> niveles;
    private readonly List<ConceptoComercial> conceptos;

    public ArbolComercialRepositorioFalso()
    {
        (nodos, niveles) = ArbolDePrueba.DesdeSemillaTodasLasTiendas();
        conceptos = nodos.Select(n => n.ConceptoComercial).DistinctBy(c => c.Id).ToList();
    }

    public Task<IReadOnlyList<NodoComercial>> ObtenerArbolCompletoAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<NodoComercial>>(nodos.OrderBy(n => n.Id).ToList());

    public Task AgregarConceptoAsync(ConceptoComercial concepto, CancellationToken ct = default)
    {
        concepto.Id = conceptos.Max(c => c.Id) + 1;
        concepto.NivelJerarquia = niveles.Single(n => n.Id == concepto.NivelJerarquiaId);
        conceptos.Add(concepto);
        return Task.CompletedTask;
    }

    public Task AgregarNodoAsync(NodoComercial nodo, CancellationToken ct = default)
    {
        nodo.Id = nodos.Max(n => n.Id) + 1;
        if (nodo.NodoPadre is not null)
        {
            nodo.NodoPadreId = nodo.NodoPadre.Id;
        }

        nodo.ConceptoComercial = conceptos.Single(c => c.Id == nodo.ConceptoComercialId);
        nodos.Add(nodo);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<NivelJerarquia>> ObtenerNivelesAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<NivelJerarquia>>(niveles.OrderBy(n => n.Profundidad).ToList());

    public Task<IReadOnlyList<ConceptoComercial>> ObtenerConceptosAsync(int? nivelJerarquiaId = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ConceptoComercial>>(conceptos
            .Where(c => nivelJerarquiaId is null || c.NivelJerarquiaId == nivelJerarquiaId)
            .ToList());

    public Task<ConceptoComercial?> ObtenerConceptoPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(conceptos.SingleOrDefault(c => c.Id == id));

    public Task<bool> ExisteConceptoAsync(int nivelJerarquiaId, string nombre, CancellationToken ct = default) =>
        Task.FromResult(conceptos.Any(c => c.NivelJerarquiaId == nivelJerarquiaId && c.Nombre == nombre));

    public Task<bool> ExisteNodoAsync(int? nodoPadreId, int conceptoComercialId, CancellationToken ct = default) =>
        Task.FromResult(nodos.Any(n => n.NodoPadreId == nodoPadreId && n.ConceptoComercialId == conceptoComercialId));

    public Task<NodoComercial?> ObtenerNodoParaEditarAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(nodos.SingleOrDefault(n => n.Id == id));

    public Task<IReadOnlyList<NodoComercial>> ObtenerArbolPorTiendaAsync(int tiendaId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<NodoComercial>>(nodos.Where(n => n.TiendaId == tiendaId).OrderBy(n => n.Id).ToList());

    public Task<bool> ExisteNodoAsync(int tiendaId, int? nodoPadreId, int conceptoComercialId, CancellationToken ct = default) =>
        Task.FromResult(nodos.Any(n =>
            n.TiendaId == tiendaId && n.NodoPadreId == nodoPadreId && n.ConceptoComercialId == conceptoComercialId));

    public async Task AgregarNodosAsync(IEnumerable<NodoComercial> nuevos, CancellationToken ct = default)
    {
        // Como EF: los hijos se enlazan por navegacion, asi que el padre debe recibir su Id antes que ellos.
        var pendientes = nuevos.ToList();
        while (pendientes.Count > 0)
        {
            var listos = pendientes.Where(n => n.NodoPadre is null || nodos.Contains(n.NodoPadre)).ToList();
            if (listos.Count == 0)
            {
                throw new InvalidOperationException("Los nodos a agregar tienen padres fuera del lote o ciclos.");
            }

            foreach (var nodo in listos)
            {
                await AgregarNodoAsync(nodo, ct);
                pendientes.Remove(nodo);
            }
        }
    }
}

internal sealed class UnidadDeTrabajoFalsa : IUnidadDeTrabajo
{
    public int Guardados { get; private set; }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
    {
        Guardados++;
        return Task.FromResult(1);
    }
}
