using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Componentes.Pages;
using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Pruebas.Apoyo;
using Abastecimiento.Front.Utilidades;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Front.Pruebas;

public class ExploradorPruebas : TestContext
{
    private static readonly CatalogoRespuesta Bogota = new(1, "BOGOTA");
    private static readonly CatalogoRespuesta Antioquia = new(2, "ANTIOQUIA");

    private static TiendaRespuesta Tienda(int hanna, string nombre, CatalogoRespuesta regional, string marca) => new(
        hanna, hanna, nombre, new CatalogoRespuesta(hanna, marca), regional, new CatalogoRespuesta(1, "CALLE"), null, [], 22);

    private static readonly TiendaRespuesta T5503 = Tienda(5503, "LILI EN YOI YAHAD TOBERIN", Bogota, "YAHAD");
    private static readonly TiendaRespuesta T2045 = Tienda(2045, "YOI VIVA ENVIGADO", Antioquia, "YOI");

    private static List<FilaExplorador> Filas() =>
    [
        .. ExploradorCargas.Construir(T5503, Datos5503.Carga(), Datos5503.Niveles),
        .. ExploradorCargas.Construir(T2045, Datos5503.Carga(), Datos5503.Niveles)
    ];

    [Fact]
    public void Construye_una_fila_por_linea_y_tienda_con_marca_perfil_y_hoja()
    {
        var filas = Filas();

        Assert.Equal(12, filas.Count);
        var primera = filas[0];
        Assert.Equal("LILI BEAUTY", primera.MarcaProducto?.Nombre);
        Assert.Equal("LABIOS", primera.Perfil?.Nombre);
        Assert.Equal("LABIALES", primera.Hoja?.Nombre);
    }

    [Fact]
    public void Filtra_los_productos_de_una_regional_en_una_hoja()
    {
        var filtro = new FiltroCargas { RegionalId = Bogota.Id, Nivel = "Tipo de Producto", Concepto = "LABIALES" };

        var fila = Assert.Single(ExploradorCargas.Filtrar(Filas(), filtro));

        Assert.Equal(5503, fila.Tienda.CodigoHanna);
        Assert.Equal(819, fila.Linea.CargaTotal);
    }

    [Fact]
    public void Filtra_por_un_concepto_de_nivel_intermedio()
    {
        var filtro = new FiltroCargas { Nivel = "Marca de Producto", Concepto = "KARITE" };

        var filas = ExploradorCargas.Filtrar(Filas(), filtro);

        Assert.Equal(2, filas.Count);
        Assert.All(filas, f => Assert.Equal("BÁLSAMO LABIAL", f.Linea.TipoProducto));
    }

    [Fact]
    public void Busqueda_por_palabras_sin_distinguir_tildes_ni_mayusculas()
    {
        var filas = ExploradorCargas.Filtrar(Filas(), new FiltroCargas { Texto = "pestanina  bogota" });

        var fila = Assert.Single(filas);
        Assert.Equal("PESTAÑINA", fila.Linea.TipoProducto);
        Assert.Equal(5503, fila.Tienda.CodigoHanna);
        Assert.Equal(2, ExploradorCargas.Filtrar(Filas(), new FiltroCargas { Texto = "balsamo" }).Count);
        Assert.Equal(6, ExploradorCargas.Filtrar(Filas(), new FiltroCargas { Texto = "2045" }).Count);
    }

    [Fact]
    public void Conceptos_del_nivel_son_distintos_y_ordenados()
    {
        var conceptos = ExploradorCargas.Conceptos(Filas(), "Perfil");

        Assert.Equal(["LABIOS", "OJOS", "ROSTRO"], conceptos);
        Assert.Empty(ExploradorCargas.Conceptos(Filas(), null));
    }

    [Fact]
    public void Limpiar_quita_los_filtros_y_conserva_el_nivel()
    {
        var filtro = new FiltroCargas { Texto = "x", RegionalId = 1, Nivel = "Perfil", Concepto = "OJOS" };

        filtro.Limpiar();

        Assert.False(filtro.TieneFiltros);
        Assert.Equal("Perfil", filtro.Nivel);
        Assert.Equal(12, ExploradorCargas.Filtrar(Filas(), filtro).Count);
    }

    [Fact]
    public void Filtro_de_nodos_por_nivel_hoja_y_busqueda()
    {
        NodoArbolRespuesta Nodo(int id, string nombre, string nivel, bool hoja, string ruta) =>
            new(id, null, id, nombre, nivel, 1, 1m, 1m, ruta, null, hoja);
        List<NodoArbolRespuesta> nodos =
        [
            Nodo(7, "LABIOS", "Perfil", false, "BEAUTY > LILI BEAUTY > LABIOS"),
            Nodo(10, "LABIALES", "Tipo de Producto", true, "BEAUTY > LILI BEAUTY > LABIOS > LABIALES"),
            Nodo(12, "PESTAÑINA", "Tipo de Producto", true, "BEAUTY > LILI BEAUTY > OJOS > PESTAÑINA")
        ];

        Assert.Equal([7], new FiltroNodos { Nivel = "Perfil" }.Aplicar(nodos).Select(n => n.NodoId));
        Assert.Equal([10, 12], new FiltroNodos { SoloHojas = true }.Aplicar(nodos).Select(n => n.NodoId));
        Assert.Equal([7, 10], new FiltroNodos { Texto = "labios" }.Aplicar(nodos).Select(n => n.NodoId));
        Assert.Equal([12], new FiltroNodos { Texto = "pestanina" }.Aplicar(nodos).Select(n => n.NodoId));
    }

    [Fact]
    public void Pagina_carga_todas_las_tiendas_y_filtra_en_vivo()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var carga = new CargaPorTiendaFalsa();
        Services.AddSingleton<IClienteTiendas>(new ListadoFalso([T5503, T2045]));
        Services.AddSingleton<IClienteCarga>(carga);
        Services.AddSingleton<IClienteArbolComercial>(new ArbolFalso());

        var cut = RenderComponent<Explorador>();
        cut.WaitForAssertion(() => Assert.Equal(12, cut.FindAll("table.tabla-explorador tbody tr").Count));
        Assert.Equal([2045, 5503], carga.Consultadas.Order());

        cut.Find("#explorador-regional").Change(Bogota.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal(6, cut.FindAll("table.tabla-explorador tbody tr").Count);

        cut.Find("#explorador-concepto").Change("LABIALES");
        Assert.Single(cut.FindAll("table.tabla-explorador tbody tr"));
        Assert.Contains("819", cut.Find(".tarjeta-cifra.destacada").TextContent);

        cut.Find("#explorador-busqueda").Input("no existe");
        Assert.Contains("Ninguna fila cumple los filtros", cut.Markup);
    }

    [Fact]
    public void Pagina_avisa_las_tiendas_que_fallan_y_muestra_las_demas()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IClienteTiendas>(new ListadoFalso([T5503, T2045]));
        Services.AddSingleton<IClienteCarga>(new CargaPorTiendaFalsa { Falla = 2045 });
        Services.AddSingleton<IClienteArbolComercial>(new ArbolFalso());

        var cut = RenderComponent<Explorador>();

        cut.WaitForAssertion(() => Assert.Equal(6, cut.FindAll("table.tabla-explorador tbody tr").Count));
        Assert.Contains("no se pudo calcular la carga de 1 tienda", cut.Markup);
        Assert.Contains("HANNA 2045", cut.Markup);
    }

    private sealed class ListadoFalso(IReadOnlyList<TiendaRespuesta> tiendas) : IClienteTiendas
    {
        public Task<IReadOnlyList<TiendaRespuesta>> ListarAsync(int? marcaTiendaId, int? regionalId, CancellationToken ct) =>
            Task.FromResult(tiendas);

        public Task<TiendaRespuesta> ObtenerAsync(int codigoHanna, CancellationToken ct) =>
            Task.FromResult(tiendas.First(t => t.CodigoHanna == codigoHanna));

        public Task<TiendaRespuesta> CrearAsync(CrearTiendaSolicitud solicitud, CancellationToken ct) =>
            throw new NotSupportedException();
    }

    private sealed class CargaPorTiendaFalsa : IClienteCarga
    {
        public List<int> Consultadas { get; } = [];

        public int? Falla { get; init; }

        public Task<CargaTiendaRespuesta> CalcularAsync(int codigoHanna, CancellationToken ct)
        {
            Consultadas.Add(codigoHanna);
            return codigoHanna == Falla
                ? Task.FromException<CargaTiendaRespuesta>(new ExcepcionApi(0, ExcepcionApi.MensajeSinConexion))
                : Task.FromResult(Datos5503.Carga());
        }
    }
}
