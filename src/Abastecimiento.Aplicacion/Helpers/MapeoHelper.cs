using Abastecimiento.Aplicacion.Dtos.ArbolComercial;
using Abastecimiento.Aplicacion.Dtos.Carga;
using Abastecimiento.Aplicacion.Dtos.Catalogos;
using Abastecimiento.Aplicacion.Dtos.Tiendas;
using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Dominio.Entidades;
using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Tiendas;

namespace Abastecimiento.Aplicacion.Helpers;

/// <summary>Conversiones entidad / resultado del motor → DTO. Los servicios nunca devuelven entidades.</summary>
public static class MapeoHelper
{
    /// <summary>
    /// Quita los ceros a la derecha que deja la escala de <c>decimal(18,6)</c> al multiplicar
    /// (1.240800000000... → 1.2408). Dividir entre 1 no altera el valor: <b>no es un redondeo</b>.
    /// </summary>
    public static decimal Normalizar(decimal valor) => valor / 1.000000000000000000000000000000000m;

    public static CatalogoRespuesta ACatalogoRespuesta(EntidadCatalogo catalogo) => new(catalogo.Id, catalogo.Nombre);

    public static TipoEspacioRespuesta ATipoEspacioRespuesta(TipoEspacio tipo) => new(tipo.Id, tipo.Nombre, tipo.Categoria);

    public static TiendaRespuesta ATiendaRespuesta(Tienda tienda) => new(
        tienda.Id,
        tienda.CodigoHanna,
        tienda.Nombre,
        ACatalogoRespuesta(tienda.MarcaTienda),
        ACatalogoRespuesta(tienda.Regional),
        ACatalogoRespuesta(tienda.TipoTienda),
        tienda.ResponsableVisual is null ? null : ACatalogoRespuesta(tienda.ResponsableVisual),
        tienda.Espacios
            .OrderBy(e => e.TipoEspacio.Categoria)
            .ThenBy(e => e.TipoEspacioId)
            .Select(e => new EspacioTiendaRespuesta(e.TipoEspacio.Nombre, e.TipoEspacio.Categoria, e.CantidadEspacios))
            .ToList(),
        tienda.EspaciosTotales);

    public static NivelRespuesta ANivelRespuesta(NivelJerarquia nivel, int profundidadHoja) =>
        new(nivel.Id, nivel.Nombre, nivel.Profundidad, nivel.Profundidad == profundidadHoja);

    public static ConceptoRespuesta AConceptoRespuesta(ConceptoComercial concepto, NivelJerarquia nivel, int profundidadHoja) =>
        new(concepto.Id, concepto.Nombre, ANivelRespuesta(nivel, profundidadHoja), concepto.UnidadesPorEspacio);

    /// <summary>Nodo del arbol a partir de un paso de <see cref="ArbolComercialHelper.RecorrerEnCascada"/> con valor raiz 1.</summary>
    public static NodoArbolRespuesta ANodoArbolRespuesta(NodoRecorrido recorrido, int profundidadHoja)
    {
        var nodo = recorrido.Nodo;
        var concepto = nodo.ConceptoComercial;
        return new NodoArbolRespuesta(
            nodo.Id,
            nodo.NodoPadreId,
            concepto.Id,
            concepto.Nombre,
            concepto.NivelJerarquia.Nombre,
            concepto.NivelJerarquia.Profundidad,
            Normalizar(nodo.PorcentajeParticipacion),
            Normalizar(recorrido.Valor),
            ArbolComercialHelper.RutaLegible(recorrido.Camino),
            concepto.UnidadesPorEspacio,
            ArbolComercialHelper.EsHoja(nodo, profundidadHoja));
    }

    public static CargaTiendaRespuesta ACargaTiendaRespuesta(Tienda tienda, ResultadoCalculo resultado) => new(
        tienda.CodigoHanna,
        tienda.Nombre,
        tienda.EspaciosTotales,
        resultado.Lineas.Select(ALineaCargaRespuesta).ToList(),
        resultado.Nodos.Select(ANodoCalculadoRespuesta).ToList(),
        Normalizar(resultado.TotalEspaciosAsignados),
        resultado.TotalCarga,
        resultado.Advertencias);

    private static LineaCargaRespuesta ALineaCargaRespuesta(LineaCalculada linea) => new(
        linea.Nodo.Id,
        linea.Nodo.ConceptoComercial.Nombre,
        linea.Camino
            .Select(n => new PasoRutaRespuesta(
                n.ConceptoComercial.NivelJerarquia.Nombre,
                n.ConceptoComercial.Nombre,
                Normalizar(n.PorcentajeParticipacion)))
            .ToList(),
        Normalizar(linea.EspaciosCalculados),
        linea.UnidadesPorEspacio,
        Normalizar(linea.CargaCalculada),
        linea.CargaTotal);

    private static NodoCalculadoRespuesta ANodoCalculadoRespuesta(NodoCalculado calculado) => new(
        calculado.Nodo.Id,
        calculado.Nodo.NodoPadreId,
        calculado.Nodo.ConceptoComercial.NivelJerarquia.Nombre,
        calculado.Nodo.ConceptoComercial.Nombre,
        Normalizar(calculado.Nodo.PorcentajeParticipacion),
        Normalizar(calculado.EspaciosCalculados));
}
