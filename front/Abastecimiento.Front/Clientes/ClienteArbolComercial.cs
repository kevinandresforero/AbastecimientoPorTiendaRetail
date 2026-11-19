using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Implementacion HTTP de <see cref="IClienteArbolComercial"/>.</summary>
public sealed class ClienteArbolComercial(HttpClient http) : ClienteApiBase(http), IClienteArbolComercial
{
    /// <inheritdoc />
    public Task<IReadOnlyList<NivelRespuesta>> ObtenerNivelesAsync(CancellationToken ct) =>
        GetAsync<IReadOnlyList<NivelRespuesta>>("api/niveles-jerarquia", ct);

    /// <inheritdoc />
    public Task<IReadOnlyList<NodoArbolRespuesta>> ObtenerArbolAsync(CancellationToken ct) =>
        GetAsync<IReadOnlyList<NodoArbolRespuesta>>("api/arbol-comercial", ct);

    /// <inheritdoc />
    public Task<IReadOnlyList<ConceptoRespuesta>> ListarConceptosAsync(int? nivelJerarquiaId, CancellationToken ct) =>
        GetAsync<IReadOnlyList<ConceptoRespuesta>>(
            nivelJerarquiaId is int nivel ? $"api/conceptos-comerciales?nivelJerarquiaId={nivel}" : "api/conceptos-comerciales",
            ct);

    /// <inheritdoc />
    public Task<ConceptoRespuesta> CrearConceptoAsync(CrearConceptoSolicitud solicitud, CancellationToken ct) =>
        PostAsync<CrearConceptoSolicitud, ConceptoRespuesta>("api/conceptos-comerciales", solicitud, ct);

    /// <inheritdoc />
    public Task<NodoArbolRespuesta> CrearNodoAsync(CrearNodoSolicitud solicitud, CancellationToken ct) =>
        PostAsync<CrearNodoSolicitud, NodoArbolRespuesta>("api/nodos-comerciales", solicitud, ct);

    /// <inheritdoc />
    public Task<NodoArbolRespuesta> ActualizarPorcentajeAsync(int nodoId, decimal porcentajeParticipacion, CancellationToken ct) =>
        PutAsync<ActualizarPorcentajeSolicitud, NodoArbolRespuesta>(
            $"api/nodos-comerciales/{nodoId}/porcentaje", new ActualizarPorcentajeSolicitud(porcentajeParticipacion), ct);
}
