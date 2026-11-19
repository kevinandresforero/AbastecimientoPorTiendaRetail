using Abastecimiento.Dominio.Entidades;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Tiendas;

namespace Abastecimiento.Dominio.Interfaces;

/// <summary>Acceso a tiendas. Lecturas sin seguimiento, incluyendo catalogos y espacios.</summary>
public interface ITiendaRepositorio
{
    Task<Tienda?> ObtenerPorCodigoHannaAsync(int codigoHanna, CancellationToken ct = default);

    Task<IReadOnlyList<Tienda>> ObtenerTodasAsync(int? marcaTiendaId = null, int? regionalId = null, CancellationToken ct = default);

    Task<bool> ExisteCodigoHannaAsync(int codigoHanna, CancellationToken ct = default);

    Task AgregarAsync(Tienda tienda, CancellationToken ct = default);
}

/// <summary>Acceso al arbol comercial.</summary>
public interface IArbolComercialRepositorio
{
    /// <summary>Todos los nodos con concepto y nivel en una sola SELECT (sin seguimiento).</summary>
    Task<IReadOnlyList<NodoComercial>> ObtenerArbolCompletoAsync(CancellationToken ct = default);

    Task AgregarConceptoAsync(ConceptoComercial concepto, CancellationToken ct = default);

    Task AgregarNodoAsync(NodoComercial nodo, CancellationToken ct = default);

    /// <summary>Niveles de la jerarquia ordenados por profundidad.</summary>
    Task<IReadOnlyList<NivelJerarquia>> ObtenerNivelesAsync(CancellationToken ct = default);

    /// <summary>Conceptos con su nivel, opcionalmente filtrados por nivel.</summary>
    Task<IReadOnlyList<ConceptoComercial>> ObtenerConceptosAsync(int? nivelJerarquiaId = null, CancellationToken ct = default);

    /// <summary>Concepto con su nivel, o null si no existe.</summary>
    Task<ConceptoComercial?> ObtenerConceptoPorIdAsync(int id, CancellationToken ct = default);

    Task<bool> ExisteConceptoAsync(int nivelJerarquiaId, string nombre, CancellationToken ct = default);

    Task<bool> ExisteNodoAsync(int? nodoPadreId, int conceptoComercialId, CancellationToken ct = default);

    /// <summary>Nodo con seguimiento (para modificarlo), incluyendo concepto y nivel.</summary>
    Task<NodoComercial?> ObtenerNodoParaEditarAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Arbol de una tienda (sus nodos con concepto y nivel) en una sola SELECT, sin seguimiento y ordenado por Id.
    /// Tienda sin nodos → lista vacia.
    /// </summary>
    Task<IReadOnlyList<NodoComercial>> ObtenerArbolPorTiendaAsync(int tiendaId, CancellationToken ct = default);

    /// <summary>
    /// Indica si el concepto ya cuelga del padre dentro de la tienda; con <paramref name="nodoPadreId"/> null valida
    /// una unidad de negocio repetida en la tienda.
    /// </summary>
    Task<bool> ExisteNodoAsync(int tiendaId, int? nodoPadreId, int conceptoComercialId, CancellationToken ct = default);

    /// <summary>
    /// Alta en lote (ej. copia del arbol de una tienda a otra). Los nodos nuevos se enlazan por la navegacion
    /// <c>NodoPadre</c> y se guardan con un solo <c>GuardarCambiosAsync</c>.
    /// </summary>
    Task AgregarNodosAsync(IEnumerable<NodoComercial> nodos, CancellationToken ct = default);
}

/// <summary>Acceso generico a catalogos simples (Id + Nombre).</summary>
public interface ICatalogoRepositorio<T> where T : EntidadCatalogo
{
    Task<IReadOnlyList<T>> ObtenerTodosAsync(CancellationToken ct = default);

    Task AgregarAsync(T entidad, CancellationToken ct = default);

    Task<bool> ExisteAsync(int id, CancellationToken ct = default);

    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);
}

/// <summary>
/// Entrega el repositorio de un catalogo cuyo tipo solo se conoce al invocar un metodo generico
/// (ej. <c>ListarAsync&lt;T&gt;</c>). Evita que la capa de aplicacion resuelva dependencias del contenedor.
/// </summary>
public interface ICatalogoRepositorioFabrica
{
    ICatalogoRepositorio<T> Obtener<T>() where T : EntidadCatalogo;
}

/// <summary>Acceso al catalogo de tipos de espacio (mobiliario y perimetrales).</summary>
public interface ITipoEspacioRepositorio
{
    Task<IReadOnlyList<TipoEspacio>> ObtenerTodosAsync(CancellationToken ct = default);

    Task<IReadOnlyList<TipoEspacio>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);

    Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);

    Task AgregarAsync(TipoEspacio tipoEspacio, CancellationToken ct = default);
}

/// <summary>Confirma los cambios pendientes en una sola transaccion.</summary>
public interface IUnidadDeTrabajo
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
