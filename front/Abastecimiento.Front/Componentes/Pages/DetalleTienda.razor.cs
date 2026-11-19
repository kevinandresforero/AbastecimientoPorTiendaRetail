using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>Detalle de una tienda (datos y espacios) y su carga calculada por la API (R3, R4a).</summary>
public partial class DetalleTienda
{
    private int? codigoCargado;
    private TiendaRespuesta? tienda;
    private CargaTiendaRespuesta? carga;
    private IReadOnlyList<NivelRespuesta> niveles = [];
    private bool cargandoTienda = true;
    private bool cargandoCarga;
    private bool noEncontrada;
    private ExcepcionApi? errorTienda;
    private ExcepcionApi? errorCarga;

    /// <summary>Codigo HANNA de la ruta.</summary>
    [Parameter]
    public int CodigoHanna { get; set; }

    /// <summary>True al llegar desde "Nueva tienda" (muestra el aviso "Tienda creada").</summary>
    [SupplyParameterFromQuery(Name = "creada")]
    public bool Creada { get; set; }

    [Inject]
    private IClienteTiendas ClienteTiendas { get; set; } = default!;

    private IReadOnlyList<Compartidos.Miga> Migas =>
        [new("Inicio", ""), new("Tiendas", "tiendas"), new($"HANNA {CodigoHanna}")];

    [Inject]
    private IClienteCarga ClienteCarga { get; set; } = default!;

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (codigoCargado != CodigoHanna)
        {
            codigoCargado = CodigoHanna;
            await CargarAsync();
        }
    }

    private async Task CargarAsync()
    {
        cargandoTienda = true;
        noEncontrada = false;
        tienda = null;
        carga = null;
        errorCarga = null;
        errorTienda = await LlamarApiAsync(async ct => tienda = await ClienteTiendas.ObtenerAsync(CodigoHanna, ct));
        if (errorTienda is { EsNoEncontrado: true })
        {
            noEncontrada = true;
            errorTienda = null;
        }

        cargandoTienda = false;
        if (tienda is not null)
        {
            StateHasChanged();
            await CargarCargaAsync();
        }
    }

    private async Task CargarCargaAsync()
    {
        cargandoCarga = true;
        errorCarga = await LlamarApiAsync(async ct =>
        {
            var tareaCarga = ClienteCarga.CalcularAsync(CodigoHanna, ct);
            var tareaNiveles = ClienteArbol.ObtenerNivelesAsync(ct);
            await Task.WhenAll(tareaCarga, tareaNiveles);
            carga = await tareaCarga;
            niveles = await tareaNiveles;
        });
        cargandoCarga = false;
    }
}
