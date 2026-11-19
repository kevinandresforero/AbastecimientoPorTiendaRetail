using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Utilidades;

/// <summary>
/// Ubica pasos de <c>ruta[]</c> de una linea de carga por la profundidad del nivel: busca el nombre del nivel de esa
/// profundidad en <c>GET /api/niveles-jerarquia</c> y luego el paso con ese nivel (no un indice fijo).
/// </summary>
public static class RutaCarga
{
    /// <summary>Profundidad del nivel "Marca de Producto".</summary>
    public const int ProfundidadMarca = 3;

    /// <summary>Profundidad del nivel "Perfil".</summary>
    public const int ProfundidadPerfil = 6;

    /// <summary>Profundidad del nivel hoja (la mayor marcada como hoja; sin niveles, el largo de la ruta).</summary>
    public static int ProfundidadHoja(IReadOnlyList<NivelRespuesta> niveles, LineaCargaRespuesta linea) =>
        niveles.Where(n => n.EsHoja).Select(n => n.Profundidad).DefaultIfEmpty(linea.Ruta.Count).Max();

    /// <summary>Paso de la ruta cuyo nivel tiene la profundidad indicada (null si la ruta no lo tiene).</summary>
    public static PasoRutaRespuesta? PasoEnProfundidad(
        LineaCargaRespuesta linea, IReadOnlyList<NivelRespuesta> niveles, int profundidad)
    {
        var nivel = niveles.FirstOrDefault(n => n.Profundidad == profundidad);
        if (nivel is not null)
        {
            return linea.Ruta.FirstOrDefault(p => string.Equals(p.Nivel, nivel.Nombre, StringComparison.Ordinal));
        }

        // Sin catalogo de niveles: la ruta va de la raiz (profundidad 1) a la hoja, sin saltos de nivel (lo valida la API).
        return profundidad >= 1 && profundidad <= linea.Ruta.Count ? linea.Ruta[profundidad - 1] : null;
    }
}
