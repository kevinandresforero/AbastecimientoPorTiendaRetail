using System.Globalization;

namespace Abastecimiento.Front.Utilidades;

/// <summary>
/// Formato de numeros para la UI (cultura es-CO: coma decimal y punto de miles). Solo presenta lo que entrega la API:
/// no recalcula ni redondea valores de negocio; el valor completo va en el tooltip (<c>title</c>).
/// </summary>
public static class Formato
{
    /// <summary>Cultura es-CO con separadores fijos (coma decimal, punto de miles) para no depender de ICU.</summary>
    public static CultureInfo Cultura { get; } = CrearCultura();

    /// <summary>Espacios o decimales con 4 decimales: 6.6 → "6,6000".</summary>
    public static string Decimal4(decimal valor) => valor.ToString("N4", Cultura);

    /// <summary>Valor completo sin ceros sobrantes, para el tooltip: 1.67508 → "1,67508".</summary>
    public static string Completo(decimal valor) => valor.ToString("#,0.############################", Cultura);

    /// <summary>Entero con separador de miles: 2966 → "2.966".</summary>
    public static string Entero(int valor) => valor.ToString("N0", Cultura);

    /// <summary>Fraccion como porcentaje con 4 decimales: 0.94 → "94,0000 %".</summary>
    public static string Porcentaje(decimal fraccion) => $"{(fraccion * 100m).ToString("N4", Cultura)} %";

    /// <summary>Fraccion como porcentaje completo, para el tooltip: 0.305 → "30,5 %".</summary>
    public static string PorcentajeCompleto(decimal fraccion) => $"{Completo(fraccion * 100m)} %";

    /// <summary>Fraccion como texto editable en %: 0.4 → "40"; 0.305 → "30,5".</summary>
    public static string PorcentajeEditable(decimal fraccion) => (fraccion * 100m).ToString("0.####", Cultura);

    private static CultureInfo CrearCultura()
    {
        CultureInfo cultura;
        try
        {
            cultura = (CultureInfo)CultureInfo.GetCultureInfo("es-CO").Clone();
        }
        catch (CultureNotFoundException)
        {
            cultura = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        }

        cultura.NumberFormat.NumberDecimalSeparator = ",";
        cultura.NumberFormat.NumberGroupSeparator = ".";
        return CultureInfo.ReadOnly(cultura);
    }
}
