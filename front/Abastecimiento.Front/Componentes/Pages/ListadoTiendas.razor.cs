using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>Listado de tiendas con filtros por marca y regional (aplicados en la query de la API).</summary>
public partial class ListadoTiendas
{
    private static readonly IReadOnlyList<Compartidos.Miga> Migas = [new("Inicio", ""), new("Tiendas")];

    private readonly FormularioFiltroTiendas filtros = new();
    private IReadOnlyList<CatalogoRespuesta> marcas = [];
    private IReadOnlyList<CatalogoRespuesta> regionales = [];
    private IReadOnlyList<TiendaRespuesta>? tiendas;
    private bool cargando = true;
    private ExcepcionApi? error;

    [Inject]
    private IClienteTiendas ClienteTiendas { get; set; } = default!;

    [Inject]
    private IClienteCatalogos ClienteCatalogos { get; set; } = default!;

    private IQueryable<TiendaRespuesta> TiendasConsulta => (tiendas ?? []).AsQueryable();

    /// <inheritdoc />
    protected override Task OnInitializedAsync() => CargarAsync();

    private async Task CargarAsync()
    {
        cargando = true;
        error = await LlamarApiAsync(async ct =>
        {
            var tareaMarcas = ClienteCatalogos.ListarAsync(TipoCatalogo.MarcasTienda, ct);
            var tareaRegionales = ClienteCatalogos.ListarAsync(TipoCatalogo.Regionales, ct);
            var tareaTiendas = ClienteTiendas.ListarAsync(filtros.MarcaTiendaId, filtros.RegionalId, ct);
            await Task.WhenAll(tareaMarcas, tareaRegionales, tareaTiendas);
            marcas = await tareaMarcas;
            regionales = await tareaRegionales;
            tiendas = await tareaTiendas;
        });
        cargando = false;
    }

    private async Task CargarTiendasAsync()
    {
        cargando = true;
        error = await LlamarApiAsync(async ct =>
            tiendas = await ClienteTiendas.ListarAsync(filtros.MarcaTiendaId, filtros.RegionalId, ct));
        cargando = false;
    }

    private static string RutaDetalle(TiendaRespuesta tienda) => $"tiendas/{tienda.CodigoHanna}";
}
