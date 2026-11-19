using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Arbol;

/// <summary>Copia el arbol de una tienda origen en la tienda destino (que debe estar vacia).</summary>
public partial class DialogoCopiarArbol
{
    private int? codigoOrigen;
    private bool enviando;
    private ExcepcionApi? error;

    /// <summary>Tienda destino (sin arbol) que recibe la copia.</summary>
    [Parameter, EditorRequired]
    public int CodigoHannaDestino { get; set; }

    /// <summary>Tiendas de las que se puede copiar (se excluye la destino).</summary>
    [Parameter]
    public IReadOnlyList<TiendaRespuesta> Tiendas { get; set; } = [];

    /// <summary>Se invoca con el resultado de la copia.</summary>
    [Parameter]
    public EventCallback<CopiaArbolRespuesta> AlCopiar { get; set; }

    /// <summary>Se invoca al cancelar.</summary>
    [Parameter]
    public EventCallback AlCancelar { get; set; }

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    private IEnumerable<TiendaRespuesta> Origenes => Tiendas.Where(t => t.CodigoHanna != CodigoHannaDestino);

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (codigoOrigen is null || codigoOrigen == CodigoHannaDestino)
        {
            codigoOrigen = Origenes.FirstOrDefault()?.CodigoHanna;
        }
    }

    private async Task CopiarAsync()
    {
        if (enviando || codigoOrigen is not int origen)
        {
            return;
        }

        enviando = true;
        error = null;
        CopiaArbolRespuesta? copia = null;
        error = await LlamarApiAsync(async ct =>
            copia = await ClienteArbol.CopiarArbolAsync(CodigoHannaDestino, origen, ct));
        enviando = false;

        if (copia is not null)
        {
            await AlCopiar.InvokeAsync(copia);
        }
    }

    private Task CancelarAsync() => AlCancelar.InvokeAsync();
}
