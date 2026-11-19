using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Pruebas.Datos;

namespace Abastecimiento.Pruebas.Aplicacion;

/// <summary>Lecturas y escrituras del arbol comercial (R2) con los arboles de la semilla en memoria.</summary>
public class ServicioArbolComercialPruebas
{
    private readonly TiendasDePrueba datos = new();
    private readonly UnidadDeTrabajoFalsa unidadDeTrabajo = new();
    private readonly ArbolComercialRepositorioFalso arbol = new();
    private readonly ServicioArbolComercial servicio;

    public ServicioArbolComercialPruebas()
    {
        servicio = new ServicioArbolComercial(arbol, new TiendaRepositorioFalso(datos), unidadDeTrabajo);
    }

    [Fact]
    public async Task NivelesOrdenadosYSoloElUltimoEsHoja()
    {
        var niveles = await servicio.ObtenerNivelesAsync(CancellationToken.None);

        Assert.Equal(Enumerable.Range(1, 7), niveles.Select(n => n.Profundidad));
        var hoja = Assert.Single(niveles, n => n.EsHoja);
        Assert.Equal("Tipo de Producto", hoja.Nombre);
    }

    [Fact]
    public async Task ArbolEnPreordenConRutaYPorcentajeAcumulado()
    {
        var arbolTienda = await servicio.ObtenerArbolAsync(5503, CancellationToken.None);

        Assert.Equal(18, arbolTienda.Count);
        Assert.Equal(new[] { 1, 2, 3, 5 }, arbolTienda.Take(4).Select(n => n.NodoId));
        var beauty = arbolTienda.Single(n => n.NodoId == 2);
        Assert.Equal(0.3m, beauty.PorcentajeAcumulado);
        Assert.Equal("BELLEZA Y BIENESTAR > BEAUTY", beauty.Ruta);
        var labiales = arbolTienda.Single(n => n.NodoId == 10);
        Assert.Equal(0.0564m, labiales.PorcentajeAcumulado);
        Assert.True(labiales.EsHoja);
        Assert.Equal(660, labiales.UnidadesPorEspacio);
        Assert.Equal(6, arbolTienda.Count(n => n.EsHoja));
    }

    [Fact]
    public async Task CadaTiendaTieneSuPropioArbolConLosMismosPorcentajes()
    {
        var tienda5503 = await servicio.ObtenerArbolAsync(5503, CancellationToken.None);
        var tienda1001 = await servicio.ObtenerArbolAsync(1001, CancellationToken.None);

        Assert.Equal(18, tienda5503.Count);
        Assert.Equal(18, tienda1001.Count);
        Assert.Empty(tienda5503.Select(n => n.NodoId).Intersect(tienda1001.Select(n => n.NodoId)));
        Assert.Equal(
            tienda5503.Select(n => (n.Ruta, n.PorcentajeParticipacion)),
            tienda1001.Select(n => (n.Ruta, n.PorcentajeParticipacion)));
        Assert.Equal(2001, tienda1001[0].NodoId);
    }

    [Fact]
    public async Task TiendaSinArbolDevuelveListaVacia()
    {
        var tienda = AgregarTienda(7000);

        var arbolTienda = await servicio.ObtenerArbolAsync(tienda.CodigoHanna, CancellationToken.None);

        Assert.Empty(arbolTienda);
    }

    [Fact]
    public async Task TiendaInexistenteLanzaNoEncontrado()
    {
        await Assert.ThrowsAsync<NoEncontradoExcepcion>(() =>
            servicio.ObtenerArbolAsync(9999, CancellationToken.None));
    }

    [Fact]
    public async Task ListarConceptosFiltraPorNivel()
    {
        var marcas = await servicio.ListarConceptosAsync(3, CancellationToken.None);
        var todos = await servicio.ListarConceptosAsync(null, CancellationToken.None);

        Assert.Equal(new[] { "KARITE", "LILI BEAUTY" }, marcas.Select(c => c.Nombre).Order().ToArray());
        Assert.All(marcas, c => Assert.Equal("Marca de Producto", c.Nivel.Nombre));
        Assert.Equal(15, todos.Count);
    }

    [Fact]
    public async Task CrearConceptoHojaConUnidades()
    {
        var concepto = await servicio.CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 7, Nombre = " RUBOR ", UnidadesPorEspacio = 400 }, CancellationToken.None);

        Assert.Equal("RUBOR", concepto.Nombre);
        Assert.True(concepto.Nivel.EsHoja);
        Assert.Equal(400, concepto.UnidadesPorEspacio);
        Assert.Equal(1, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task ConceptoNoHojaConUnidadesLanzaValidacion()
    {
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() => servicio.CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 3, Nombre = "NUEVA MARCA", UnidadesPorEspacio = 10 }, CancellationToken.None));
    }

    [Fact]
    public async Task ConceptoEnNivelInexistenteLanzaValidacion()
    {
        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() => servicio.CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 99, Nombre = "X" }, CancellationToken.None));
    }

    [Fact]
    public async Task ConceptoDuplicadoEnElNivelLanzaConflicto()
    {
        await Assert.ThrowsAsync<ConflictoExcepcion>(() => servicio.CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 6, Nombre = "LABIOS" }, CancellationToken.None));
    }

    [Fact]
    public async Task ActualizarPorcentajeValidoRecalculaElAcumulado()
    {
        // KARITE (nodo 4) de 6% a 5%: hermanos 0.94 + 0.05 <= 1.
        var nodo = await servicio.ActualizarPorcentajeAsync(
            4, new ActualizarPorcentajeSolicitud { PorcentajeParticipacion = 0.05m }, CancellationToken.None);

        Assert.Equal(0.05m, nodo.PorcentajeParticipacion);
        Assert.Equal(0.015m, nodo.PorcentajeAcumulado);
        Assert.Equal(1, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task CrearNodoEnUnaTiendaNoModificaElArbolDeOtra()
    {
        var concepto = await servicio.CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 2, Nombre = "HOGAR" }, CancellationToken.None);

        var nodo = await servicio.CrearNodoAsync(
            1001,
            new CrearNodoSolicitud { NodoPadreId = 2001, ConceptoComercialId = concepto.Id, PorcentajeParticipacion = 0.2m },
            CancellationToken.None);

        Assert.Equal("BELLEZA Y BIENESTAR > HOGAR", nodo.Ruta);
        Assert.Equal(19, (await servicio.ObtenerArbolAsync(1001, CancellationToken.None)).Count);
        Assert.Equal(18, (await servicio.ObtenerArbolAsync(5503, CancellationToken.None)).Count);
    }

    [Fact]
    public async Task NodoConPadreDeOtraTiendaLanzaValidacion()
    {
        var concepto = await servicio.CrearConceptoAsync(
            new CrearConceptoSolicitud { NivelJerarquiaId = 2, Nombre = "HOGAR" }, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() => servicio.CrearNodoAsync(
            1001,
            new CrearNodoSolicitud { NodoPadreId = 1, ConceptoComercialId = concepto.Id, PorcentajeParticipacion = 0.2m },
            CancellationToken.None));

        Assert.Contains(ex.Errores, e => e.Contains("tienda"));
    }

    [Fact]
    public async Task CopiarArbolCreaNodosNuevosEIndependientes()
    {
        var destino = AgregarTienda(7000);

        var copia = await servicio.CopiarArbolAsync(destino.CodigoHanna, 5503, CancellationToken.None);

        Assert.Equal((5503, 7000, 18), (copia.CodigoHannaOrigen, copia.CodigoHannaDestino, copia.NodosCopiados));
        var arbolDestino = await servicio.ObtenerArbolAsync(destino.CodigoHanna, CancellationToken.None);
        var arbolOrigen = await servicio.ObtenerArbolAsync(5503, CancellationToken.None);
        Assert.Equal(18, arbolDestino.Count);
        Assert.Empty(arbolDestino.Select(n => n.NodoId).Intersect(arbolOrigen.Select(n => n.NodoId)));
        var nodosDestino = await arbol.ObtenerArbolPorTiendaAsync(destino.Id, CancellationToken.None);
        Assert.Equal(18, nodosDestino.Count);
        Assert.All(nodosDestino, n => Assert.Equal(destino.Id, n.TiendaId));
        Assert.Equal(
            arbolOrigen.Select(n => (n.Ruta, n.PorcentajeParticipacion, n.PorcentajeAcumulado)),
            arbolDestino.Select(n => (n.Ruta, n.PorcentajeParticipacion, n.PorcentajeAcumulado)));
        Assert.Equal(1, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task CopiarArbolADestinoConNodosLanzaConflicto()
    {
        await Assert.ThrowsAsync<ConflictoExcepcion>(() =>
            servicio.CopiarArbolAsync(1001, 5503, CancellationToken.None));

        Assert.Equal(0, unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task CopiarArbolDeOrigenVacioLanzaValidacion()
    {
        var origen = AgregarTienda(7000);
        var destino = AgregarTienda(7001);

        await Assert.ThrowsAsync<ValidacionDominioExcepcion>(() =>
            servicio.CopiarArbolAsync(destino.CodigoHanna, origen.CodigoHanna, CancellationToken.None));
    }

    [Fact]
    public async Task CopiarArbolDeTiendaInexistenteLanzaNoEncontrado()
    {
        var destino = AgregarTienda(7000);

        await Assert.ThrowsAsync<NoEncontradoExcepcion>(() =>
            servicio.CopiarArbolAsync(destino.CodigoHanna, 9999, CancellationToken.None));
    }

    private Tienda AgregarTienda(int codigoHanna)
    {
        var tienda = new Tienda
        {
            Id = datos.Tiendas.Max(t => t.Id) + 1,
            CodigoHanna = codigoHanna,
            Nombre = $"TIENDA {codigoHanna}",
            MarcaTiendaId = 1,
            RegionalId = 1,
            TipoTiendaId = 1
        };
        datos.Enlazar(tienda);
        datos.Tiendas.Add(tienda);
        return tienda;
    }
}
