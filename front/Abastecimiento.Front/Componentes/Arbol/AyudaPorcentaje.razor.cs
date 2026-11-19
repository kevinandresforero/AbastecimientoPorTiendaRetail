using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Arbol;

/// <summary>Ayuda no bloqueante: suma de los % de los hermanos y lo que queda hasta 100%.</summary>
public partial class AyudaPorcentaje
{
    /// <summary>Suma de los % (fraccion) de los hermanos, sin el nodo editado.</summary>
    [Parameter]
    public decimal SumaHermanos { get; set; }

    private decimal Disponible => Math.Max(0m, 1m - SumaHermanos);
}
