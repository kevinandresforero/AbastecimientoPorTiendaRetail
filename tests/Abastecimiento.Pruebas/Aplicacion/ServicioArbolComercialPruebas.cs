using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Servicios;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Pruebas.Datos;

namespace Abastecimiento.Pruebas.Aplicacion;

/// <summary>Lecturas y escrituras del arbol comercial (R2) con el arbol de la semilla en memoria.</summary>
public class ServicioArbolComercialPruebas
{
    private readonly UnidadDeTrabajoFalsa unidadDeTrabajo = new();
    private readonly ServicioArbolComercial servicio;

    public ServicioArbolComercialPruebas()
    {
        servicio = new ServicioArbolComercial(new ArbolComercialRepositorioFalso(), unidadDeTrabajo);
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
        var arbol = await servicio.ObtenerArbolAsync(CancellationToken.None);

        Assert.Equal(18, arbol.Count);
        Assert.Equal(new[] { 1, 2, 3, 5 }, arbol.Take(4).Select(n => n.NodoId));
        var beauty = arbol.Single(n => n.NodoId == 2);
        Assert.Equal(0.3m, beauty.PorcentajeAcumulado);
        Assert.Equal("BELLEZA Y BIENESTAR > BEAUTY", beauty.Ruta);
        var labiales = arbol.Single(n => n.NodoId == 10);
        Assert.Equal(0.0564m, labiales.PorcentajeAcumulado);
        Assert.True(labiales.EsHoja);
        Assert.Equal(660, labiales.UnidadesPorEspacio);
        Assert.Equal(6, arbol.Count(n => n.EsHoja));
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
}
