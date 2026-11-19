using System.Globalization;
using System.Text.RegularExpressions;
using Abastecimiento.Front.Pruebas.Apoyo;

namespace Abastecimiento.Front.Pruebas;

/// <summary>
/// Verifica con la formula WCAG 2.1 cada par de colores de la tabla §7.2 del spec, leyendo los tokens reales de
/// <c>wwwroot/css/tema.css</c>. Texto >= 4.5:1; UI y foco >= 3.0:1.
/// </summary>
public class ContrasteTemaPruebas
{
    private const double Texto = 4.5;
    private const double Ui = 3.0;

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Tokens = new(LeerTokens);

    /// <summary>(primer plano, fondo, minimo, uso). Tabla §7.2 del spec + pares agregados por el front.</summary>
    public static TheoryData<string, string, double, string> Pares => new()
    {
        { "texto", "fondo", Texto, "Texto principal sobre el fondo" },
        { "texto", "superficie", Texto, "Texto sobre tarjetas y campos" },
        { "texto", "fondo-alterno", Texto, "Filas en hover" },
        { "texto", "primario-suave", Texto, "Fila seleccionada y totales" },
        { "texto", "destacado", Texto, "Cifra principal y menu activo (ambar)" },
        { "texto", "destacado-suave", Texto, "Texto sobre ambar suave" },
        { "texto", "acento-suave", Texto, "Texto sobre magenta suave" },
        { "info-texto", "info-fondo", Texto, "Alerta informativa" },
        { "texto-secundario", "fondo", Texto, "Texto secundario sobre el fondo" },
        { "texto-secundario", "superficie", Texto, "Texto secundario sobre tarjetas" },
        { "texto-secundario", "fondo-alterno", Texto, "Texto secundario en hover" },
        { "texto-secundario", "primario-suave", Texto, "Texto secundario en fila seleccionada" },
        { "superficie", "primario", Texto, "Boton primario (morado)" },
        { "superficie", "primario-oscuro", Texto, "Boton primario en hover" },
        { "superficie", "acento", Texto, "Boton secundario (magenta)" },
        { "superficie", "acento-oscuro", Texto, "Boton secundario en hover" },
        { "primario-oscuro", "fondo", Texto, "Enlaces sobre el fondo" },
        { "primario-oscuro", "superficie", Texto, "Enlaces sobre tarjetas" },
        { "primario-oscuro", "fondo-alterno", Texto, "Enlaces en hover de fila" },
        { "primario-oscuro", "primario-suave", Texto, "Encabezados de tabla, chips y alerta de exito" },
        { "acento-oscuro", "fondo", Texto, "Antetitulos magenta" },
        { "acento-oscuro", "superficie", Texto, "Boton de contorno magenta" },
        { "acento-oscuro", "acento-suave", Texto, "Chips magenta y alerta de error" },
        { "fondo", "texto", Texto, "Texto claro en la barra lateral y el hero" },
        { "destacado", "texto", Texto, "Ambar sobre el fondo oscuro" },
        { "fondo", "primario", Texto, "Texto claro sobre el circulo morado del hero" },
        { "fondo", "primario-oscuro", Texto, "Menu lateral en hover" },
        { "advertencia-texto", "advertencia-fondo", Texto, "Alerta de advertencia" },
        { "deshabilitado-texto", "deshabilitado-fondo", Texto, "Campo / boton deshabilitado" },
        { "placeholder", "superficie", Texto, "Placeholder" },
        { "error-texto", "fondo", Texto, "Mensaje de validacion sobre el fondo" },
        { "borde-campo", "superficie", Ui, "Borde de campo sobre tarjetas" },
        { "borde-campo", "fondo", Ui, "Borde de campo sobre el fondo" },
        { "foco", "fondo", Ui, "Anillo de foco sobre el fondo" },
        { "foco", "superficie", Ui, "Anillo de foco sobre tarjetas" },
        { "foco", "fondo-alterno", Ui, "Anillo de foco en hover de fila" },
        { "foco", "primario-suave", Ui, "Anillo de foco en fila seleccionada" },
        { "foco", "acento-suave", Ui, "Anillo de foco en alerta de error" },
        { "foco", "destacado-suave", Ui, "Anillo de foco en alerta de advertencia" },
        { "fondo", "texto", Ui, "Anillo de foco claro en la barra lateral" },
        { "primario", "primario-suave", Ui, "Barra de porcentaje" }
    };

    [Theory]
    [MemberData(nameof(Pares))]
    public void Cada_par_cumple_el_minimo(string primerPlano, string fondo, double minimo, string uso)
    {
        var ratio = Ratio(Color(primerPlano), Color(fondo));
        Assert.True(ratio >= minimo,
            $"{uso}: --color-{primerPlano} sobre --color-{fondo} = {ratio.ToString("0.00", CultureInfo.InvariantCulture)}:1 (minimo {minimo}).");
    }

    [Fact]
    public void Fondo_y_texto_de_la_paleta_sin_blanco_ni_negro_puros()
    {
        Assert.Equal("#F2F2F2", Color("fondo"), ignoreCase: true);
        Assert.Equal("#252623", Color("texto"), ignoreCase: true);
        Assert.Equal(13.59, Math.Round(Ratio(Color("fondo"), Color("texto")), 2));
        var css = File.ReadAllText(RutasFront.Tema);
        Assert.DoesNotContain("#FFFFFF", css, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("#000000", css, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Tokens_declarados_una_sola_vez()
    {
        var css = File.ReadAllText(RutasFront.Tema);
        var repetidos = Regex.Matches(css, @"(--color-[a-z0-9-]+)\s*:\s*#")
            .GroupBy(m => m.Groups[1].Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
        Assert.Empty(repetidos);
    }

    /// <summary>Ratio WCAG 2.1: (L1 + 0.05) / (L2 + 0.05) con L1 la luminancia mayor.</summary>
    public static double Ratio(string colorA, string colorB)
    {
        var a = Luminancia(colorA);
        var b = Luminancia(colorB);
        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    private static double Luminancia(string hex)
    {
        static double Canal(string hex, int inicio)
        {
            var c = int.Parse(hex.AsSpan(inicio, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255.0;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Canal(hex, 1) + 0.7152 * Canal(hex, 3) + 0.0722 * Canal(hex, 5);
    }

    private static string Color(string token) =>
        Tokens.Value.TryGetValue(token, out var hex)
            ? hex
            : throw new Xunit.Sdk.XunitException($"El token --color-{token} no existe en tema.css.");

    private static IReadOnlyDictionary<string, string> LeerTokens()
    {
        var css = File.ReadAllText(RutasFront.Tema);
        return Regex.Matches(css, @"--color-(?<nombre>[a-z0-9-]+)\s*:\s*(?<hex>#[0-9A-Fa-f]{6})\s*;")
            .ToDictionary(m => m.Groups["nombre"].Value, m => m.Groups["hex"].Value);
    }
}
