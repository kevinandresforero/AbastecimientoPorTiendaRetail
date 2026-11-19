using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Seguridad;
using Abastecimiento.Dominio.Entidades.Tiendas;

namespace Abastecimiento.Infraestructura.Persistencia.Semilla;

/// <summary>
/// Datos semilla (spec seccion 6) con IDs explicitos. Cada propiedad devuelve instancias nuevas para que
/// el modelo EF y las pruebas no compartan objetos.
/// </summary>
public static class DatosSemilla
{
    public static MarcaTienda[] MarcasTienda =>
    [
        new() { Id = 1, Nombre = "YAHAD" },
        new() { Id = 2, Nombre = "LILI PINK" },
        new() { Id = 3, Nombre = "YOI" }
    ];

    public static Regional[] Regionales =>
    [
        new() { Id = 1, Nombre = "BOGOTA" },
        new() { Id = 2, Nombre = "ANTIOQUIA" }
    ];

    public static TipoTienda[] TiposTienda =>
    [
        new() { Id = 1, Nombre = "MIXTA" },
        new() { Id = 2, Nombre = "MONOMARCA" }
    ];

    public static TipoEspacio[] TiposEspacio =>
    [
        new() { Id = 1, Nombre = "Isla", Categoria = CategoriaEspacio.Mobiliario },
        new() { Id = 2, Nombre = "Casa David", Categoria = CategoriaEspacio.Mobiliario },
        new() { Id = 3, Nombre = "Mesa", Categoria = CategoriaEspacio.Mobiliario },
        new() { Id = 4, Nombre = "Góndola", Categoria = CategoriaEspacio.Mobiliario },
        new() { Id = 5, Nombre = "Mesa Central", Categoria = CategoriaEspacio.Mobiliario },
        new() { Id = 6, Nombre = "Exhibidor", Categoria = CategoriaEspacio.Mobiliario },
        new() { Id = 7, Nombre = "Perimetral", Categoria = CategoriaEspacio.Perimetral }
    ];

    public static Tienda[] Tiendas =>
    [
        new() { Id = 1, CodigoHanna = 5503, Nombre = "LILI EN YOI YAHAD TOBERIN BOGOTA", MarcaTiendaId = 1, RegionalId = 1, TipoTiendaId = 1 },
        new() { Id = 2, CodigoHanna = 1001, Nombre = "LILI PINK UNICENTRO BOGOTA", MarcaTiendaId = 2, RegionalId = 1, TipoTiendaId = 2 },
        new() { Id = 3, CodigoHanna = 2045, Nombre = "YOI VIVA ENVIGADO", MarcaTiendaId = 3, RegionalId = 2, TipoTiendaId = 2 }
    ];

    public static EspacioTienda[] EspaciosTienda =>
    [
        // 5503: Isla 7 + Casa David 4 + Mesa 4 + Perimetral 7 = 22
        new() { TiendaId = 1, TipoEspacioId = 1, CantidadEspacios = 7 },
        new() { TiendaId = 1, TipoEspacioId = 2, CantidadEspacios = 4 },
        new() { TiendaId = 1, TipoEspacioId = 3, CantidadEspacios = 4 },
        new() { TiendaId = 1, TipoEspacioId = 7, CantidadEspacios = 7 },
        // 1001: Gondola 10 + Isla 6 + Perimetral 14 = 30
        new() { TiendaId = 2, TipoEspacioId = 4, CantidadEspacios = 10 },
        new() { TiendaId = 2, TipoEspacioId = 1, CantidadEspacios = 6 },
        new() { TiendaId = 2, TipoEspacioId = 7, CantidadEspacios = 14 },
        // 2045: Mesa Central 8 + Exhibidor 6 + Perimetral 6 = 20
        new() { TiendaId = 3, TipoEspacioId = 5, CantidadEspacios = 8 },
        new() { TiendaId = 3, TipoEspacioId = 6, CantidadEspacios = 6 },
        new() { TiendaId = 3, TipoEspacioId = 7, CantidadEspacios = 6 }
    ];

    public static NivelJerarquia[] NivelesJerarquia =>
    [
        new() { Id = 1, Nombre = "Unidad de Negocio", Profundidad = 1 },
        new() { Id = 2, Nombre = "Categoría", Profundidad = 2 },
        new() { Id = 3, Nombre = "Marca de Producto", Profundidad = 3 },
        new() { Id = 4, Nombre = "Segmento", Profundidad = 4 },
        new() { Id = 5, Nombre = "Subcategoría", Profundidad = 5 },
        new() { Id = 6, Nombre = "Perfil", Profundidad = 6 },
        new() { Id = 7, Nombre = "Tipo de Producto", Profundidad = 7 }
    ];

    public static ConceptoComercial[] ConceptosComerciales =>
    [
        new() { Id = 1, NivelJerarquiaId = 1, Nombre = "BELLEZA Y BIENESTAR" },
        new() { Id = 2, NivelJerarquiaId = 2, Nombre = "BEAUTY" },
        new() { Id = 3, NivelJerarquiaId = 3, Nombre = "LILI BEAUTY" },
        new() { Id = 4, NivelJerarquiaId = 3, Nombre = "KARITE" },
        new() { Id = 5, NivelJerarquiaId = 4, Nombre = "GIRLS" },
        new() { Id = 6, NivelJerarquiaId = 5, Nombre = "MAKEUP" },
        new() { Id = 7, NivelJerarquiaId = 6, Nombre = "LABIOS" },
        new() { Id = 8, NivelJerarquiaId = 6, Nombre = "OJOS" },
        new() { Id = 9, NivelJerarquiaId = 6, Nombre = "ROSTRO" },
        new() { Id = 10, NivelJerarquiaId = 7, Nombre = "LABIALES", UnidadesPorEspacio = 660 },
        new() { Id = 11, NivelJerarquiaId = 7, Nombre = "BRILLO LABIOS", UnidadesPorEspacio = 540 },
        new() { Id = 12, NivelJerarquiaId = 7, Nombre = "PESTAÑINA", UnidadesPorEspacio = 288 },
        new() { Id = 13, NivelJerarquiaId = 7, Nombre = "DELINEADOR", UnidadesPorEspacio = 420 },
        new() { Id = 14, NivelJerarquiaId = 7, Nombre = "BASE / POLVOS", UnidadesPorEspacio = 310 },
        new() { Id = 15, NivelJerarquiaId = 7, Nombre = "BÁLSAMO LABIAL", UnidadesPorEspacio = 600 }
    ];

    /// <summary>
    /// Arbol de referencia del enunciado (tienda 5503, TiendaId 1, Ids 1–18). Las demas tiendas de la semilla reciben una
    /// copia identica con Ids desplazados (ver <see cref="NodosComercialesDeTienda"/>).
    /// </summary>
    private static NodoComercial[] ArbolBase =>
    [
        new() { Id = 1, NodoPadreId = null, ConceptoComercialId = 1, PorcentajeParticipacion = 1.00m },   // BELLEZA Y BIENESTAR
        new() { Id = 2, NodoPadreId = 1, ConceptoComercialId = 2, PorcentajeParticipacion = 0.30m },      // BEAUTY
        new() { Id = 3, NodoPadreId = 2, ConceptoComercialId = 3, PorcentajeParticipacion = 0.94m },      // LILI BEAUTY
        new() { Id = 4, NodoPadreId = 2, ConceptoComercialId = 4, PorcentajeParticipacion = 0.06m },      // KARITE
        new() { Id = 5, NodoPadreId = 3, ConceptoComercialId = 5, PorcentajeParticipacion = 1.00m },      // GIRLS (LILI BEAUTY)
        new() { Id = 6, NodoPadreId = 5, ConceptoComercialId = 6, PorcentajeParticipacion = 1.00m },      // MAKEUP
        new() { Id = 7, NodoPadreId = 6, ConceptoComercialId = 7, PorcentajeParticipacion = 0.40m },      // LABIOS
        new() { Id = 8, NodoPadreId = 6, ConceptoComercialId = 8, PorcentajeParticipacion = 0.45m },      // OJOS
        new() { Id = 9, NodoPadreId = 6, ConceptoComercialId = 9, PorcentajeParticipacion = 0.15m },      // ROSTRO
        new() { Id = 10, NodoPadreId = 7, ConceptoComercialId = 10, PorcentajeParticipacion = 0.50m },    // LABIALES
        new() { Id = 11, NodoPadreId = 7, ConceptoComercialId = 11, PorcentajeParticipacion = 0.50m },    // BRILLO LABIOS
        new() { Id = 12, NodoPadreId = 8, ConceptoComercialId = 12, PorcentajeParticipacion = 0.60m },    // PESTAÑINA
        new() { Id = 13, NodoPadreId = 8, ConceptoComercialId = 13, PorcentajeParticipacion = 0.40m },    // DELINEADOR
        new() { Id = 14, NodoPadreId = 9, ConceptoComercialId = 14, PorcentajeParticipacion = 1.00m },    // BASE / POLVOS
        new() { Id = 15, NodoPadreId = 4, ConceptoComercialId = 5, PorcentajeParticipacion = 1.00m },     // GIRLS (KARITE)
        new() { Id = 16, NodoPadreId = 15, ConceptoComercialId = 6, PorcentajeParticipacion = 1.00m },    // MAKEUP
        new() { Id = 17, NodoPadreId = 16, ConceptoComercialId = 7, PorcentajeParticipacion = 1.00m },    // LABIOS
        new() { Id = 18, NodoPadreId = 17, ConceptoComercialId = 15, PorcentajeParticipacion = 1.00m }    // BÁLSAMO LABIAL
    ];

    /// <summary>Desplazamiento de Ids por tienda: 5503 → 0 (1–18), 1001 → 2000 (2001–2018), 2045 → 3000 (3001–3018).</summary>
    private static readonly IReadOnlyDictionary<int, int> DesplazamientoIdsPorTienda =
        new Dictionary<int, int> { [1] = 0, [2] = 2000, [3] = 3000 };

    /// <summary>Los 54 nodos de la semilla: un arbol identico (18 nodos) por cada una de las 3 tiendas.</summary>
    public static NodoComercial[] NodosComerciales =>
        DesplazamientoIdsPorTienda.Keys.Order().SelectMany(NodosComercialesDeTienda).ToArray();

    /// <summary>
    /// Los 18 nodos de una tienda de la semilla: mismo concepto y porcentaje que el arbol base, con
    /// <c>Id = desplazamiento + Id</c> y <c>NodoPadreId = desplazamiento + NodoPadreId</c> (raiz: null).
    /// </summary>
    public static NodoComercial[] NodosComercialesDeTienda(int tiendaId)
    {
        if (!DesplazamientoIdsPorTienda.TryGetValue(tiendaId, out var desplazamiento))
        {
            throw new ArgumentOutOfRangeException(nameof(tiendaId), tiendaId, "La semilla solo define arboles para las tiendas 1, 2 y 3.");
        }

        return ArbolBase
            .Select(n => new NodoComercial
            {
                Id = desplazamiento + n.Id,
                TiendaId = tiendaId,
                NodoPadreId = n.NodoPadreId is int padre ? desplazamiento + padre : null,
                ConceptoComercialId = n.ConceptoComercialId,
                PorcentajeParticipacion = n.PorcentajeParticipacion
            })
            .ToArray();
    }

    public static Rol[] Roles =>
    [
        new() { Id = 1, Nombre = "admin", Descripcion = "Crea tiendas y administra usuarios, roles y catalogos" },
        new() { Id = 2, Nombre = "admin_tienda", Descripcion = "Administra tiendas existentes (actualiza sus datos y espacios); no crea tiendas" },
        new() { Id = 3, Nombre = "Analista", Descripcion = "Parametriza el arbol comercial (conceptos, nodos y porcentajes)" },
        new() { Id = 4, Nombre = "Consulta", Descripcion = "Solo lectura de calculos y reportes" }
    ];
}
