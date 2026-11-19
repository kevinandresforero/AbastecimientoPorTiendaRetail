using System.Diagnostics;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>Pagina de error no controlado (solo fuera de Development).</summary>
public partial class Error
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? IdSolicitud { get; set; }

    private bool MostrarIdSolicitud => !string.IsNullOrEmpty(IdSolicitud);

    /// <inheritdoc />
    protected override void OnInitialized() =>
        IdSolicitud = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}
