using System.Globalization;
using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Dtos.Reportes;
using Abastecimiento.Aplicacion.Helpers;
using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Excepciones;
using Abastecimiento.Dominio.Interfaces;
using Abastecimiento.Dominio.Reglas;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>
/// Lee una sola vez las tiendas, los niveles y el arbol completo; ejecuta el motor por tienda con su propio
/// arbol (agrupado por <c>TiendaId</c>) y agrupa el resultado. Las cargas se suman a partir de las lineas ya
/// redondeadas (igual que la tabla del enunciado).
/// </summary>
public sealed class ServicioReportes(
    ITiendaRepositorio tiendas,
    IArbolComercialRepositorio arbolRepositorio,
    IMotorCalculoCarga motor) : IReporteServicio
{
    private const string ClaveTotal = "TOTAL";
    private const string NombreTotal = "TOTAL GENERAL";

    private sealed record CalculoTienda(Tienda Tienda, ResultadoCalculo Resultado);

    public async Task<ConsolidadoRespuesta> ObtenerConsolidadoAsync(AgruparPor agruparPor, int? nivelId, CancellationToken ct)
    {
        if (!Enum.IsDefined(agruparPor))
        {
            throw new ValidacionDominioExcepcion("agruparPor debe ser tienda, marcaTienda, regional o nivel.");
        }

        var niveles = await arbolRepositorio.ObtenerNivelesAsync(ct);
        NivelRespuesta? nivel = null;
        if (agruparPor == AgruparPor.Nivel)
        {
            nivel = ObtenerNivel(nivelId, niveles);
        }

        var todas = await tiendas.ObtenerTodasAsync(ct: ct);
        var nodos = await arbolRepositorio.ObtenerArbolCompletoAsync(ct);
        var profundidadHoja = ValidadorArbolComercial.ObtenerProfundidadHoja(niveles);
        var nodosPorTienda = nodos.ToLookup(n => n.TiendaId);
        var calculos = todas
            .Select(t => new CalculoTienda(
                t,
                motor.Calcular(t.EspaciosTotales, nodosPorTienda[t.Id].ToList(), profundidadHoja)))
            .ToList();

        return agruparPor switch
        {
            AgruparPor.Tienda => PorTienda(calculos, agruparPor, c => c.Tienda.Id, c => c.Tienda.CodigoHanna.ToString(CultureInfo.InvariantCulture), c => c.Tienda.Nombre),
            AgruparPor.MarcaTienda => PorTienda(calculos, agruparPor, c => c.Tienda.MarcaTiendaId, c => c.Tienda.MarcaTiendaId.ToString(CultureInfo.InvariantCulture), c => c.Tienda.MarcaTienda.Nombre),
            AgruparPor.Regional => PorTienda(calculos, agruparPor, c => c.Tienda.RegionalId, c => c.Tienda.RegionalId.ToString(CultureInfo.InvariantCulture), c => c.Tienda.Regional.Nombre),
            _ => PorNivel(calculos, nivel!)
        };
    }

    private static NivelRespuesta ObtenerNivel(int? nivelId, IReadOnlyList<NivelJerarquia> niveles)
    {
        if (nivelId is null)
        {
            throw new ValidacionDominioExcepcion("Para agrupar por nivel se requiere el parametro nivelId.");
        }

        var nivel = niveles.FirstOrDefault(n => n.Id == nivelId)
            ?? throw new ValidacionDominioExcepcion($"El nivel de jerarquia {nivelId} no existe.");

        return MapeoHelper.ANivelRespuesta(nivel, ValidadorArbolComercial.ObtenerProfundidadHoja(niveles));
    }

    /// <summary>Agrupa tiendas completas (tienda, marca de tienda o regional).</summary>
    private static ConsolidadoRespuesta PorTienda(
        IReadOnlyList<CalculoTienda> calculos,
        AgruparPor agruparPor,
        Func<CalculoTienda, int> llave,
        Func<CalculoTienda, string> clave,
        Func<CalculoTienda, string> nombre)
    {
        var grupos = calculos
            .GroupBy(llave)
            .Select(g => new GrupoConsolidadoRespuesta(
                clave(g.First()),
                nombre(g.First()),
                g.Count(),
                g.Sum(c => c.Tienda.EspaciosTotales),
                MapeoHelper.Normalizar(g.Sum(c => c.Resultado.TotalEspaciosAsignados)),
                g.Sum(c => c.Resultado.TotalCarga)));

        // Tiendas por codigo HANNA (como el resto de la API); demas agrupaciones por nombre.
        var ordenados = agruparPor == AgruparPor.Tienda
            ? grupos.OrderBy(g => int.Parse(g.Clave, CultureInfo.InvariantCulture)).ToList()
            : grupos.OrderBy(g => g.Nombre, StringComparer.Ordinal).ToList();

        var total = new GrupoConsolidadoRespuesta(
            ClaveTotal,
            NombreTotal,
            calculos.Count,
            calculos.Sum(c => c.Tienda.EspaciosTotales),
            MapeoHelper.Normalizar(calculos.Sum(c => c.Resultado.TotalEspaciosAsignados)),
            calculos.Sum(c => c.Resultado.TotalCarga));

        return new ConsolidadoRespuesta(agruparPor, null, ordenados, total);
    }

    /// <summary>
    /// Agrupa las lineas de todas las tiendas por el concepto ancestro del nivel pedido
    /// (ej. nivel 3 = marca de producto: LILI BEAUTY / KARITE). Los espacios fisicos no aplican (null).
    /// </summary>
    private static ConsolidadoRespuesta PorNivel(IReadOnlyList<CalculoTienda> calculos, NivelRespuesta nivel)
    {
        var lineas = calculos
            .SelectMany(c => c.Resultado.Lineas.Select(l => new
            {
                c.Tienda.Id,
                Linea = l,
                Ancestro = l.Camino.FirstOrDefault(n => n.ConceptoComercial.NivelJerarquiaId == nivel.Id)
            }))
            .Where(x => x.Ancestro is not null)
            .ToList();

        var grupos = lineas
            .GroupBy(x => x.Ancestro!.ConceptoComercialId)
            .Select(g => new GrupoConsolidadoRespuesta(
                g.Key.ToString(CultureInfo.InvariantCulture),
                g.First().Ancestro!.ConceptoComercial.Nombre,
                g.Select(x => x.Id).Distinct().Count(),
                null,
                MapeoHelper.Normalizar(g.Sum(x => x.Linea.EspaciosCalculados)),
                g.Sum(x => x.Linea.CargaTotal)))
            .OrderBy(g => g.Nombre, StringComparer.Ordinal)
            .ToList();

        var total = new GrupoConsolidadoRespuesta(
            ClaveTotal,
            NombreTotal,
            lineas.Select(x => x.Id).Distinct().Count(),
            null,
            MapeoHelper.Normalizar(lineas.Sum(x => x.Linea.EspaciosCalculados)),
            lineas.Sum(x => x.Linea.CargaTotal));

        return new ConsolidadoRespuesta(AgruparPor.Nivel, nivel, grupos, total);
    }
}
