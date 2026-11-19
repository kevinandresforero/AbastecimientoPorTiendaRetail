using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Catalogos;

/// <summary>Lista y alta de un catalogo simple; el elemento creado se agrega a la lista sin recargar.</summary>
public partial class PanelCatalogo
{
    private IReadOnlyList<CatalogoRespuesta> elementos = [];
    private IFormularioNombre formulario = new FormularioCatalogo();
    private CatalogoRespuesta? creado;
    private bool cargando = true;
    private bool enviando;
    private ExcepcionApi? errorCarga;
    private ExcepcionApi? errorEnvio;

    /// <summary>Catalogo a gestionar.</summary>
    [Parameter, EditorRequired]
    public TipoCatalogo Tipo { get; set; }

    /// <summary>Titulo de la lista.</summary>
    [Parameter]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Nombre en singular para "Agregar ...".</summary>
    [Parameter]
    public string Singular { get; set; } = string.Empty;

    /// <summary>Longitud maxima del nombre (50, o 100 para responsables visuales).</summary>
    [Parameter]
    public int LongitudMaxima { get; set; } = 50;

    [Inject]
    private IClienteCatalogos ClienteCatalogos { get; set; } = default!;

    private string IdCampo => $"nombre-{Tipo}";

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        formulario = NuevoFormulario();
        await CargarAsync();
    }

    private IFormularioNombre NuevoFormulario() =>
        LongitudMaxima > 50 ? new FormularioResponsableVisual() : new FormularioCatalogo();

    private async Task CargarAsync()
    {
        cargando = true;
        errorCarga = await LlamarApiAsync(async ct => elementos = await ClienteCatalogos.ListarAsync(Tipo, ct));
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
        CatalogoRespuesta? nuevo = null;
        errorEnvio = await LlamarApiAsync(async ct =>
            nuevo = await ClienteCatalogos.CrearAsync(Tipo, new CrearCatalogoSolicitud((formulario.Nombre ?? string.Empty).Trim()), ct));
        enviando = false;

        if (nuevo is not null)
        {
            creado = nuevo;
            elementos = [.. elementos, nuevo];
            formulario = NuevoFormulario();
        }
    }
}
