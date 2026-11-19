using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Utilidades;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>
/// Arbol comercial (R2) por tienda: elegir la tienda, ver su arbol, editar porcentajes, crear conceptos, agregar
/// nodos y copiar el arbol de otra tienda cuando esta vacia.
/// </summary>
public partial class ArbolComercial
{
    private static readonly IReadOnlyList<Compartidos.Miga> Migas = [new("Inicio", ""), new("Árbol comercial")];

    private static readonly GridSort<NodoArbolRespuesta> NodoPorNombre = GridSort<NodoArbolRespuesta>.ByAscending(n => n.Nombre);
    private static readonly GridSort<NodoArbolRespuesta> NodoPorProfundidad = GridSort<NodoArbolRespuesta>.ByAscending(n => n.Profundidad);
    private static readonly GridSort<NodoArbolRespuesta> NodoPorParticipacion = GridSort<NodoArbolRespuesta>.ByAscending(n => n.PorcentajeParticipacion);
    private static readonly GridSort<NodoArbolRespuesta> NodoPorAcumulado = GridSort<NodoArbolRespuesta>.ByAscending(n => n.PorcentajeAcumulado);

    private readonly FiltroNodos filtroNodos = new();
    private bool vistaTabla;
    private bool inicializado;
    private int? tiendaSolicitada;

    /// <summary>Nodos de la vista en tabla, con los filtros aplicados (orden de la API: preorden).</summary>
    private List<NodoArbolRespuesta> NodosFiltrados => filtroNodos.Aplicar(arbol);

    private IQueryable<NodoArbolRespuesta> NodosConsulta => NodosFiltrados.AsQueryable();

    private void CambiarVista(bool tabla) => vistaTabla = tabla;

    private IReadOnlyList<TiendaRespuesta> tiendas = [];
    private IReadOnlyList<NodoArbolRespuesta> arbol = [];
    private IReadOnlyList<NivelRespuesta> niveles = [];
    private int? codigoSeleccionado;
    private bool cargando = true;
    private ExcepcionApi? error;
    private string? aviso;
    private int? idSeleccionado;
    private NodoArbolRespuesta? nodoEditando;
    private NodoArbolRespuesta? padreNuevoNodo;
    private bool mostrarNuevoNodo;
    private bool mostrarNuevoConcepto;
    private bool mostrarCopiarArbol;

    /// <summary>Codigo HANNA de la tienda pedida en la URL (<c>/arbol?tienda=5503</c>).</summary>
    [SupplyParameterFromQuery(Name = "tienda")]
    public int? Tienda { get; set; }

    /// <summary>Tienda cuyo arbol se esta viendo.</summary>
    private TiendaRespuesta? TiendaSeleccionada => tiendas.FirstOrDefault(t => t.CodigoHanna == codigoSeleccionado);

    /// <summary>Solo se puede copiar en una tienda vacia y si hay otra tienda de la que copiar.</summary>
    private bool PuedeCopiar => !cargando && arbol.Count == 0 && codigoSeleccionado is not null && tiendas.Count > 1;

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    [Inject]
    private IClienteTiendas ClienteTiendas { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (!inicializado || tiendaSolicitada != Tienda)
        {
            inicializado = true;
            tiendaSolicitada = Tienda;
            await CargarAsync();
        }
    }

    private async Task CargarAsync()
    {
        cargando = true;
        error = await LlamarApiAsync(async ct =>
        {
            var tareaTiendas = ClienteTiendas.ListarAsync(null, null, ct);
            var tareaNiveles = ClienteArbol.ObtenerNivelesAsync(ct);
            await Task.WhenAll(tareaTiendas, tareaNiveles);
            tiendas = await tareaTiendas;
            niveles = (await tareaNiveles).OrderBy(n => n.Profundidad).ToList();
            await SeleccionarTiendaInicialAsync(ct);
        });
        cargando = false;
    }

    private async Task SeleccionarTiendaInicialAsync(CancellationToken ct)
    {
        var codigo = tiendas.Any(t => t.CodigoHanna == Tienda)
            ? Tienda
            : tiendas.FirstOrDefault()?.CodigoHanna;
        codigoSeleccionado = codigo;
        arbol = codigo is int valor ? await ClienteArbol.ObtenerArbolAsync(valor, ct) : [];
    }

    private async Task CambiarTiendaAsync()
    {
        CerrarDialogos();
        await RecargarArbolAsync();
    }

    private async Task RecargarArbolAsync()
    {
        cargando = true;
        error = await LlamarApiAsync(async ct =>
        {
            arbol = codigoSeleccionado is int codigo
                ? await ClienteArbol.ObtenerArbolAsync(codigo, ct)
                : [];
        });
        cargando = false;
    }

    /// <summary>Suma de los % de los hermanos (ayuda no bloqueante; la API valida el limite).</summary>
    private decimal SumaHermanos(int? padreId, int? excluirId) =>
        arbol.Where(n => n.NodoPadreId == padreId && n.NodoId != excluirId).Sum(n => n.PorcentajeParticipacion);

    /// <summary>Ancho visual de la barra de % (presentacion: la fraccion de la API como % de ancho, entre 0 y 100).</summary>
    private static string AnchoBarra(decimal fraccion) =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{Math.Clamp(fraccion * 100m, 0m, 100m):0.##}%");

    private void AbrirEditarPorcentaje(NodoArbolRespuesta nodo)
    {
        CerrarDialogos();
        idSeleccionado = nodo.NodoId;
        nodoEditando = nodo;
    }

    private void AbrirNuevoNodo(NodoArbolRespuesta? padre)
    {
        CerrarDialogos();
        idSeleccionado = padre?.NodoId;
        padreNuevoNodo = padre;
        mostrarNuevoNodo = true;
    }

    private void AbrirNuevoConcepto()
    {
        CerrarDialogos();
        mostrarNuevoConcepto = true;
    }

    private void AbrirCopiarArbol()
    {
        CerrarDialogos();
        mostrarCopiarArbol = true;
    }

    private void CerrarDialogos()
    {
        aviso = null;
        nodoEditando = null;
        padreNuevoNodo = null;
        mostrarNuevoNodo = false;
        mostrarNuevoConcepto = false;
        mostrarCopiarArbol = false;
    }

    private async Task PorcentajeGuardadoAsync(NodoArbolRespuesta nodo)
    {
        CerrarDialogos();
        aviso = $"Porcentaje actualizado: {nodo.Ruta}.";
        idSeleccionado = nodo.NodoId;
        await RecargarArbolAsync();
    }

    private async Task NodoCreadoAsync(NodoArbolRespuesta nodo)
    {
        CerrarDialogos();
        aviso = $"Nodo creado: {nodo.Ruta}.";
        idSeleccionado = nodo.NodoId;
        await RecargarArbolAsync();
    }

    private async Task ArbolCopiadoAsync(CopiaArbolRespuesta copia)
    {
        CerrarDialogos();
        aviso = $"Árbol copiado: {Formato.Entero(copia.NodosCopiados)} nodos desde HANNA {copia.CodigoHannaOrigen}.";
        await RecargarArbolAsync();
    }

    private void ConceptoCreado(ConceptoRespuesta concepto)
    {
        CerrarDialogos();
        aviso = $"Concepto creado: {concepto.Nombre} ({concepto.Nivel.Nombre}). Ya puede agregarlo como nodo.";
    }
}
