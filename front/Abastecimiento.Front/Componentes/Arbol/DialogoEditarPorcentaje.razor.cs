using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Abastecimiento.Front.Utilidades;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Arbol;

/// <summary>Editar el % de participacion de un nodo: el usuario escribe en % y se envia la fraccion.</summary>
public partial class DialogoEditarPorcentaje
{
    private static readonly IReadOnlyDictionary<string, string> AliasCampos = new Dictionary<string, string>
    {
        ["PorcentajeParticipacion"] = nameof(FormularioPorcentaje.Porcentaje)
    };

    private readonly FormularioPorcentaje formulario = new();
    private int? nodoInicializado;
    private bool enviando;
    private ExcepcionApi? error;

    /// <summary>Nodo a editar.</summary>
    [Parameter, EditorRequired]
    public NodoArbolRespuesta Nodo { get; set; } = default!;

    /// <summary>Suma de los % de los hermanos (sin este nodo).</summary>
    [Parameter]
    public decimal SumaHermanos { get; set; }

    /// <summary>Se invoca con el nodo actualizado por la API.</summary>
    [Parameter]
    public EventCallback<NodoArbolRespuesta> AlGuardar { get; set; }

    /// <summary>Se invoca al cancelar.</summary>
    [Parameter]
    public EventCallback AlCancelar { get; set; }

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (nodoInicializado != Nodo.NodoId)
        {
            nodoInicializado = Nodo.NodoId;
            formulario.Porcentaje = Formato.PorcentajeEditable(Nodo.PorcentajeParticipacion);
        }
    }

    private async Task GuardarAsync()
    {
        if (enviando)
        {
            return;
        }

        enviando = true;
        NodoArbolRespuesta? actualizado = null;
        error = await LlamarApiAsync(async ct =>
            actualizado = await ClienteArbol.ActualizarPorcentajeAsync(Nodo.NodoId, formulario.Fraccion, ct));
        enviando = false;

        if (actualizado is not null)
        {
            await AlGuardar.InvokeAsync(actualizado);
        }
    }

    private Task CancelarAsync() => AlCancelar.InvokeAsync();
}
