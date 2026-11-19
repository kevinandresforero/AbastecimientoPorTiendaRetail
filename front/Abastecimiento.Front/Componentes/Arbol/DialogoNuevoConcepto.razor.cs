using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Arbol;

/// <summary>Nuevo concepto comercial; las unidades por espacio solo se piden (y se envian) en el nivel hoja.</summary>
public partial class DialogoNuevoConcepto
{
    private readonly FormularioConcepto formulario = new();
    private bool enviando;
    private ExcepcionApi? error;

    /// <summary>Niveles de la jerarquia.</summary>
    [Parameter]
    public IReadOnlyList<NivelRespuesta> Niveles { get; set; } = [];

    /// <summary>Se invoca con el concepto creado.</summary>
    [Parameter]
    public EventCallback<ConceptoRespuesta> AlGuardar { get; set; }

    /// <summary>Se invoca al cancelar.</summary>
    [Parameter]
    public EventCallback AlCancelar { get; set; }

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnParametersSet() => formulario.Niveles = Niveles;

    private async Task GuardarAsync()
    {
        if (enviando)
        {
            return;
        }

        enviando = true;
        ConceptoRespuesta? creado = null;
        error = await LlamarApiAsync(async ct =>
            creado = await ClienteArbol.CrearConceptoAsync(formulario.ASolicitud(), ct));
        enviando = false;

        if (creado is not null)
        {
            await AlGuardar.InvokeAsync(creado);
        }
    }

    private Task CancelarAsync() => AlCancelar.InvokeAsync();
}
