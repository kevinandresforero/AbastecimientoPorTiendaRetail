using Abastecimiento.Aplicacion.Configuracion;
using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Pruebas.Datos;
using Microsoft.Extensions.Options;

namespace Abastecimiento.Pruebas.Aplicacion;

public class MotorCalculoCargaPruebas
{
    private static MotorCalculoCarga CrearMotor(MidpointRounding modo = MidpointRounding.AwayFromZero) =>
        new(Options.Create(new OpcionesCalculo { ModoRedondeo = modo }));

    /// <summary>Tabla esperada del enunciado (seccion 4.B) para HANNA 5503 con 22 espacios.</summary>
    public static TheoryData<string, string, decimal, int, decimal, int> TablaEsperada5503 => new()
    {
        { "LILI BEAUTY", "LABIALES", 1.2408m, 660, 818.928m, 819 },
        { "LILI BEAUTY", "BRILLO LABIOS", 1.2408m, 540, 670.032m, 670 },
        { "LILI BEAUTY", "PESTAÑINA", 1.67508m, 288, 482.42304m, 482 },
        { "LILI BEAUTY", "DELINEADOR", 1.11672m, 420, 469.0224m, 469 },
        { "LILI BEAUTY", "BASE / POLVOS", 0.9306m, 310, 288.486m, 288 },
        { "KARITE", "BÁLSAMO LABIAL", 0.396m, 600, 237.6m, 238 }
    };

    [Theory]
    [MemberData(nameof(TablaEsperada5503))]
    public void Hanna5503ReproduceCadaLineaDelEnunciado(
        string marcaProducto, string tipoProducto, decimal espacios, int unidades, decimal cargaCalculada, int cargaTotal)
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();

        var resultado = CrearMotor().Calcular(22, nodos, 7);

        var linea = Assert.Single(resultado.Lineas, l =>
            l.Nodo.ConceptoComercial.Nombre == tipoProducto && l.Camino.Any(n => n.ConceptoComercial.Nombre == marcaProducto));
        Assert.Equal(espacios, linea.EspaciosCalculados);
        Assert.Equal(unidades, linea.UnidadesPorEspacio);
        Assert.Equal(cargaCalculada, linea.CargaCalculada);
        Assert.Equal(cargaTotal, linea.CargaTotal);
    }

    [Fact]
    public void Hanna5503CuadraLosTotalesDelEnunciado()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();

        var resultado = CrearMotor().Calcular(22, nodos, 7);

        Assert.Equal(6, resultado.Lineas.Count);
        Assert.Equal(6.6m, resultado.TotalEspaciosAsignados);
        Assert.Equal(2966, resultado.TotalCarga);
        Assert.Empty(resultado.Advertencias);
    }

    [Theory]
    [InlineData(1001, 30, 9, 4046)]
    [InlineData(2045, 20, 6, 2696)]
    public void LasOtrasTiendasCalculanConSusEspacios(int codigoHanna, int espaciosEsperados, int espaciosAsignados, int cargaEsperada)
    {
        var tiendas = new TiendasDePrueba();
        var (nodos, _) = ArbolDePrueba.DesdeSemilla(codigoHanna == 1001 ? 2 : 3);
        var espaciosTotales = tiendas.EspaciosTotales(codigoHanna);

        var resultado = CrearMotor().Calcular(espaciosTotales, nodos, 7);

        Assert.Equal(espaciosEsperados, espaciosTotales);
        Assert.Equal(espaciosAsignados, resultado.TotalEspaciosAsignados);
        Assert.Equal(cargaEsperada, resultado.TotalCarga);
    }

    [Fact]
    public void ModoToEvenSoloCambiaLosEmpates()
    {
        // Arbol de 2 niveles: A = 5 x 0.5 x 1 = 2.5 (empate); B = 5 x 0.46 x 1 = 2.3 (sin empate).
        var nodos = ArbolSimple((0.5m, 1), (0.46m, 1));

        var lejosDeCero = CrearMotor(MidpointRounding.AwayFromZero).Calcular(5, nodos, 2);
        var alPar = CrearMotor(MidpointRounding.ToEven).Calcular(5, nodos, 2);

        Assert.Equal(3, lejosDeCero.Lineas[0].CargaTotal);
        Assert.Equal(2, alPar.Lineas[0].CargaTotal);
        Assert.Equal(lejosDeCero.Lineas[1].CargaTotal, alPar.Lineas[1].CargaTotal);
        Assert.Equal(lejosDeCero.Lineas[0].CargaCalculada, alPar.Lineas[0].CargaCalculada);
    }

    [Fact]
    public void ModoToEvenNoAlteraHanna5503PorqueNoHayEmpates()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();

        Assert.Equal(2966, CrearMotor(MidpointRounding.ToEven).Calcular(22, nodos, 7).TotalCarga);
    }

    [Fact]
    public void RamaIncompletaGeneraAdvertenciaSinLanzarExcepcion()
    {
        var (nodos, niveles) = ArbolDePrueba.DesdeSemilla();
        var nivelCategoria = niveles.Single(n => n.Profundidad == 2);
        nodos.Add(new NodoComercial
        {
            Id = 100,
            NodoPadreId = 1,
            ConceptoComercialId = 100,
            PorcentajeParticipacion = 0.1m,
            ConceptoComercial = new ConceptoComercial { Id = 100, Nombre = "HOGAR", NivelJerarquiaId = nivelCategoria.Id, NivelJerarquia = nivelCategoria }
        });

        var resultado = CrearMotor().Calcular(22, nodos, 7);

        var advertencia = Assert.Single(resultado.Advertencias);
        Assert.Contains("BELLEZA Y BIENESTAR > HOGAR", advertencia);
        Assert.Contains("profundidad 2", advertencia);
        Assert.Contains("(7)", advertencia);
        Assert.Equal(2966, resultado.TotalCarga);
    }

    [Fact]
    public void ArbolConOchoNivelesCalculaSinCambiosDeCodigo()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();
        var nivelVariante = new NivelJerarquia { Id = 8, Nombre = "Variante", Profundidad = 8 };
        var siguienteId = nodos.Max(n => n.Id);

        // Cada tipo de producto pasa a tener un hijo "ESTANDAR" (100%) en el nuevo nivel hoja, con sus mismas unidades.
        foreach (var tipoProducto in nodos.Where(n => n.ConceptoComercial.NivelJerarquia.Profundidad == 7).ToList())
        {
            siguienteId++;
            nodos.Add(new NodoComercial
            {
                Id = siguienteId,
                NodoPadreId = tipoProducto.Id,
                ConceptoComercialId = siguienteId,
                PorcentajeParticipacion = 1m,
                ConceptoComercial = new ConceptoComercial
                {
                    Id = siguienteId,
                    Nombre = "ESTANDAR",
                    NivelJerarquiaId = nivelVariante.Id,
                    NivelJerarquia = nivelVariante,
                    UnidadesPorEspacio = tipoProducto.ConceptoComercial.UnidadesPorEspacio
                }
            });
        }

        var resultado = CrearMotor().Calcular(22, nodos, 8);

        Assert.Equal(6, resultado.Lineas.Count);
        Assert.All(resultado.Lineas, l => Assert.Equal(8, l.Camino.Count));
        Assert.Equal(2966, resultado.TotalCarga);
        Assert.Equal(6.6m, resultado.TotalEspaciosAsignados);
        Assert.Empty(resultado.Advertencias);
    }

    [Fact]
    public void NoHayRedondeoIntermedioEnLosEspacios()
    {
        // Participaciones minimas: 22 x 0.000001 x 0.000001 = 0.000000000022 (no se pierde por redondeo).
        var nodos = ArbolSimple((0.000001m, 660));
        nodos[0].PorcentajeParticipacion = 0.000001m;

        var resultado = CrearMotor().Calcular(22, nodos, 2);

        var linea = Assert.Single(resultado.Lineas);
        Assert.Equal(0.000000000022m, linea.EspaciosCalculados);
        Assert.Equal(0.00000001452m, linea.CargaCalculada);
        Assert.Equal(0, linea.CargaTotal);
    }

    [Fact]
    public void LosEspaciosDeLaSemillaConservanTodosLosDecimales()
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();

        var resultado = CrearMotor().Calcular(22, nodos, 7);

        // 22 x 0.30 x 0.94 x 0.45 x 0.60 = 1.67508 (el enunciado lo presenta como 1.6751).
        var pestanina = resultado.Nodos.Single(n => n.Nodo.Id == 12);
        Assert.Equal(1.67508m, pestanina.EspaciosCalculados);
    }

    [Theory]
    [InlineData(2, "BEAUTY", "6.6")]
    [InlineData(3, "LILI BEAUTY", "6.204")]
    [InlineData(4, "KARITE", "0.396")]
    [InlineData(1, "BELLEZA Y BIENESTAR", "22")]
    public void CategoriaBeautyDe5503ReparteSusEspaciosComoElEnunciado(int nodoId, string nombre, string espacios)
    {
        var (nodos, _) = ArbolDePrueba.DesdeSemilla();

        var resultado = CrearMotor().Calcular(22, nodos, 7);

        var nodo = resultado.Nodos.Single(n => n.Nodo.Id == nodoId);
        Assert.Equal(nombre, nodo.Nodo.ConceptoComercial.Nombre);
        Assert.Equal(decimal.Parse(espacios, System.Globalization.CultureInfo.InvariantCulture), nodo.EspaciosCalculados);
    }

    /// <summary>Raiz (profundidad 1, 100%) con hojas de profundidad 2.</summary>
    private static List<NodoComercial> ArbolSimple(params (decimal Porcentaje, int Unidades)[] hojas)
    {
        var nivelRaiz = new NivelJerarquia { Id = 1, Nombre = "Raiz", Profundidad = 1 };
        var nivelHoja = new NivelJerarquia { Id = 2, Nombre = "Hoja", Profundidad = 2 };
        var nodos = new List<NodoComercial>
        {
            new()
            {
                Id = 1,
                ConceptoComercialId = 1,
                PorcentajeParticipacion = 1m,
                ConceptoComercial = new ConceptoComercial { Id = 1, Nombre = "RAIZ", NivelJerarquiaId = 1, NivelJerarquia = nivelRaiz }
            }
        };

        for (var i = 0; i < hojas.Length; i++)
        {
            var id = i + 2;
            nodos.Add(new NodoComercial
            {
                Id = id,
                NodoPadreId = 1,
                ConceptoComercialId = id,
                PorcentajeParticipacion = hojas[i].Porcentaje,
                ConceptoComercial = new ConceptoComercial
                {
                    Id = id,
                    Nombre = $"HOJA {id}",
                    NivelJerarquiaId = 2,
                    NivelJerarquia = nivelHoja,
                    UnidadesPorEspacio = hojas[i].Unidades
                }
            });
        }

        return nodos;
    }
}
