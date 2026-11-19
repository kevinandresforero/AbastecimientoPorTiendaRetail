using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>Ventana modal de Bootstrap renderizada por Blazor (sin JavaScript).</summary>
public partial class Dialogo
{
    /// <summary>Titulo del dialogo.</summary>
    [Parameter, EditorRequired]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Contenido (normalmente un EditForm).</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private string IdTitulo { get; } = $"dialogo-{Guid.NewGuid():N}";
}
