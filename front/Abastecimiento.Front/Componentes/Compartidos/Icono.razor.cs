using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>
/// Icono SVG de trazo, decorativo (<c>aria-hidden</c>): siempre acompaña a un texto visible. Toma el color del texto
/// (<c>currentColor</c>), asi que no introduce colores propios.
/// </summary>
public partial class Icono
{
    private static readonly IReadOnlyDictionary<string, string[]> Iconos = new Dictionary<string, string[]>
    {
        ["inicio"] = ["M3 11l9-8 9 8", "M5 10v10h14V10", "M10 20v-6h4v6"],
        ["tiendas"] = ["M4 4h16l1.5 5h-19z", "M5 9v11h14V9", "M9.5 20v-5.5h5V20"],
        ["arbol"] = ["M9 3h6v5H9z", "M3 16h6v5H3z", "M15 16h6v5h-6z", "M12 8v4", "M6 16v-4h12v4"],
        ["reportes"] = ["M4 20h16", "M7 16v-5", "M12 16V6", "M17 16v-8"],
        ["catalogos"] = ["M9 6h11", "M9 12h11", "M9 18h11", "M4.5 6h.01", "M4.5 12h.01", "M4.5 18h.01"],
        ["buscar"] = ["M11 4a7 7 0 1 0 0 14a7 7 0 0 0 0-14z", "M20 20l-4.2-4.2"],
        ["mas"] = ["M12 5v14", "M5 12h14"],
        ["espacios"] = ["M4 4h7v7H4z", "M13 4h7v7h-7z", "M4 13h7v7H4z", "M13 13h7v7h-7z"],
        ["carga"] = ["M3 7.5l9-4.5 9 4.5-9 4.5z", "M3 7.5v9l9 4.5 9-4.5v-9", "M12 12v9"],
        ["menu"] = ["M4 6h16", "M4 12h16", "M4 18h16"],
        ["flecha"] = ["M5 12h14", "M13 6l6 6-6 6"],
        ["editar"] = ["M4 20h4L19 9l-4-4L4 16z", "M13.5 6.5l4 4"],
        ["capas"] = ["M12 3l9 5-9 5-9-5z", "M3 13l9 5 9-5"],
        ["producto"] = ["M6 3h12l-1 18H7z", "M9 7h6"]
    };

    /// <summary>Nombre del icono (inicio, tiendas, arbol, reportes, catalogos, buscar, mas, espacios, carga, menu,
    /// flecha, editar, capas, producto).</summary>
    [Parameter, EditorRequired]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Clases CSS extra.</summary>
    [Parameter]
    public string? Clase { get; set; }

    private IReadOnlyList<string> Trazos => Iconos.TryGetValue(Nombre, out var trazos) ? trazos : [];
}
