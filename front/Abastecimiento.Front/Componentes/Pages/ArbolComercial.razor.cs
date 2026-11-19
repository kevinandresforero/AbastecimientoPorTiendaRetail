using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Utilidades;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>Arbol comercial (R2): ver el arbol, editar porcentajes, crear conceptos y agregar nodos.</summary>
public partial class ArbolComercial
{
    private static readonly IReadOnlyList<Compartidos.Miga> Migas = [new("Inicio", ""), new("Árbol comercial")];

    private static readonly GridSort<NodoArbolRespuesta> NodoPorNombre = GridSort<NodoArbolRespuesta>.ByAscending(n => n.Nombre);
    private static readonly GridSort<NodoArbolRespuesta> NodoPorProfundidad = GridSort<NodoArbolRespuesta>.ByAscending(n => n.Profundidad);
    private static readonly GridSort<NodoArbolRespuesta> NodoPorParticipacion = GridSort<NodoArbolRespuesta>.ByAscending(n => n.PorcentajeParticipacion);
    private static readonly GridSort<NodoArbolRespuesta> NodoPorAcumulado = GridSort<NodoArbolRespuesta>.ByAscending(n => n.PorcentajeAcumulado);

    private readonly FiltroNodos filtroNodos = new();
    private bool vistaTabla;

    /// <summary>Nodos de la vista en tabla, con los filtros aplicados (orden de la API: preorden).</summary>
    private List<NodoArbolRespuesta> NodosFiltrados => filtroNodos.Aplicar(arbol);

    private IQueryable<NodoArbolRespuesta> NodosConsulta => NodosFiltrados.AsQueryable();

    private void CambiarVista(bool tabla) => vistaTabla = tabla;

    private IReadOnlyList<NodoArbolRespuesta> arbol = [];
    private IReadOnlyList<NivelRespuesta> niveles = [];
    private bool cargando = true;
    private ExcepcionApi? error;
    private string? aviso;
    private int? idSeleccionado;
    private NodoArbolRespuesta? nodoEditando;
    private NodoArbolRespuesta? padreNuevoNodo;
    private bool mostrarNuevoNodo;
    private bool mostrarNuevoConcepto;

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    /// <inheritdoc />
    protected override Task OnInitializedAsync() => CargarAsync();

    private async Task CargarAsync()
    {
        cargando = true;
        error = await LlamarApiAsync(async ct =>
        {
            var tareaArbol = ClienteArbol.ObtenerArbolAsync(ct);
            var tareaNiveles = ClienteArbol.ObtenerNivelesAsync(ct);
            await Task.WhenAll(tareaArbol, tareaNiveles);
            arbol = await tareaArbol;
            niveles = (await tareaNiveles).OrderBy(n => n.Profundidad).ToList();
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

    private void CerrarDialogos()
    {
        aviso = null;
        nodoEditando = null;
        padreNuevoNodo = null;
        mostrarNuevoNodo = false;
        mostrarNuevoConcepto = false;
    }

    private async Task PorcentajeGuardadoAsync(NodoArbolRespuesta nodo)
    {
        CerrarDialogos();
        aviso = $"Porcentaje actualizado: {nodo.Ruta}.";
        idSeleccionado = nodo.NodoId;
        await CargarAsync();
    }

    private async Task NodoCreadoAsync(NodoArbolRespuesta nodo)
    {
        CerrarDialogos();
        aviso = $"Nodo creado: {nodo.Ruta}.";
        idSeleccionado = nodo.NodoId;
        await CargarAsync();
    }

    private void ConceptoCreado(ConceptoRespuesta concepto)
    {
        CerrarDialogos();
        aviso = $"Concepto creado: {concepto.Nombre} ({concepto.Nivel.Nombre}). Ya puede agregarlo como nodo.";
    }
}
