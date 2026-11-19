using Abastecimiento.Front.Contratos;
using Abastecimiento.Front.Utilidades;
using Microsoft.AspNetCore.Components;

namespace Abastecimiento.Front.Componentes.Compartidos;

/// <summary>
/// Tabla de lineas de carga con totales (sin llamadas HTTP). Las columnas de marca y perfil se toman de <c>ruta[]</c> por
/// la profundidad del nivel: se busca el nombre del nivel de esa profundidad en <see cref="Niveles"/> y luego el paso de
/// la ruta con ese nivel (no un indice fijo).
/// </summary>
public partial class TablaCarga
{
    /// <summary>Resultado de <c>GET /api/carga/{codigoHanna}</c>.</summary>
    [Parameter, EditorRequired]
    public CargaTiendaRespuesta Carga { get; set; } = default!;

    /// <summary>Niveles de <c>GET /api/niveles-jerarquia</c> (para ubicar marca, perfil y hoja por profundidad).</summary>
    [Parameter]
    public IReadOnlyList<NivelRespuesta> Niveles { get; set; } = [];

    private IReadOnlyList<FilaTablaCarga> Filas { get; set; } = [];

    /// <inheritdoc />
    protected override void OnParametersSet() =>
        Filas = Carga.Lineas.Select(CrearFila).ToList();

    private FilaTablaCarga CrearFila(LineaCargaRespuesta linea) => new(
        linea,
        RutaCarga.PasoEnProfundidad(linea, Niveles, RutaCarga.ProfundidadMarca),
        RutaCarga.PasoEnProfundidad(linea, Niveles, RutaCarga.ProfundidadPerfil),
        RutaCarga.PasoEnProfundidad(linea, Niveles, RutaCarga.ProfundidadHoja(Niveles, linea)));

    private static string NombreDe(PasoRutaRespuesta? paso) => paso?.Nombre ?? "—";
}

/// <summary>Fila de la tabla de carga con los pasos de la ruta ya ubicados.</summary>
public sealed record FilaTablaCarga(
    LineaCargaRespuesta Linea,
    PasoRutaRespuesta? Marca,
    PasoRutaRespuesta? Perfil,
    PasoRutaRespuesta? TipoProducto);
