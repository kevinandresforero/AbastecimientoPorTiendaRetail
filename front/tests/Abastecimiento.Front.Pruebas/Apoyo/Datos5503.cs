using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Pruebas.Apoyo;

/// <summary>Respuesta de <c>GET /api/carga/5503</c> segun la tabla del enunciado (22 espacios totales).</summary>
public static class Datos5503
{
    public static IReadOnlyList<NivelRespuesta> Niveles { get; } =
    [
        new(1, "Unidad de Negocio", 1, false),
        new(2, "Categoría", 2, false),
        new(3, "Marca de Producto", 3, false),
        new(4, "Segmento", 4, false),
        new(5, "Subcategoría", 5, false),
        new(6, "Perfil", 6, false),
        new(7, "Tipo de Producto", 7, true)
    ];

    public static CargaTiendaRespuesta Carga(bool rutaInvertida = false)
    {
        LineaCargaRespuesta Linea(int nodoId, string marca, decimal pctMarca, string perfil, decimal pctPerfil,
            string tipo, decimal pctTipo, decimal espacios, int unidades, decimal cargaCalculada, int cargaTotal)
        {
            List<PasoRutaRespuesta> ruta =
            [
                new("Unidad de Negocio", "BELLEZA Y BIENESTAR", 1m),
                new("Categoría", "BEAUTY", 0.3m),
                new("Marca de Producto", marca, pctMarca),
                new("Segmento", "GIRLS", 1m),
                new("Subcategoría", "MAKEUP", 1m),
                new("Perfil", perfil, pctPerfil),
                new("Tipo de Producto", tipo, pctTipo)
            ];
            if (rutaInvertida)
            {
                ruta.Reverse();
            }

            return new LineaCargaRespuesta(nodoId, tipo, ruta, espacios, unidades, cargaCalculada, cargaTotal);
        }

        return new CargaTiendaRespuesta(
            5503,
            "LILI EN YOI YAHAD TOBERIN",
            22,
            [
                Linea(10, "LILI BEAUTY", 0.94m, "LABIOS", 0.4m, "LABIALES", 0.5m, 1.2408m, 660, 818.928m, 819),
                Linea(11, "LILI BEAUTY", 0.94m, "LABIOS", 0.4m, "BRILLO LABIOS", 0.5m, 1.2408m, 540, 670.032m, 670),
                Linea(12, "LILI BEAUTY", 0.94m, "OJOS", 0.45m, "PESTAÑINA", 0.6m, 1.67508m, 288, 482.42304m, 482),
                Linea(13, "LILI BEAUTY", 0.94m, "OJOS", 0.45m, "DELINEADOR", 0.4m, 1.11672m, 420, 469.0224m, 469),
                Linea(14, "LILI BEAUTY", 0.94m, "ROSTRO", 0.15m, "BASE / POLVOS", 1m, 0.9306m, 310, 288.486m, 288),
                Linea(18, "KARITE", 0.06m, "LABIOS", 1m, "BÁLSAMO LABIAL", 1m, 0.396m, 600, 237.6m, 238)
            ],
            [
                new(1, null, "Unidad de Negocio", "BELLEZA Y BIENESTAR", 1m, 22m),
                new(2, 1, "Categoría", "BEAUTY", 0.3m, 6.6m),
                new(3, 2, "Marca de Producto", "LILI BEAUTY", 0.94m, 6.204m),
                new(4, 2, "Marca de Producto", "KARITE", 0.06m, 0.396m)
            ],
            6.6m,
            2966,
            []);
    }
}
