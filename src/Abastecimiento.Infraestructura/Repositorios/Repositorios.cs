using Abastecimiento.Dominio.Entidades;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Interfaces;
using Abastecimiento.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Abastecimiento.Infraestructura.Repositorios;

public class TiendaRepositorio(AbastecimientoDbContext contexto) : ITiendaRepositorio
{
    public Task<Tienda?> ObtenerPorCodigoHannaAsync(int codigoHanna, CancellationToken ct = default) =>
        ConsultaCompleta().FirstOrDefaultAsync(t => t.CodigoHanna == codigoHanna, ct);

    public async Task<IReadOnlyList<Tienda>> ObtenerTodasAsync(int? marcaTiendaId = null, int? regionalId = null, CancellationToken ct = default)
    {
        var consulta = ConsultaCompleta();

        if (marcaTiendaId is not null)
        {
            consulta = consulta.Where(t => t.MarcaTiendaId == marcaTiendaId);
        }

        if (regionalId is not null)
        {
            consulta = consulta.Where(t => t.RegionalId == regionalId);
        }

        return await consulta.OrderBy(t => t.CodigoHanna).ToListAsync(ct);
    }

    public Task<bool> ExisteCodigoHannaAsync(int codigoHanna, CancellationToken ct = default) =>
        contexto.Tiendas.AnyAsync(t => t.CodigoHanna == codigoHanna, ct);

    public async Task AgregarAsync(Tienda tienda, CancellationToken ct = default) =>
        await contexto.Tiendas.AddAsync(tienda, ct);

    private IQueryable<Tienda> ConsultaCompleta() =>
        contexto.Tiendas
            .AsNoTracking()
            .Include(t => t.MarcaTienda)
            .Include(t => t.Regional)
            .Include(t => t.TipoTienda)
            .Include(t => t.ResponsableVisual)
            .Include(t => t.Espacios).ThenInclude(e => e.TipoEspacio);
}

public class ArbolComercialRepositorio(AbastecimientoDbContext contexto) : IArbolComercialRepositorio
{
    /// <summary>
    /// Una sola SELECT (solo navegaciones de referencia). Con AsNoTracking EF no enlaza NodoPadre/NodosHijos:
    /// el arbol se arma en memoria con <c>ToLookup(n => n.NodoPadreId)</c>.
    /// </summary>
    public async Task<IReadOnlyList<NodoComercial>> ObtenerArbolCompletoAsync(CancellationToken ct = default) =>
        await contexto.NodosComerciales
            .AsNoTracking()
            .Include(n => n.ConceptoComercial).ThenInclude(c => c.NivelJerarquia)
            .OrderBy(n => n.Id)
            .ToListAsync(ct);

    public async Task AgregarConceptoAsync(ConceptoComercial concepto, CancellationToken ct = default) =>
        await contexto.ConceptosComerciales.AddAsync(concepto, ct);

    public async Task AgregarNodoAsync(NodoComercial nodo, CancellationToken ct = default) =>
        await contexto.NodosComerciales.AddAsync(nodo, ct);

    public async Task<IReadOnlyList<NivelJerarquia>> ObtenerNivelesAsync(CancellationToken ct = default) =>
        await contexto.NivelesJerarquia.AsNoTracking().OrderBy(n => n.Profundidad).ToListAsync(ct);

    public async Task<IReadOnlyList<ConceptoComercial>> ObtenerConceptosAsync(int? nivelJerarquiaId = null, CancellationToken ct = default)
    {
        var consulta = contexto.ConceptosComerciales.AsNoTracking().Include(c => c.NivelJerarquia).AsQueryable();

        if (nivelJerarquiaId is not null)
        {
            consulta = consulta.Where(c => c.NivelJerarquiaId == nivelJerarquiaId);
        }

        return await consulta
            .OrderBy(c => c.NivelJerarquia.Profundidad)
            .ThenBy(c => c.Nombre)
            .ToListAsync(ct);
    }

    public Task<ConceptoComercial?> ObtenerConceptoPorIdAsync(int id, CancellationToken ct = default) =>
        contexto.ConceptosComerciales
            .AsNoTracking()
            .Include(c => c.NivelJerarquia)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExisteConceptoAsync(int nivelJerarquiaId, string nombre, CancellationToken ct = default) =>
        contexto.ConceptosComerciales.AnyAsync(c => c.NivelJerarquiaId == nivelJerarquiaId && c.Nombre == nombre, ct);

    public Task<bool> ExisteNodoAsync(int? nodoPadreId, int conceptoComercialId, CancellationToken ct = default) =>
        contexto.NodosComerciales.AnyAsync(n => n.NodoPadreId == nodoPadreId && n.ConceptoComercialId == conceptoComercialId, ct);

    public Task<NodoComercial?> ObtenerNodoParaEditarAsync(int id, CancellationToken ct = default) =>
        contexto.NodosComerciales
            .Include(n => n.ConceptoComercial).ThenInclude(c => c.NivelJerarquia)
            .FirstOrDefaultAsync(n => n.Id == id, ct);
}

public class CatalogoRepositorio<T>(AbastecimientoDbContext contexto) : ICatalogoRepositorio<T> where T : EntidadCatalogo
{
    public async Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await contexto.Set<T>().AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(ct);

    public async Task AgregarAsync(T entidad, CancellationToken ct = default) =>
        await contexto.Set<T>().AddAsync(entidad, ct);

    public Task<bool> ExisteAsync(int id, CancellationToken ct = default) =>
        contexto.Set<T>().AnyAsync(c => c.Id == id, ct);

    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default) =>
        contexto.Set<T>().AnyAsync(c => c.Nombre == nombre, ct);
}

/// <summary>Crea repositorios de catalogo sobre el mismo <see cref="AbastecimientoDbContext"/> (misma transaccion).</summary>
public class CatalogoRepositorioFabrica(AbastecimientoDbContext contexto) : ICatalogoRepositorioFabrica
{
    public ICatalogoRepositorio<T> Obtener<T>() where T : EntidadCatalogo => new CatalogoRepositorio<T>(contexto);
}

public class TipoEspacioRepositorio(AbastecimientoDbContext contexto) : ITipoEspacioRepositorio
{
    public async Task<IReadOnlyList<TipoEspacio>> ObtenerTodosAsync(CancellationToken ct = default) =>
        await contexto.TiposEspacio.AsNoTracking().OrderBy(t => t.Id).ToListAsync(ct);

    /// <summary>
    /// Con seguimiento: las instancias se asignan como navegacion de los <see cref="EspacioTienda"/> nuevos sin que EF
    /// intente insertarlas de nuevo.
    /// </summary>
    public async Task<IReadOnlyList<TipoEspacio>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default) =>
        await contexto.TiposEspacio.Where(t => ids.Contains(t.Id)).ToListAsync(ct);

    public Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default) =>
        contexto.TiposEspacio.AnyAsync(t => t.Nombre == nombre, ct);

    public async Task AgregarAsync(TipoEspacio tipoEspacio, CancellationToken ct = default) =>
        await contexto.TiposEspacio.AddAsync(tipoEspacio, ct);
}
