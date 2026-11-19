using System.ComponentModel.DataAnnotations;
using Abastecimiento.Dominio.Reglas;

namespace Abastecimiento.Aplicacion.Dtos.ArbolComercial;

/// <summary>Nivel de la jerarquia comercial.</summary>
public sealed record NivelRespuesta(int Id, string Nombre, int Profundidad, bool EsHoja);

/// <summary>Concepto comercial ("que es": GIRLS, LABIALES...).</summary>
public sealed record ConceptoRespuesta(int Id, string Nombre, NivelRespuesta Nivel, int? UnidadesPorEspacio);

/// <summary>Nodo del arbol con su porcentaje propio y el acumulado desde la raiz.</summary>
/// <param name="NodoId">Id del nodo.</param>
/// <param name="NodoPadreId">Id del padre (null en la raiz).</param>
/// <param name="ConceptoComercialId">Concepto del nodo.</param>
/// <param name="Nombre">Nombre del concepto.</param>
/// <param name="Nivel">Nombre del nivel.</param>
/// <param name="Profundidad">Profundidad del nivel.</param>
/// <param name="PorcentajeParticipacion">Fraccion respecto al padre (0.3 = 30%).</param>
/// <param name="PorcentajeAcumulado">Producto de los porcentajes desde la raiz (sin redondeo).</param>
/// <param name="Ruta">Ruta legible desde la raiz.</param>
/// <param name="UnidadesPorEspacio">Solo en el nivel hoja.</param>
/// <param name="EsHoja">Si el nodo es de nivel hoja (Tipo de Producto).</param>
public sealed record NodoArbolRespuesta(
    int NodoId,
    int? NodoPadreId,
    int ConceptoComercialId,
    string Nombre,
    string Nivel,
    int Profundidad,
    decimal PorcentajeParticipacion,
    decimal PorcentajeAcumulado,
    string Ruta,
    int? UnidadesPorEspacio,
    bool EsHoja);

/// <summary>Resultado de copiar el arbol comercial de una tienda a otra.</summary>
/// <param name="CodigoHannaOrigen">Tienda de la que se copio.</param>
/// <param name="CodigoHannaDestino">Tienda que recibio la copia.</param>
/// <param name="NodosCopiados">Cantidad de nodos clonados.</param>
public sealed record CopiaArbolRespuesta(int CodigoHannaOrigen, int CodigoHannaDestino, int NodosCopiados);

/// <summary>Datos para crear un concepto comercial.</summary>
public sealed record CrearConceptoSolicitud
{
    [Range(1, int.MaxValue)]
    public int NivelJerarquiaId { get; init; }

    [Required]
    [StringLength(LongitudesCampo.NombreConceptoComercial)]
    public string Nombre { get; init; } = string.Empty;

    /// <summary>Obligatorio (mayor que 0) solo en el nivel hoja; debe omitirse en los demas niveles.</summary>
    [Range(1, int.MaxValue)]
    public int? UnidadesPorEspacio { get; init; }
}

/// <summary>Datos para ubicar un concepto en el arbol bajo un padre.</summary>
public sealed record CrearNodoSolicitud
{
    /// <summary>Padre del nodo; null para un nodo raiz (profundidad 1).</summary>
    public int? NodoPadreId { get; init; }

    [Range(1, int.MaxValue)]
    public int ConceptoComercialId { get; init; }

    /// <summary>Fraccion respecto al padre, en (0, 1]: 0.3 = 30%. Se guarda con maximo 6 decimales (decimal(18,6)).</summary>
    [Range(typeof(decimal), "0.000001", "1", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal PorcentajeParticipacion { get; init; }
}

/// <summary>Nuevo porcentaje de participacion de un nodo.</summary>
public sealed record ActualizarPorcentajeSolicitud
{
    /// <summary>Fraccion respecto al padre, en (0, 1]: 0.3 = 30%. Se guarda con maximo 6 decimales (decimal(18,6)).</summary>
    [Range(typeof(decimal), "0.000001", "1", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal PorcentajeParticipacion { get; init; }
}
