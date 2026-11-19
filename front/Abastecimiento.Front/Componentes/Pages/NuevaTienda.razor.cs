using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>
/// Alta de tienda: carga los catalogos, valida la forma del formulario y envia a la API. 201 → detalle con aviso;
/// 400/409 → errores junto al campo o en el resumen.
/// </summary>
public partial class NuevaTienda
{
    private static readonly IReadOnlyList<Compartidos.Miga> Migas =
        [new("Inicio", ""), new("Tiendas", "tiendas"), new("Nueva tienda")];

    private readonly FormularioTienda formulario = new();
    private IReadOnlyList<CatalogoRespuesta> marcas = [];
    private IReadOnlyList<CatalogoRespuesta> regionales = [];
    private IReadOnlyList<CatalogoRespuesta> tiposTienda = [];
    private IReadOnlyList<TipoEspacioRespuesta> tiposEspacio = [];
    private bool cargando = true;
    private bool enviando;
    private ExcepcionApi? errorCarga;
    private ExcepcionApi? errorEnvio;

    [Inject]
    private IClienteTiendas ClienteTiendas { get; set; } = default!;

    [Inject]
    private IClienteCatalogos ClienteCatalogos { get; set; } = default!;

    [Inject]
    private IClienteTiposEspacio ClienteTiposEspacio { get; set; } = default!;

    [Inject]
    private NavigationManager Navegacion { get; set; } = default!;

    /// <inheritdoc />
    protected override Task OnInitializedAsync() => CargarCatalogosAsync();

    private async Task CargarCatalogosAsync()
    {
        cargando = true;
        errorCarga = await LlamarApiAsync(async ct =>
        {
            var tareaMarcas = ClienteCatalogos.ListarAsync(TipoCatalogo.MarcasTienda, ct);
            var tareaRegionales = ClienteCatalogos.ListarAsync(TipoCatalogo.Regionales, ct);
            var tareaTipos = ClienteCatalogos.ListarAsync(TipoCatalogo.TiposTienda, ct);
            var tareaEspacios = ClienteTiposEspacio.ListarAsync(ct);
            await Task.WhenAll(tareaMarcas, tareaRegionales, tareaTipos, tareaEspacios);
            marcas = await tareaMarcas;
            regionales = await tareaRegionales;
            tiposTienda = await tareaTipos;
            tiposEspacio = await tareaEspacios;
            formulario.TiposEspacio = tiposEspacio;
        });
        cargando = false;
    }

    private void AgregarFila() => formulario.Espacios.Add(new FilaEspacio());

    private void QuitarFila(FilaEspacio fila)
    {
        if (formulario.Espacios.Count > 1)
        {
            formulario.Espacios.Remove(fila);
        }
    }

    private async Task GuardarAsync()
    {
        if (enviando)
        {
            return;
        }

        enviando = true;
        errorEnvio = null;
        TiendaRespuesta? creada = null;
        errorEnvio = await LlamarApiAsync(async ct =>
            creada = await ClienteTiendas.CrearAsync(formulario.ASolicitud(), ct));
        enviando = false;

        if (creada is not null)
        {
            Navegacion.NavigateTo($"tiendas/{creada.CodigoHanna}?creada=true");
        }
    }
}
