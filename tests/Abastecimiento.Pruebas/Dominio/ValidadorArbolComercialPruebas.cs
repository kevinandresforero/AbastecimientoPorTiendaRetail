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
        nodos.Add(new NodoComercial { Id = 99, NodoPadreId = 10, ConceptoComercial = conceptoMate, PorcentajeParticipacion = 1m });

        var errores = ValidadorArbolComercial.ValidarArbol(nodos, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));

        // Solo quedan como errores las hojas de nivel 7 que aun no se migraron al nivel 8 (comportamiento esperado).
        Assert.Equal(8, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));
        Assert.DoesNotContain(errores, e => e.Contains("MATE") || e.Contains("LABIALES"));
        Assert.All(errores, e => Assert.Contains("no es de nivel hoja", e));
    }
}
