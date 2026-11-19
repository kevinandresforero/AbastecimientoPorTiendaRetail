using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>
/// Lista jerarquica indentada, generica para <c>NodoArbolRespuesta</c> y <c>NodoCalculadoRespuesta</c>: ordena en
/// preorden a partir del id del padre, sangra por profundidad y permite expandir/contraer cada rama.
/// </summary>
public partial class ArbolNodos<TNodo>
{
    private readonly HashSet<int> contraidos = [];

    /// <summary>Nodos a mostrar (en cualquier orden).</summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<TNodo> Nodos { get; set; } = [];

    /// <summary>Id del nodo.</summary>
    [Parameter, EditorRequired]
    public Func<TNodo, int> ObtenerId { get; set; } = default!;

    /// <summary>Id del padre (null en la raiz).</summary>
    [Parameter, EditorRequired]
    public Func<TNodo, int?> ObtenerPadreId { get; set; } = default!;

    /// <summary>Nombre visible.</summary>
    [Parameter, EditorRequired]
    public Func<TNodo, string> ObtenerNombre { get; set; } = default!;

    /// <summary>Texto pequeño opcional bajo el nombre (ej. el nivel del nodo).</summary>
    [Parameter]
    public Func<TNodo, string?>? ObtenerDetalle { get; set; }

    /// <summary>Profundidad del nodo (1 = raiz); si falta se deduce de la posicion en el arbol.</summary>
    [Parameter]
    public Func<TNodo, int>? ObtenerProfundidad { get; set; }

    /// <summary>Encabezados extra (<c>th</c>) despues de "Nombre".</summary>
    [Parameter]
    public RenderFragment? Encabezados { get; set; }

    /// <summary>Celdas extra (<c>td</c>) de cada nodo.</summary>
    [Parameter]
    public RenderFragment<TNodo>? Columnas { get; set; }

    /// <summary>Nombre accesible de la tabla.</summary>
    [Parameter]
    public string Titulo { get; set; } = "Árbol";

    /// <summary>Nodo resaltado.</summary>
    [Parameter]
    public int? IdSeleccionado { get; set; }

    private IReadOnlyList<FilaArbol> Filas { get; set; } = [];

    private IEnumerable<FilaArbol> FilasVisibles => Filas.Where(f => !f.Ancestros.Any(contraidos.Contains));

    /// <inheritdoc />
    protected override void OnParametersSet() => Filas = OrdenarEnPreorden();

    private List<FilaArbol> OrdenarEnPreorden()
    {
        var ids = Nodos.Select(ObtenerId).ToHashSet();
        var hijos = Nodos
            .Where(n => ObtenerPadreId(n) is int padre && ids.Contains(padre))
            .GroupBy(n => ObtenerPadreId(n)!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
        var resultado = new List<FilaArbol>(Nodos.Count);
        var visitados = new HashSet<int>();

        void Visitar(TNodo nodo, int profundidadCalculada, IReadOnlyList<int> ancestros)
        {
            var id = ObtenerId(nodo);
            if (!visitados.Add(id))
            {
                return;
            }

            var tieneHijos = hijos.TryGetValue(id, out var deEsteNodo);
            var profundidad = ObtenerProfundidad?.Invoke(nodo) ?? profundidadCalculada;
            resultado.Add(new FilaArbol(nodo, id, ObtenerNombre(nodo), profundidad, tieneHijos, ancestros));
            if (tieneHijos)
            {
                var ancestrosHijos = ancestros.Append(id).ToList();
                foreach (var hijo in deEsteNodo!)
                {
                    Visitar(hijo, profundidadCalculada + 1, ancestrosHijos);
                }
            }
        }

        foreach (var raiz in Nodos.Where(n => ObtenerPadreId(n) is not int padre || !ids.Contains(padre)))
        {
            Visitar(raiz, 1, []);
        }

        return resultado;
    }

    private bool EstaContraido(FilaArbol fila) => contraidos.Contains(fila.Id);

    private void Alternar(FilaArbol fila)
    {
        if (!contraidos.Remove(fila.Id))
        {
            contraidos.Add(fila.Id);
        }
    }

    private void ExpandirTodo() => contraidos.Clear();

    private void ContraerTodo()
    {
        foreach (var fila in Filas.Where(f => f.TieneHijos))
        {
            contraidos.Add(fila.Id);
        }
    }

    private static string Sangria(FilaArbol fila) =>
        string.Create(CultureInfo.InvariantCulture, $"{Math.Max(0, fila.Profundidad - 1) * 1.25m}rem");

    private string? ClaseFila(FilaArbol fila) => fila.Id == IdSeleccionado ? "table-active" : null;

    /// <summary>Nodo ya ubicado en el preorden.</summary>
    private sealed record FilaArbol(
        TNodo Nodo, int Id, string Nombre, int Profundidad, bool TieneHijos, IReadOnlyList<int> Ancestros);
}
