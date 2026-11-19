using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Catalogos;

/// <summary>Lista y alta de tipos de espacio (nombre + categoria mobiliario/perimetral).</summary>
public partial class PanelTiposEspacio
{
    private static readonly IReadOnlyList<CategoriaEspacio> Categorias = Enum.GetValues<CategoriaEspacio>();

    private IReadOnlyList<TipoEspacioRespuesta> tipos = [];
    private FormularioTipoEspacio formulario = new();
    private TipoEspacioRespuesta? creado;
    private bool cargando = true;
    private bool enviando;
    private ExcepcionApi? errorCarga;
    private ExcepcionApi? errorEnvio;

    [Inject]
    private IClienteTiposEspacio ClienteTiposEspacio { get; set; } = default!;

    /// <inheritdoc />
    protected override Task OnInitializedAsync() => CargarAsync();

    private async Task CargarAsync()
    {
        cargando = true;
        errorCarga = await LlamarApiAsync(async ct => tipos = await ClienteTiposEspacio.ListarAsync(ct));
        cargando = false;
    }

    private async Task GuardarAsync()
    {
        if (enviando)
        {
            return;
        }

        enviando = true;
        creado = null;
        TipoEspacioRespuesta? nuevo = null;
        errorEnvio = await LlamarApiAsync(async ct =>
            nuevo = await ClienteTiposEspacio.CrearAsync(formulario.ASolicitud(), ct));
        enviando = false;

        if (nuevo is not null)
        {
            creado = nuevo;
            tipos = [.. tipos, nuevo];
            formulario = new FormularioTipoEspacio();
        }
    }
}
