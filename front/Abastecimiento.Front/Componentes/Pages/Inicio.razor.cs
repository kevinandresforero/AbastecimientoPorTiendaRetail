using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Formularios;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Pages;

/// <summary>Inicio: buscador por codigo HANNA, cifras generales (total del consolidado por tienda) y accesos.</summary>
public partial class Inicio
{
    private static readonly IReadOnlyList<SeccionInicio> Secciones =
    [
        new("Tiendas", "Registro de tiendas, mobiliario y espacios totales. Carga de cada tienda.", "tiendas", "tiendas"),
        new("Árbol comercial", "Los 7 niveles con su % de participación y las unidades por espacio.", "arbol", "arbol"),
        new("Explorador de cargas", "Carga de todas las tiendas en una tabla con búsqueda y filtros.", "explorador", "buscar"),
        new("Reportes","Consolidado de espacios y unidades por tienda, marca o regional.", "reportes", "reportes"),
        new("Catálogos", "Marcas, regionales, tipos de tienda y tipos de espacio.", "catalogos", "catalogos")
    ];

    private readonly FormularioBusquedaHanna busqueda = new();
    private GrupoConsolidadoRespuesta? total;
    private bool cargando = true;
    private ExcepcionApi? error;

    [Inject]
    private NavigationManager Navegacion { get; set; } = default!;

    [Inject]
    private IClienteReportes ClienteReportes { get; set; } = default!;

    /// <inheritdoc />
    protected override Task OnInitializedAsync() => CargarCifrasAsync();

    private async Task CargarCifrasAsync()
    {
        cargando = true;
        error = await LlamarApiAsync(async ct =>
            total = (await ClienteReportes.ConsolidadoAsync(AgruparPor.Tienda, null, ct)).Total);
        cargando = false;
    }

    private void Buscar() => Navegacion.NavigateTo($"tiendas/{busqueda.CodigoHanna}");

    private sealed record SeccionInicio(string Titulo, string Descripcion, string Ruta, string Icono);
}
