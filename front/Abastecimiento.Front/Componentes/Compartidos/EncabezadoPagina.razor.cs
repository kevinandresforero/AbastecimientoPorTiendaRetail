using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>Encabezado comun de las paginas: ruta de navegacion, antetitulo, titulo (h1), descripcion y acciones.</summary>
public partial class EncabezadoPagina
{
    /// <summary>Titulo de la pagina (h1).</summary>
    [Parameter, EditorRequired]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Texto corto sobre el titulo (ej. "HANNA 5503").</summary>
    [Parameter]
    public string? Antetitulo { get; set; }

    /// <summary>Descripcion bajo el titulo.</summary>
    [Parameter]
    public string? Descripcion { get; set; }

    /// <summary>Ruta de navegacion; el ultimo elemento suele no tener enlace.</summary>
    [Parameter]
    public IReadOnlyList<Miga> Migas { get; set; } = [];

    /// <summary>Botones a la derecha del titulo.</summary>
    [Parameter]
    public RenderFragment? Acciones { get; set; }

    /// <summary>Contenido bajo el titulo (ej. chips con datos).</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}

/// <summary>Elemento de la ruta de navegacion; <see cref="Ruta"/> null = pagina actual.</summary>
public sealed record Miga(string Texto, string? Ruta = null);
