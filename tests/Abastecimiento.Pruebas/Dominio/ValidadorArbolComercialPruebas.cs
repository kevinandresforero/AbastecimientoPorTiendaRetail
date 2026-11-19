using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Reglas;
using Abastecimiento.Pruebas.Datos;

namespace Abastecimiento.Pruebas.Dominio;

public class ValidadorArbolComercialPruebas
{
    [Fact]
    public void LaSemillaEsUnArbolValido()
    {
        var (nodos, niveles) = ArbolDePrueba.DesdeSemilla();

        var errores = ValidadorArbolComercial.ValidarArbol(nodos, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));

        Assert.Empty(errores);
    }

    [Fact]
    public void LaProfundidadHojaSeDerivaDelNivelMasProfundo()
    {
        var (_, niveles) = ArbolDePrueba.DesdeSemilla();

        Assert.Equal(7, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));
    }

    [Fact]
    public void RechazaNodoQueSaltaNiveles()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var beauty = nodos.Single(n => n.Id == 2);                 // profundidad 2
        var conceptoGirls = nodos.Single(n => n.Id == 5).ConceptoComercial; // profundidad 4
        var nuevo = new NodoComercial { NodoPadreId = 2, ConceptoComercial = conceptoGirls, PorcentajeParticipacion = 0.01m };

        var ex = Assert.Throws<ValidacionDominioExcepcion>(() =>
            ValidadorArbolComercial.ValidarNodo(nuevo, beauty, nodos.Where(n => n.NodoPadreId == 2)));

        Assert.Contains(ex.Errores, e => e.Contains("profundidad 3"));
    }

    [Fact]
    public void RechazaRaizQueNoEsDeProfundidadUno()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var conceptoBeauty = nodos.Single(n => n.Id == 2).ConceptoComercial;
        var nuevo = new NodoComercial { NodoPadreId = null, ConceptoComercial = conceptoBeauty, PorcentajeParticipacion = 1m };

        Assert.Throws<ValidacionDominioExcepcion>(() => ValidadorArbolComercial.ValidarNodo(nuevo, null, []));
    }

    [Fact]
    public void RechazaHermanosQueSumanMasDeUno()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var labios = nodos.Single(n => n.Id == 7);                 // hijos: LABIALES 0.50 + BRILLO 0.50
        var conceptoDelineador = nodos.Single(n => n.Id == 13).ConceptoComercial;
        var nuevo = new NodoComercial { NodoPadreId = 7, ConceptoComercial = conceptoDelineador, PorcentajeParticipacion = 0.10m };

        var ex = Assert.Throws<ValidacionDominioExcepcion>(() =>
            ValidadorArbolComercial.ValidarNodo(nuevo, labios, nodos.Where(n => n.NodoPadreId == 7)));

        Assert.Contains(ex.Errores, e => e.Contains("supera 1"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(1.000001)]
    public void RechazaPorcentajeFueraDeRango(decimal porcentaje)
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var rostro = nodos.Single(n => n.Id == 9);
        var nuevo = new NodoComercial { NodoPadreId = 9, ConceptoComercial = nodos.Single(n => n.Id == 10).ConceptoComercial, PorcentajeParticipacion = porcentaje };

        Assert.Throws<ValidacionDominioExcepcion>(() => ValidadorArbolComercial.ValidarNodo(nuevo, rostro, []));
    }

    [Fact]
    public void RechazaConceptoHojaSinUnidadesPorEspacio()
    {
        var (_, niveles) = ArbolDePrueba.DesdeSemilla();
        var tipoProducto = niveles.Single(n => n.Profundidad == 7);
        var concepto = new ConceptoComercial { Nombre = "SOMBRAS", NivelJerarquia = tipoProducto, UnidadesPorEspacio = null };

        Assert.Throws<ValidacionDominioExcepcion>(() => ValidadorArbolComercial.ValidarConcepto(concepto, tipoProducto, 7));
    }

    [Fact]
    public void RechazaConceptoNoHojaConUnidadesPorEspacio()
    {
        var (_, niveles) = ArbolDePrueba.DesdeSemilla();
        var perfil = niveles.Single(n => n.Profundidad == 6);
        var concepto = new ConceptoComercial { Nombre = "CEJAS", NivelJerarquia = perfil, UnidadesPorEspacio = 100 };

        Assert.Throws<ValidacionDominioExcepcion>(() => ValidadorArbolComercial.ValidarConcepto(concepto, perfil, 7));
    }

    [Fact]
    public void UnNivelOchoNoRequiereCambiosDeEsquemaNiDeCodigo()
    {
        // Escalabilidad: agregar un nivel 8 al final mueve el nivel hoja y el validador se adapta solo.
        var (nodos, niveles) = ArbolDePrueba.DesdeSemilla();
        var variante = new NivelJerarquia { Id = 8, Nombre = "Variante", Profundidad = 8 };
        niveles.Add(variante);

        var labiales = nodos.Single(n => n.Id == 10);
        labiales.ConceptoComercial.UnidadesPorEspacio = null; // deja de ser hoja
        var conceptoMate = new ConceptoComercial { Id = 99, Nombre = "MATE", NivelJerarquia = variante, UnidadesPorEspacio = 660 };
        nodos.Add(new NodoComercial { Id = 99, TiendaId = 1, NodoPadreId = 10, ConceptoComercial = conceptoMate, PorcentajeParticipacion = 1m });

        var errores = ValidadorArbolComercial.ValidarArbol(nodos, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));

        // Solo quedan como errores las hojas de nivel 7 que aun no se migraron al nivel 8 (comportamiento esperado).
        Assert.Equal(8, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));
        Assert.DoesNotContain(errores, e => e.Contains("MATE") || e.Contains("LABIALES"));
        Assert.All(errores, e => Assert.Contains("no es de nivel hoja", e));
    }

    // ---- Arbol comercial por tienda (regla 5 y hermanos por tienda) ----

    [Fact]
    public void LosArbolesDeLasTresTiendasSonValidos()
    {
        var (nodos, niveles) = ArbolDePrueba.DesdeSemillaTodasLasTiendas();

        var errores = ValidadorArbolComercial.ValidarArbol(nodos, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));

        // 3 raices al 100% cada una: no suman 3 porque los hermanos se agrupan por tienda.
        Assert.Equal(54, nodos.Count);
        Assert.Empty(errores);
    }

    [Fact]
    public void RechazaNodoCuyoPadreEsDeOtraTienda()
    {
        var (nodosTienda1, _) = ArbolDePrueba.DesdeSemilla(1);
        var (nodosTienda2, _) = ArbolDePrueba.DesdeSemilla(2);
        var beautyTienda1 = nodosTienda1.Single(n => n.Id == 2);
        var conceptoLiliBeauty = nodosTienda1.Single(n => n.Id == 3).ConceptoComercial;
        var nuevo = new NodoComercial
        {
            TiendaId = 2, NodoPadreId = beautyTienda1.Id, ConceptoComercial = conceptoLiliBeauty, PorcentajeParticipacion = 0.01m
        };

        var ex = Assert.Throws<ValidacionDominioExcepcion>(() =>
            ValidadorArbolComercial.ValidarNodo(nuevo, beautyTienda1, nodosTienda2.Where(n => n.NodoPadreId == beautyTienda1.Id)));

        Assert.Contains(ex.Errores, e => e.Contains("tienda 2") && e.Contains("tienda 1"));
    }

    [Fact]
    public void ValidarArbolDetectaHijoColgadoDeOtraTienda()
    {
        var (nodos, niveles) = ArbolDePrueba.DesdeSemillaTodasLasTiendas();
        nodos.Single(n => n.Id == 2003).NodoPadreId = 2; // LILI BEAUTY de 1001 colgado de BEAUTY de 5503

        var errores = ValidadorArbolComercial.ValidarArbol(nodos, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));

        Assert.Contains(errores, e => e.StartsWith("Nodo 2003:") && e.Contains("deben ser la misma"));
    }

    [Fact]
    public void LasUnidadesDeNegocioDeUnaTiendaNoSuperanEl100()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla(1);
        var raiz = nodos.Single(n => n.NodoPadreId is null);
        raiz.PorcentajeParticipacion = 0.30m;
        var otraUnidad = new NodoComercial { TiendaId = 1, ConceptoComercial = raiz.ConceptoComercial, PorcentajeParticipacion = 0.80m };

        var ex = Assert.Throws<ValidacionDominioExcepcion>(() =>
            ValidadorArbolComercial.ValidarNodo(otraUnidad, null, [raiz]));

        Assert.Contains(ex.Errores, e => e.Contains("supera 1"));
        ValidadorArbolComercial.ValidarNodo(
            new NodoComercial { TiendaId = 1, ConceptoComercial = raiz.ConceptoComercial, PorcentajeParticipacion = 0.70m }, null, [raiz]);
    }

    [Fact]
    public void ValidarArbolSumaUnidadesDeNegocioPorTienda()
    {
        var (nodos, niveles) = ArbolDePrueba.DesdeSemillaTodasLasTiendas();
        var raiz2 = nodos.Single(n => n.Id == 2001);
        nodos.Add(new NodoComercial { Id = 9999, TiendaId = 2, ConceptoComercial = raiz2.ConceptoComercial, PorcentajeParticipacion = 0.50m });

        var errores = ValidadorArbolComercial.ValidarArbol(nodos, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));

        Assert.Contains(errores, e => e.StartsWith("Tienda 2: las unidades de negocio suman") && e.EndsWith("(> 1)."));
        Assert.DoesNotContain(errores, e => e.StartsWith("Tienda 1") || e.StartsWith("Tienda 3"));
    }
}
