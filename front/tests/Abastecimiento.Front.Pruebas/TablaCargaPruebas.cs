using Abastecimiento.Front.Componentes.Compartidos;
using Abastecimiento.Front.Pruebas.Apoyo;

namespace Abastecimiento.Front.Pruebas;

public class TablaCargaPruebas : TestContext
{
    private IRenderedComponent<TablaCarga> Renderizar(bool rutaInvertida = false) =>
        RenderComponent<TablaCarga>(p => p
            .Add(c => c.Carga, Datos5503.Carga(rutaInvertida))
            .Add(c => c.Niveles, Datos5503.Niveles));

    private static List<string> Textos(IRenderedFragment cut, string selector) =>
        cut.FindAll(selector).Select(e => e.TextContent.Trim()).ToList();

    [Fact]
    public void Muestra_las_6_lineas_de_5503_con_las_cargas_del_enunciado()
    {
        var cut = Renderizar();

        Assert.Equal(6, cut.FindAll("tbody tr").Count);
        Assert.Equal(["819", "670", "482", "469", "288", "238"], Textos(cut, "td.celda-carga"));
    }

    [Fact]
    public void Fila_total_muestra_6_6000_espacios_y_2966_unidades()
    {
        var cut = Renderizar();

        Assert.Equal("6,6000", cut.Find("td.total-espacios").TextContent.Trim());
        Assert.Equal("2.966", cut.Find("td.total-carga").TextContent.Trim());
    }

    [Fact]
    public void Espacios_con_4_decimales_y_valor_completo_en_el_title()
    {
        var cut = Renderizar();

        var pestanina = cut.FindAll("td.celda-espacios")[2];
        Assert.Equal("1,6751", pestanina.TextContent.Trim());
        Assert.Equal("1,67508", pestanina.GetAttribute("title"));
    }

    [Fact]
    public void Marca_y_perfil_se_toman_de_la_ruta()
    {
        var cut = Renderizar();

        Assert.Equal(
            ["LILI BEAUTY", "LILI BEAUTY", "LILI BEAUTY", "LILI BEAUTY", "LILI BEAUTY", "KARITE"],
            Textos(cut, "td.celda-marca"));
        var perfiles = Textos(cut, "td.celda-perfil");
        Assert.StartsWith("LABIOS", perfiles[0]);
        Assert.Contains("(40,0000 %)", perfiles[0]);
        Assert.StartsWith("OJOS", perfiles[2]);
        Assert.Contains("(100,0000 %)", perfiles[5]);
        Assert.Contains("PESTAÑINA", cut.FindAll("td.celda-tipo")[2].TextContent);
        Assert.Contains("(60,0000 %)", cut.FindAll("td.celda-tipo")[2].TextContent);
    }

    [Fact]
    public void Marca_y_perfil_se_ubican_por_profundidad_del_nivel_no_por_indice()
    {
        // Con la ruta en orden inverso un indice fijo tomaria otros conceptos; por nombre de nivel no cambia nada.
        var cut = Renderizar(rutaInvertida: true);

        Assert.Equal("LILI BEAUTY", Textos(cut, "td.celda-marca")[0]);
        Assert.StartsWith("LABIOS", Textos(cut, "td.celda-perfil")[0]);
        Assert.Equal("KARITE", Textos(cut, "td.celda-marca")[5]);
    }
}
