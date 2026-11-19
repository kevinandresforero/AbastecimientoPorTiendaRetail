using System.Text.RegularExpressions;
using Abastecimiento.Front.Pruebas.Apoyo;

namespace Abastecimiento.Front.Pruebas;

/// <summary>Reglas del spec que se verifican leyendo el codigo: colores solo en tema.css y capas del front.</summary>
public partial class ArquitecturaPruebas
{
    [GeneratedRegex(@"#[0-9A-Fa-f]{3}(?:[0-9A-Fa-f]{3})?(?:[0-9A-Fa-f]{2})?\b|\brgba?\(|\bhsla?\(")]
    private static partial Regex ColorSuelto();

    private static IEnumerable<string> Archivos(string carpeta, params string[] extensiones) =>
        Directory.EnumerateFiles(Path.Combine(RutasFront.Proyecto, carpeta), "*", SearchOption.AllDirectories)
            .Where(f => extensiones.Any(e => f.EndsWith(e, StringComparison.OrdinalIgnoreCase)));

    [Fact]
    public void Componentes_sin_colores_sueltos()
    {
        var hallazgos = Archivos("Componentes", ".razor", ".razor.css", ".cs")
            .SelectMany(f => File.ReadLines(f).Select((linea, i) => (f, i, linea)))
            .Where(x => ColorSuelto().IsMatch(x.linea))
            .Select(x => $"{Path.GetFileName(x.f)}:{x.i + 1}: {x.linea.Trim()}")
            .ToList();

        Assert.Empty(hallazgos);
    }

    [Fact]
    public void App_css_solo_usa_tokens()
    {
        var css = File.ReadAllText(Path.Combine(RutasFront.Proyecto, "wwwroot", "css", "app.css"));
        Assert.DoesNotMatch(ColorSuelto(), css);
    }

    [Fact]
    public void Ningun_razor_tiene_bloque_code()
    {
        var conCodigo = Archivos("Componentes", ".razor")
            .Where(f => File.ReadAllText(f).Contains("@code", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(conCodigo);
    }

    [Fact]
    public void Componentes_no_usan_HttpClient()
    {
        var usan = Archivos("Componentes", ".razor", ".cs")
            .Where(f => File.ReadAllText(f).Contains("HttpClient", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToList();

        Assert.Empty(usan);
    }

    [Fact]
    public void Front_no_referencia_proyectos_del_backend()
    {
        var csproj = File.ReadAllText(Path.Combine(RutasFront.Proyecto, "Abastecimiento.Front.csproj"));
        foreach (var backend in new[] { "Abastecimiento.Dominio", "Abastecimiento.Aplicacion", "Abastecimiento.Infraestructura", "Abastecimiento.Api" })
        {
            Assert.DoesNotContain(backend, csproj, StringComparison.Ordinal);
        }
    }
}
