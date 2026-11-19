namespace Abastecimiento.Aplicacion.Helpers;

/// <summary>Unico punto de redondeo del sistema (nota tecnica de precision del enunciado).</summary>
public static class RedondeoHelper
{
    /// <summary>Redondea la carga calculada de una linea a unidades enteras segun el modo configurado.</summary>
    public static int RedondearCarga(decimal cargaCalculada, MidpointRounding modo) =>
        (int)Math.Round(cargaCalculada, 0, modo);
}
