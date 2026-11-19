using Abastecimiento.Aplicacion.Configuracion;
using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Dtos.Reportes;
using Abastecimiento.Aplicacion.Dtos.Tiendas;
using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Aplicacion.Servicios;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Pruebas.Datos;
using Microsoft.Extensions.Options;

namespace Abastecimiento.Pruebas.Aplicacion;

/// <summary>Servicios de aplicacion con repositorios en memoria (sin base de datos).</summary>
public class ServiciosPruebas
{
    private readonly TiendasDePrueba datos = new();
    private readonly ArbolComercialRepositorioFalso arbol = new();
    private readonly UnidadDeTrabajoFalsa unidadDeTrabajo = new();
    private readonly MotorCalculoCarga motor = new(Options.Create(new OpcionesCalculo()));

    private ServicioTiendas CrearServicioTiendas() => new(
        new TiendaRepositorioFalso(datos),
        new TipoEspacioRepositorioFalso(datos.TiposEspacio),
        new CatalogoRepositorioFalso<MarcaTienda>(datos.MarcasTienda),
        new CatalogoRepositorioFalso<Regional>(datos.Regionales),
        new CatalogoRepositorioFalso<TipoTienda>(datos.TiposTienda),
        new CatalogoRepositorioFalso<ResponsableVisual>(datos.ResponsablesVisuales),
        unidadDeTrabajo);

    private ServicioArbolComercial CrearServicioArbol() => new(arbol, unidadDeTrabajo);

    private ServicioReportes CrearServicioReportes() => new(new TiendaRepositorioFalso(datos), arbol, motor);

    private static CrearTiendaSolicitud Solicitud(int codigoHanna, params (int TipoEspacioId, int Cantidad)[] espacios) => new()
    {
        CodigoHanna = codigoHanna,
        Nombre = "  TIENDA NUEVA  ",
        MarcaTiendaId = 2,
        RegionalId = 1,
        TipoTiendaId = 2,
        Espacios = espacios.Select(e => new EspacioSolicitud { TipoEspacioId = e.TipoEspacioId, CantidadEspacios = e.Cantidad }).ToList()
    };

    [Fact]
    public async Task CrearTiendaValidaCalculaEspaciosTotalesYGuardaUnaVez()
    {
        var respuesta = await CrearServicioTiendas().CrearAsync(Solicitud(3000, (1, 5), (4, 3), (7, 10)), CancellationToken.None);

        Assert.Equal(3000, respuesta.CodigoHanna);
        Assert.Equal("TIENDA NUEVA", respuesta.Nombre);
        Assert.Equal(18, respuesta.EspaciosTotales);
        Assert.Equal("LILI PINK", respuesta.MarcaTienda.Nombre);
        Assert.Equal(1, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task CodigoHannaDuplicadoLanzaConflicto()
    {
        await Assert.ThrowsAsync<ConflictoExcepcion>(() =>
            CrearServicioTiendas().CrearAsync(Solicitud(5503, (1, 5)), CancellationToken.None));

        Assert.Equal(0, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task CuatroTiposDeMobiliarioLanzaValidacion()
    {
        var ex = await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicioTiendas().CrearAsync(Solicitud(3001, (1, 1), (2, 1), (3, 1), (4, 1), (7, 4)), CancellationToken.None));

        Assert.Contains(ex.Errores, e => e.Contains("maximo 3"));
        Assert.Equal(0, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task CatalogoOTipoDeEspacioInexistenteLanzaValidacionConTodosLosErrores()
    {
        var solicitud = Solicitud(3002, (1, 5), (99, 2)) with { RegionalId = 50 };

        var ex = await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicioTiendas().CrearAsync(solicitud, CancellationToken.None));

        Assert.Contains(ex.Errores, e => e.Contains("regional 50"));
        Assert.Contains(ex.Errores, e => e.Contains("99"));
    }

    [Fact]
    public async Task ObtenerTiendaInexistenteLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoExcepcion>(() =>
            CrearServicioTiendas().ObtenerPorCodigoHannaAsync(9999, CancellationToken.None));
    }

    [Fact]
    public async Task NodoQueSaltaNivelLanzaValidacion()
    {
        // GIRLS (concepto 5, profundidad 4) directamente bajo BEAUTY (nodo 2, profundidad 2).
        var solicitud = new CrearNodoSolicitud { NodoPadreId = 2, ConceptoComercialId = 5, PorcentajeParticipacion = 0.01m };

        var ex = await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicioArbol().CrearNodoAsync(solicitud, CancellationToken.None));

        Assert.Contains(ex.Errores, e => e.Contains("profundidad 3"));
    }

    [Fact]
    public async Task SumaDeHermanosMayorAUnoLanzaValidacion()
    {
        // Bajo BEAUTY ya suman 0.94 + 0.06 = 1: una nueva marca de producto excede el 100%.
        var concepto = await CrearServicioArbol().CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 3, Nombre = "NUEVA MARCA" }, CancellationToken.None);
        var solicitud = new CrearNodoSolicitud { NodoPadreId = 2, ConceptoComercialId = concepto.Id, PorcentajeParticipacion = 0.1m };

        var ex = await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicioArbol().CrearNodoAsync(solicitud, CancellationToken.None));

        Assert.Contains(ex.Errores, e => e.Contains("supera 1"));
    }

    [Fact]
    public async Task CrearNodoValidoDevuelvePorcentajeAcumuladoYRuta()
    {
        var servicio = CrearServicioArbol();
        var concepto = await servicio.CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 2, Nombre = "HOGAR" }, CancellationToken.None);

        var nodo = await servicio.CrearNodoAsync(
            new CrearNodoSolicitud { NodoPadreId = 1, ConceptoComercialId = concepto.Id, PorcentajeParticipacion = 0.2m },
            CancellationToken.None);

        Assert.Equal("BELLEZA Y BIENESTAR > HOGAR", nodo.Ruta);
        Assert.Equal(0.2m, nodo.PorcentajeAcumulado);
        Assert.False(nodo.EsHoja);
    }

    [Fact]
    public async Task ConceptoHojaSinUnidadesPorEspacioLanzaValidacion()
    {
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicioArbol().CrearConceptoAsync(
                new CrearConceptoSolicitud { NivelJerarquiaId = 7, Nombre = "RUBOR" }, CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarPorcentajeQueExcedeLosHermanosLanzaValidacion()
    {
        // LILI BEAUTY (0.94) a 0.95 con KARITE en 0.06 → 1.01.
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicioArbol().ActualizarPorcentajeAsync(3, new ActualizarPorcentajeSolicitud { PorcentajeParticipacion = 0.95m }, CancellationToken.None));

        Assert.Equal(0, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task ActualizarNodoInexistenteLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoExcepcion>(() =>
            CrearServicioArbol().ActualizarPorcentajeAsync(999, new ActualizarPorcentajeSolicitud { PorcentajeParticipacion = 0.5m }, CancellationToken.None));
    }

    [Fact]
    public async Task CargaDeTiendaInexistenteLanzaNoEncontrado()
    {
        var servicio = new ServicioCarga(new TiendaRepositorioFalso(datos), arbol, motor);

        await Assert.ThrowsAsync<NoEncontradoExcepcion>(() => servicio.CalcularPorCodigoHannaAsync(9999, CancellationToken.None));
    }

    [Fact]
    public async Task CargaDe5503DevuelveElDesgloseDelEnunciado()
    {
        var servicio = new ServicioCarga(new TiendaRepositorioFalso(datos), arbol, motor);

        var carga = await servicio.CalcularPorCodigoHannaAsync(5503, CancellationToken.None);

        Assert.Equal(22, carga.EspaciosTotales);
        Assert.Equal(6.6m, carga.TotalEspaciosAsignados);
        Assert.Equal(2966, carga.TotalCarga);
        Assert.Equal(new[] { 819, 670, 482, 469, 288, 238 }, carga.Lineas.Select(l => l.CargaTotal).ToArray());
        Assert.Equal(7, carga.Lineas[0].Ruta.Count);
    }

    [Fact]
    public async Task ConsolidadoPorRegionalAgrupaBogotaYAntioquia()
    {
        var consolidado = await CrearServicioReportes().ObtenerConsolidadoAsync(AgruparPor.Regional, null, CancellationToken.None);

        var bogota = Assert.Single(consolidado.Grupos, g => g.Nombre == "BOGOTA");
        var antioquia = Assert.Single(consolidado.Grupos, g => g.Nombre == "ANTIOQUIA");
        Assert.Equal((2, 52, 15.6m, 7012), (bogota.CantidadTiendas, bogota.EspaciosTotales!.Value, bogota.EspaciosAsignados, bogota.CargaTotal));
        Assert.Equal((1, 20, 6m, 2696), (antioquia.CantidadTiendas, antioquia.EspaciosTotales!.Value, antioquia.EspaciosAsignados, antioquia.CargaTotal));
        Assert.Equal(9708, consolidado.Total.CargaTotal);
        Assert.Equal(72, consolidado.Total.EspaciosTotales);
    }

    [Fact]
    public async Task ConsolidadoPorNivel3AgrupaPorMarcaDeProducto()
    {
        var consolidado = await CrearServicioReportes().ObtenerConsolidadoAsync(AgruparPor.Nivel, 3, CancellationToken.None);

        Assert.Equal("Marca de Producto", consolidado.Nivel!.Nombre);
        var liliBeauty = Assert.Single(consolidado.Grupos, g => g.Nombre == "LILI BEAUTY");
        var karite = Assert.Single(consolidado.Grupos, g => g.Nombre == "KARITE");
        Assert.Equal(8930, liliBeauty.CargaTotal);
        Assert.Equal(778, karite.CargaTotal);
        Assert.Null(liliBeauty.EspaciosTotales);
        Assert.Equal(21.6m, consolidado.Total.EspaciosAsignados);
        Assert.Equal(9708, consolidado.Total.CargaTotal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(99)]
    public async Task ConsolidadoPorNivelSinNivelValidoLanzaValidacion(int? nivelId)
    {
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            CrearServicioReportes().ObtenerConsolidadoAsync(AgruparPor.Nivel, nivelId, CancellationToken.None));
    }

    [Fact]
    public async Task ListarTiendasSinFiltroOrdenaPorCodigoHanna()
    {
        var tiendas = await CrearServicioTiendas().ListarAsync(null, null, CancellationToken.None);

        Assert.Equal(new[] { 1001, 2045, 5503 }, tiendas.Select(t => t.CodigoHanna));
        Assert.Equal(new[] { 30, 20, 22 }, tiendas.Select(t => t.EspaciosTotales));
    }

    [Theory]
    [InlineData(null, 1, new[] { 1001, 5503 })]
    [InlineData(3, null, new[] { 2045 })]
    [InlineData(2, 2, new int[0])]
    public async Task ListarTiendasAplicaFiltros(int? marcaTiendaId, int? regionalId, int[] esperadas)
    {
        var tiendas = await CrearServicioTiendas().ListarAsync(marcaTiendaId, regionalId, CancellationToken.None);

        Assert.Equal(esperadas, tiendas.Select(t => t.CodigoHanna));
    }

    [Fact]
    public async Task ConsolidadoPorTiendaOrdenaPorCodigoHanna()
    {
        var consolidado = await CrearServicioReportes().ObtenerConsolidadoAsync(AgruparPor.Tienda, null, CancellationToken.None);

        Assert.Equal(new[] { "1001", "2045", "5503" }, consolidado.Grupos.Select(g => g.Clave));
        Assert.Equal(new[] { 4046, 2696, 2966 }, consolidado.Grupos.Select(g => g.CargaTotal));
        Assert.All(consolidado.Grupos, g => Assert.Equal(1, g.CantidadTiendas));
        Assert.Equal(3, consolidado.Total.CantidadTiendas);
    }

    [Fact]
    public async Task ConsolidadoPorMarcaDeTiendaAgrupaPorMarca()
    {
        var consolidado = await CrearServicioReportes().ObtenerConsolidadoAsync(AgruparPor.MarcaTienda, null, CancellationToken.None);

        Assert.Equal(
            new[] { ("LILI PINK", 4046), ("YAHAD", 2966), ("YOI", 2696) },
            consolidado.Grupos.Select(g => (g.Nombre, g.CargaTotal)));
        Assert.Equal(72, consolidado.Total.EspaciosTotales);
    }
}
