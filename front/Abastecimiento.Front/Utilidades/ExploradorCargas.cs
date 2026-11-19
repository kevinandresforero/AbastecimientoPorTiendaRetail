using System.Globalization;
using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Utilidades;

/// <summary>
/// Fila del explorador: una linea de carga (tipo de producto) de una tienda, con los datos de la tienda para filtrar.
/// Todos los valores vienen de la API (<c>GET /api/tiendas</c> y <c>GET /api/carga/{hanna}</c>); no se recalcula nada.
/// </summary>
public sealed record FilaExplorador(
    TiendaRespuesta Tienda,
    LineaCargaRespuesta Linea,
    PasoRutaRespuesta? MarcaProducto,
    PasoRutaRespuesta? Perfil,
    PasoRutaRespuesta? Hoja)
{
    /// <summary>Clave unica de la fila (tienda + nodo hoja).</summary>
    public string Clave => $"{Tienda.CodigoHanna}-{Linea.NodoId}";
}

/// <summary>Filtros del explorador de cargas; null o vacio = sin filtrar por ese campo.</summary>
public sealed class FiltroCargas
{
    /// <summary>Busqueda libre (tienda, codigo, regional, marca y cualquier concepto de la ruta).</summary>
    public string? Texto { get; set; }

    /// <summary>Regional de la tienda.</summary>
    public int? RegionalId { get; set; }

    /// <summary>Marca de la tienda.</summary>
    public int? MarcaTiendaId { get; set; }

    /// <summary>Tienda (codigo HANNA).</summary>
    public int? CodigoHanna { get; set; }

    /// <summary>Nombre del nivel del arbol por el que se filtra el concepto (ej. "Tipo de Producto").</summary>
    public string? Nivel { get; set; }

    /// <summary>Concepto de ese nivel (ej. "LABIALES"); solo aplica con <see cref="Nivel"/>.</summary>
    public string? Concepto { get; set; }

    /// <summary>True si hay algun filtro activo.</summary>
    public bool TieneFiltros =>
        !string.IsNullOrWhiteSpace(Texto) || RegionalId is not null || MarcaTiendaId is not null || CodigoHanna is not null
        || !string.IsNullOrWhiteSpace(Concepto);

    /// <summary>Quita todos los filtros (conserva el nivel elegido).</summary>
    public void Limpiar()
    {
        Texto = null;
        RegionalId = null;
        MarcaTiendaId = null;
        CodigoHanna = null;
        Concepto = null;
    }
}

/// <summary>Arma y filtra las filas del explorador de cargas (presentacion: solo cruza y filtra datos de la API).</summary>
public static class ExploradorCargas
{
    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;
    private const CompareOptions SinTildesNiMayusculas = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    /// <summary>Una fila por linea de carga de la tienda, con marca, perfil y hoja ubicados por profundidad.</summary>
    public static IEnumerable<FilaExplorador> Construir(
        TiendaRespuesta tienda, CargaTiendaRespuesta carga, IReadOnlyList<NivelRespuesta> niveles) =>
        carga.Lineas.Select(linea => new FilaExplorador(
            tienda,
            linea,
            RutaCarga.PasoEnProfundidad(linea, niveles, RutaCarga.ProfundidadMarca),
            RutaCarga.PasoEnProfundidad(linea, niveles, RutaCarga.ProfundidadPerfil),
            RutaCarga.PasoEnProfundidad(linea, niveles, RutaCarga.ProfundidadHoja(niveles, linea))));

    /// <summary>Aplica todos los filtros a la vez (Y logico).</summary>
    public static List<FilaExplorador> Filtrar(IEnumerable<FilaExplorador> filas, FiltroCargas filtro)
    {
        var palabras = Palabras(filtro.Texto);
        return filas.Where(f =>
                (filtro.RegionalId is not int regional || f.Tienda.Regional.Id == regional)
                && (filtro.MarcaTiendaId is not int marca || f.Tienda.MarcaTienda.Id == marca)
                && (filtro.CodigoHanna is not int hanna || f.Tienda.CodigoHanna == hanna)
                && CumpleConcepto(f, filtro.Nivel, filtro.Concepto)
                && palabras.All(p => Contiene(TextoBuscable(f), p)))
            .ToList();
    }

    /// <summary>Conceptos distintos de un nivel presentes en las filas, ordenados (opciones del filtro).</summary>
    public static List<string> Conceptos(IEnumerable<FilaExplorador> filas, string? nivel) =>
        string.IsNullOrWhiteSpace(nivel)
            ? []
            : filas.SelectMany(f => f.Linea.Ruta)
                .Where(p => string.Equals(p.Nivel, nivel, StringComparison.Ordinal))
                .Select(p => p.Nombre)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Create(CultureInfo.InvariantCulture, ignoreCase: true))
                .ToList();

    /// <summary>True si <paramref name="texto"/> contiene <paramref name="buscado"/> sin distinguir tildes ni mayusculas.</summary>
    public static bool Contiene(string texto, string buscado) =>
        Comparador.IndexOf(texto, buscado, SinTildesNiMayusculas) >= 0;

    /// <summary>Palabras de la busqueda (cada una debe aparecer).</summary>
    public static string[] Palabras(string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? [] : texto.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool CumpleConcepto(FilaExplorador fila, string? nivel, string? concepto) =>
        string.IsNullOrWhiteSpace(nivel) || string.IsNullOrWhiteSpace(concepto)
        || fila.Linea.Ruta.Any(p => string.Equals(p.Nivel, nivel, StringComparison.Ordinal)
                                    && string.Equals(p.Nombre, concepto, StringComparison.Ordinal));

    private static string TextoBuscable(FilaExplorador f) => string.Join(' ',
        f.Tienda.CodigoHanna.ToString(CultureInfo.InvariantCulture),
        f.Tienda.Nombre,
        f.Tienda.Regional.Nombre,
        f.Tienda.MarcaTienda.Nombre,
        f.Tienda.TipoTienda.Nombre,
        string.Join(' ', f.Linea.Ruta.Select(p => p.Nombre)));
}

/// <summary>Filtros de la vista en tabla del arbol comercial.</summary>
public sealed class FiltroNodos
{
    /// <summary>Busqueda libre en el nombre y la ruta del nodo.</summary>
    public string? Texto { get; set; }

    /// <summary>Nombre del nivel (null = todos).</summary>
    public string? Nivel { get; set; }

    /// <summary>Solo nodos hoja.</summary>
    public bool SoloHojas { get; set; }

    /// <summary>Aplica los filtros (Y logico) conservando el orden de la API (preorden).</summary>
    public List<NodoArbolRespuesta> Aplicar(IEnumerable<NodoArbolRespuesta> nodos)
    {
        var palabras = ExploradorCargas.Palabras(Texto);
        return nodos.Where(n =>
                (string.IsNullOrWhiteSpace(Nivel) || string.Equals(n.Nivel, Nivel, StringComparison.Ordinal))
                && (!SoloHojas || n.EsHoja)
                && palabras.All(p => ExploradorCargas.Contiene($"{n.Nombre} {n.Ruta}", p)))
            .ToList();
    }
}
