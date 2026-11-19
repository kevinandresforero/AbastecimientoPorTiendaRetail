using Abastecimiento.Front.Clientes;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>Muestra una <see cref="ExcepcionApi"/>: titulo, detalle, errores y boton opcional "Reintentar".</summary>
public partial class MensajeError
{
    /// <summary>Error a mostrar; null no muestra nada.</summary>
    [Parameter]
    public ExcepcionApi? Error { get; set; }

    /// <summary>Accion del boton "Reintentar" (sin delegado no se muestra el boton).</summary>
    [Parameter]
    public EventCallback AlReintentar { get; set; }

    /// <summary>Muestra tambien los errores por campo (fuera de un formulario).</summary>
    [Parameter]
    public bool IncluirErroresPorCampo { get; set; } = true;

    private bool MostrarReintentar => AlReintentar.HasDelegate;

    private bool MostrarErroresPorCampo => IncluirErroresPorCampo && Error is { ErroresPorCampo.Count: > 0 };

    private Task ReintentarAsync() => AlReintentar.InvokeAsync();
}
