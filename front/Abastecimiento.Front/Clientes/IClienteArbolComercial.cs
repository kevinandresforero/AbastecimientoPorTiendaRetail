using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Niveles, arbol, conceptos y nodos comerciales.</summary>
public interface IClienteArbolComercial
{
    /// <summary>GET <c>/api/niveles-jerarquia</c>.</summary>
    Task<IReadOnlyList<NivelRespuesta>> ObtenerNivelesAsync(CancellationToken ct);

    /// <summary>GET <c>/api/arbol-comercial</c> (preorden).</summary>
    Task<IReadOnlyList<NodoArbolRespuesta>> ObtenerArbolAsync(CancellationToken ct);

    /// <summary>GET <c>/api/conceptos-comerciales?nivelJerarquiaId=</c>.</summary>
    Task<IReadOnlyList<ConceptoRespuesta>> ListarConceptosAsync(int? nivelJerarquiaId, CancellationToken ct);

    /// <summary>POST <c>/api/conceptos-comerciales</c>; 400, 409.</summary>
    Task<ConceptoRespuesta> CrearConceptoAsync(CrearConceptoSolicitud solicitud, CancellationToken ct);

    /// <summary>POST <c>/api/nodos-comerciales</c>; 400, 404, 409.</summary>
    Task<NodoArbolRespuesta> CrearNodoAsync(CrearNodoSolicitud solicitud, CancellationToken ct);

    /// <summary>PUT <c>/api/nodos-comerciales/{id}/porcentaje</c> con la fraccion (0.3 = 30%); 400, 404.</summary>
    Task<NodoArbolRespuesta> ActualizarPorcentajeAsync(int nodoId, decimal porcentajeParticipacion, CancellationToken ct);
}
