using System.ComponentModel.DataAnnotations;
using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Componentes.Arbol;
using Abastecimiento.Front.Formularios;
using Abastecimiento.Front.Pruebas.Apoyo;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Front.Pruebas;

public class FormularioConceptoPruebas : TestContext
{
    private readonly ArbolFalso arbol = new();

    public FormularioConceptoPruebas() => Services.AddSingleton<IClienteArbolComercial>(arbol);

    private static List<ValidationResult> Validar(object modelo)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), resultados, validateAllProperties: true);
        return resultados;
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    public void Nivel_hoja_sin_unidades_validas_es_invalido(int? unidades)
    {
        var formulario = new FormularioConcepto
        {
            Niveles = Datos5503.Niveles, NivelJerarquiaId = 7, Nombre = "LABIALES", UnidadesPorEspacio = unidades
        };

        var error = Assert.Single(Validar(formulario));
        Assert.Contains(nameof(FormularioConcepto.UnidadesPorEspacio), error.MemberNames);
    }

    [Fact]
    public void Nivel_hoja_con_unidades_es_valido_y_nivel_no_hoja_no_las_exige()
    {
        Assert.Empty(Validar(new FormularioConcepto
        {
            Niveles = Datos5503.Niveles, NivelJerarquiaId = 7, Nombre = "LABIALES", UnidadesPorEspacio = 660
        }));
        Assert.Empty(Validar(new FormularioConcepto { Niveles = Datos5503.Niveles, NivelJerarquiaId = 6, Nombre = "LABIOS" }));
    }

    [Fact]
    public void Nivel_no_hoja_oculta_el_campo_y_envia_null()
    {
        var cut = RenderComponent<DialogoNuevoConcepto>(p => p.Add(c => c.Niveles, Datos5503.Niveles));

        cut.Find("#concepto-nivel").Change("7");
        cut.Find("#concepto-unidades").Change("600");
        cut.Find("#concepto-nivel").Change("6");
        Assert.Empty(cut.FindAll("#concepto-unidades"));
        cut.Find("#concepto-nombre").Change("CABELLO");
        cut.Find("form").Submit();

        var enviada = Assert.Single(arbol.ConceptosCreados);
        Assert.Equal(6, enviada.NivelJerarquiaId);
        Assert.Null(enviada.UnidadesPorEspacio);
    }

    [Fact]
    public void Nivel_hoja_muestra_el_campo_y_lo_exige()
    {
        var cut = RenderComponent<DialogoNuevoConcepto>(p => p.Add(c => c.Niveles, Datos5503.Niveles));

        cut.Find("#concepto-nivel").Change("7");
        cut.Find("#concepto-nombre").Change("SOMBRAS");
        cut.Find("form").Submit();

        Assert.NotEmpty(cut.FindAll("#concepto-unidades"));
        Assert.Contains("unidades por espacio son obligatorias", cut.Markup);
        Assert.Empty(arbol.ConceptosCreados);

        cut.Find("#concepto-unidades").Change("250");
        cut.Find("form").Submit();
        Assert.Equal(250, Assert.Single(arbol.ConceptosCreados).UnidadesPorEspacio);
    }
}
