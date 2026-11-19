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
        hanna, hanna, nombre, new CatalogoRespuesta(hanna, marca), regional, new CatalogoRespuesta(1, "CALLE"), [], 22);

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
    public void Agrupa_por_producto_con_sus_tiendas_y_total()
    {
        var grupos = ExploradorCargas.AgruparPorProducto(Filas());

        Assert.Equal(6, grupos.Count);
        var labiales = Assert.Single(grupos, g => g.TipoProducto == "LABIALES");
        Assert.Equal(2, labiales.Tiendas);
        Assert.Equal(819 + 819, labiales.CargaTotal);
        Assert.Equal("LILI BEAUTY", labiales.MarcaProducto?.Nombre);
        Assert.Equal("LABIOS", labiales.Perfil?.Nombre);
        Assert.Equal(grupos.Select(g => g.CargaTotal).OrderDescending(), grupos.Select(g => g.CargaTotal));
    }

    [Fact]
    public void Agrupa_por_ruta_aunque_cada_tienda_tenga_su_propio_nodo()
    {
        // Arbol por tienda: el mismo producto tiene otro NodoId en la otra tienda; debe quedar en un solo grupo.
        var carga2045 = Datos5503.Carga() with
        {
            Lineas = Datos5503.Carga().Lineas.Select(l => l with { NodoId = l.NodoId + 1000, CargaTotal = l.CargaTotal / 2 }).ToList()
        };
        List<FilaExplorador> filas =
        [
            .. ExploradorCargas.Construir(T5503, Datos5503.Carga(), Datos5503.Niveles),
            .. ExploradorCargas.Construir(T2045, carga2045, Datos5503.Niveles)
        ];

        var grupos = ExploradorCargas.AgruparPorProducto(filas);

        Assert.Equal(6, grupos.Count);
        var labiales = Assert.Single(grupos, g => g.TipoProducto == "LABIALES");
        Assert.Equal([5503, 2045], labiales.Filas.Select(f => f.Tienda.CodigoHanna));
        Assert.Equal(819 + 409, labiales.CargaTotal);
    }

    [Fact]
    public void Productos_con_el_mismo_nombre_en_otra_marca_son_grupos_distintos()
    {
        var grupos = ExploradorCargas.AgruparPorProducto(Filas());

        Assert.Equal(grupos.Count, grupos.Select(g => g.Clave).Distinct().Count());
        Assert.Contains(grupos, g => g.TipoProducto == "BÁLSAMO LABIAL" && g.MarcaProducto?.Nombre == "KARITE");
    }

    [Fact]
    public void Vista_por_producto_muestra_que_va_donde_va_y_cuanto()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IClienteTiendas>(new ListadoFalso([T5503, T2045]));
        Services.AddSingleton<IClienteCarga>(new CargaPorTiendaFalsa());
        Services.AddSingleton<IClienteArbolComercial>(new ArbolFalso());

        var cut = RenderComponent<Explorador>();
        cut.WaitForAssertion(() => Assert.Equal(12, cut.FindAll("table.tabla-explorador tbody tr").Count));

        cut.Find("#vista-producto").Click();

        Assert.Equal(6, cut.FindAll(".grupo-producto").Count);
        Assert.Equal("true", cut.Find("#vista-producto").GetAttribute("aria-pressed"));
        // Solo el primero (mayor carga: LABIALES 819 x 2 = 1.638) viene abierto, con sus 2 tiendas y su total.
        var primero = cut.FindAll(".grupo-producto")[0];
        Assert.Contains("LABIALES", primero.TextContent);
        Assert.Contains("1.638", primero.QuerySelector(".grupo-producto-cifra")!.TextContent);
        Assert.Equal(2, primero.QuerySelectorAll("table.tabla-producto tbody tr").Length);
        Assert.Single(cut.FindAll("table.tabla-producto"));

        cut.FindAll("button").First(b => b.TextContent.Contains("Expandir todo")).Click();
        Assert.Equal(6, cut.FindAll("table.tabla-producto").Count);

        cut.Find("#explorador-regional").Change(Bogota.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        var labiales = cut.FindAll(".grupo-producto").First(g => g.TextContent.Contains("LABIALES"));
        Assert.Contains("819", labiales.QuerySelector(".grupo-producto-cifra")!.TextContent);
        Assert.Contains("1 tienda", labiales.TextContent);

        cut.FindAll("button").First(b => b.TextContent.Contains("Contraer todo")).Click();
        Assert.Empty(cut.FindAll("table.tabla-producto"));
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
