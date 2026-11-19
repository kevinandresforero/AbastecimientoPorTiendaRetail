using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>Indicador de carga (spinner de Bootstrap con texto).</summary>
public partial class Cargando
{
    /// <summary>Texto visible junto al spinner.</summary>
    [Parameter]
    public string Texto { get; set; } = "Cargando…";
}
