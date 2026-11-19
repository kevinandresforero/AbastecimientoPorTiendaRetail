using System.Globalization;
using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Utilidades;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>
/// Explorador de cargas: pide las tiendas y la carga de cada una (<c>GET /api/carga/{hanna}</c>, maximo
/// <see cref="LlamadasEnParalelo"/> a la vez, con progreso visible) y las muestra en una tabla filtrable y ordenable.
/// Solo cruza y filtra datos de la API: el calculo lo hace el backend.
/// </summary>
public partial class Explorador
{
    /// <summary>Llamadas simultaneas a <c>/api/carga</c> para no saturar la API.</summary>
    public const int LlamadasEnParalelo = 4;

    private static readonly IReadOnlyList<Compartidos.Miga> Migas = [new("Inicio", ""), new("Explorador de cargas")];

    private static readonly GridSort<FilaExplorador> PorTienda = GridSort<FilaExplorador>.ByAscending(f => f.Tienda.CodigoHanna);
    private static readonly GridSort<FilaExplorador> PorRegional = GridSort<FilaExplorador>.ByAscending(f => f.Tienda.Regional.Nombre);
    private static readonly GridSort<FilaExplorador> PorTipo = GridSort<FilaExplorador>
        .ByAscending(f => f.MarcaProducto == null ? "" : f.MarcaProducto.Nombre)
        .ThenAscending(f => f.Perfil == null ? "" : f.Perfil.Nombre)
        .ThenAscending(f => f.Linea.TipoProducto);
    private static readonly GridSort<FilaExplorador> PorEspacios = GridSort<FilaExplorador>.ByAscending(f => f.Linea.EspaciosCalculados);
    private static readonly GridSort<FilaExplorador> PorUnidades = GridSort<FilaExplorador>.ByAscending(f => f.Linea.UnidadesPorEspacio);
    private static readonly GridSort<FilaExplorador> PorCarga = GridSort<FilaExplorador>.ByAscending(f => f.Linea.CargaTotal);

    private readonly FiltroCargas filtro = new();
    private readonly List<(TiendaRespuesta Tienda, ExcepcionApi Error)> erroresTiendas = [];
    private List<FilaExplorador> todas = [];
    private List<FilaExplorador> filtradas = [];
    private List<GrupoProducto> grupos = [];
    private HashSet<string>? abiertos;
    private bool vistaPorProducto;
    private IReadOnlyList<TiendaRespuesta> tiendas = [];
    private IReadOnlyList<NivelRespuesta> niveles = [];
    private IReadOnlyList<CatalogoRespuesta> regionales = [];
    private IReadOnlyList<CatalogoRespuesta> marcas = [];
    private List<string> conceptos = [];
    private bool cargando = true;
    private int totalTiendas;
    private int tiendasListas;
    private ExcepcionApi? error;

    [Inject]
    private IClienteTiendas ClienteTiendas { get; set; } = default!;

    [Inject]
    private IClienteCarga ClienteCarga { get; set; } = default!;

    [Inject]
    private IClienteArbolComercial ClienteArbol { get; set; } = default!;

    private IQueryable<FilaExplorador> FilasConsulta => filtradas.AsQueryable();

    private int TiendasFiltradas => filtradas.Select(f => f.Tienda.CodigoHanna).Distinct().Count();

    private decimal EspaciosFiltrados => filtradas.Sum(f => f.Linea.EspaciosCalculados);

    private int CargaFiltrada => filtradas.Sum(f => f.Linea.CargaTotal);

    private string AnchoProgreso => totalTiendas == 0
        ? "35%"
        : string.Create(CultureInfo.InvariantCulture, $"{Math.Max(4, tiendasListas * 100 / totalTiendas)}%");

    /// <inheritdoc />
    protected override Task OnInitializedAsync() => CargarAsync();

    private async Task CargarAsync()
    {
        cargando = true;
        totalTiendas = 0;
        tiendasListas = 0;
        erroresTiendas.Clear();
        var filas = new List<FilaExplorador>();

        error = await LlamarApiAsync(async ct =>
        {
            var tareaTiendas = ClienteTiendas.ListarAsync(null, null, ct);
            var tareaNiveles = ClienteArbol.ObtenerNivelesAsync(ct);
            await Task.WhenAll(tareaTiendas, tareaNiveles);
            tiendas = (await tareaTiendas).OrderBy(t => t.CodigoHanna).ToList();
            niveles = (await tareaNiveles).OrderBy(n => n.Profundidad).ToList();
        });

        if (error is null)
        {
            totalTiendas = tiendas.Count;
            StateHasChanged();
            using var limite = new SemaphoreSlim(LlamadasEnParalelo);
            await Task.WhenAll(tiendas.Select(tienda => CargarTiendaAsync(tienda, filas, limite)));

            todas = filas;
            regionales = Distintos(tiendas.Select(t => t.Regional));
            marcas = Distintos(tiendas.Select(t => t.MarcaTienda));
            filtro.Nivel ??= niveles.LastOrDefault(n => n.EsHoja)?.Nombre ?? niveles.LastOrDefault()?.Nombre;
            ActualizarConceptos();
            Filtrar();
        }

        cargando = false;
    }

    private async Task CargarTiendaAsync(TiendaRespuesta tienda, List<FilaExplorador> filas, SemaphoreSlim limite)
    {
        await limite.WaitAsync(Cancelacion);
        try
        {
            CargaTiendaRespuesta? carga = null;
            var fallo = await LlamarApiAsync(async ct => carga = await ClienteCarga.CalcularAsync(tienda.CodigoHanna, ct));
            if (fallo is not null)
            {
                erroresTiendas.Add((tienda, fallo));
            }
            else if (carga is not null)
            {
                filas.AddRange(ExploradorCargas.Construir(tienda, carga, niveles));
            }
        }
        finally
        {
            limite.Release();
            tiendasListas++;
            if (!Liberado)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void Filtrar()
    {
        filtradas = ExploradorCargas.Filtrar(todas, filtro);
        grupos = ExploradorCargas.AgruparPorProducto(filtradas);
    }

    private void CambiarVista(bool porProducto) => vistaPorProducto = porProducto;

    /// <summary>Por defecto solo esta abierto el primer producto (el de mas carga).</summary>
    private bool EstaAbierto(GrupoProducto grupo) =>
        abiertos?.Contains(grupo.Clave) ?? (grupos.Count > 0 && ReferenceEquals(grupo, grupos[0]));

    private void Alternar(GrupoProducto grupo)
    {
        abiertos ??= grupos.Where(EstaAbierto).Select(g => g.Clave).ToHashSet(StringComparer.Ordinal);
        if (!abiertos.Remove(grupo.Clave))
        {
            abiertos.Add(grupo.Clave);
        }
    }

    private void ExpandirTodos() => abiertos = grupos.Select(g => g.Clave).ToHashSet(StringComparer.Ordinal);

    private void ContraerTodos() => abiertos = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Id HTML estable del grupo (para aria-controls).</summary>
    private int IdGrupo(GrupoProducto grupo) => grupos.IndexOf(grupo);

    private void CambiarNivel()
    {
        filtro.Concepto = null;
        ActualizarConceptos();
        Filtrar();
    }

    private void ActualizarConceptos() => conceptos = ExploradorCargas.Conceptos(todas, filtro.Nivel);

    private void LimpiarFiltros()
    {
        filtro.Limpiar();
        Filtrar();
    }

    private static List<CatalogoRespuesta> Distintos(IEnumerable<CatalogoRespuesta> catalogos) =>
        catalogos.DistinctBy(c => c.Id).OrderBy(c => c.Nombre, StringComparer.CurrentCultureIgnoreCase).ToList();
}
