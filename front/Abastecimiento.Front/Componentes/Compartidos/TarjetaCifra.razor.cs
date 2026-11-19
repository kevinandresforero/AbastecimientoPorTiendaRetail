using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>Tarjeta con una cifra destacada (KPI). El valor llega ya formateado: no calcula nada.</summary>
public partial class TarjetaCifra
{
    /// <summary>Etiqueta sobre la cifra.</summary>
    [Parameter, EditorRequired]
    public string Etiqueta { get; set; } = string.Empty;

    /// <summary>Cifra formateada.</summary>
    [Parameter, EditorRequired]
    public string Valor { get; set; } = string.Empty;

    /// <summary>Texto pequeño bajo la cifra.</summary>
    [Parameter]
    public string? Detalle { get; set; }

    /// <summary>Valor completo para el tooltip.</summary>
    [Parameter]
    public string? Tooltip { get; set; }

    /// <summary>Nombre del <see cref="Compartidos.Icono"/>.</summary>
    [Parameter]
    public string? NombreIcono { get; set; }

    /// <summary>Fondo verde para la cifra principal.</summary>
    [Parameter]
    public bool Destacada { get; set; }
}
