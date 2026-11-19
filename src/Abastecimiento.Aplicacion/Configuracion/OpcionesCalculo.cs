namespace Abastecimiento.Aplicacion.Configuracion;

/// <summary>Opciones del motor de calculo (seccion <c>"Calculo"</c> de appsettings).</summary>
public class OpcionesCalculo
{
    public const string Seccion = "Calculo";

    /// <summary>Regla de redondeo de la Carga Total (unico redondeo del sistema). Por defecto: al entero mas cercano, empates hacia arriba.</summary>
    public MidpointRounding ModoRedondeo { get; set; } = MidpointRounding.AwayFromZero;
}
