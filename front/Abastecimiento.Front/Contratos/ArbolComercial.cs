namespace Abastecimiento.Front.Contratos;

/// <summary>Nivel de la jerarquia comercial (1 Unidad de Negocio ... 7 Tipo de Producto).</summary>
public sealed record NivelRespuesta(int Id, string Nombre, int Profundidad, bool EsHoja);

/// <summary>Concepto comercial de un nivel; solo los del nivel hoja tienen unidades por espacio.</summary>
public sealed record ConceptoRespuesta(int Id, string Nombre, NivelRespuesta Nivel, int? UnidadesPorEspacio);

/// <summary>Nodo del arbol comercial (la API lo entrega en preorden).</summary>
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

/// <summary>Cuerpo de <c>POST /api/conceptos-comerciales</c>.</summary>
public sealed record CrearConceptoSolicitud(int NivelJerarquiaId, string Nombre, int? UnidadesPorEspacio);

/// <summary>Cuerpo de <c>POST /api/nodos-comerciales</c>; el porcentaje va como fraccion (0.3 = 30%).</summary>
public sealed record CrearNodoSolicitud(int? NodoPadreId, int ConceptoComercialId, decimal PorcentajeParticipacion);

/// <summary>Cuerpo de <c>PUT /api/nodos-comerciales/{id}/porcentaje</c> (fraccion).</summary>
public sealed record ActualizarPorcentajeSolicitud(decimal PorcentajeParticipacion);
