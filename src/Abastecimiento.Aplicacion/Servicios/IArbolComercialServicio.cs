using Abastecimiento.Aplicacion.Dtos.ArbolComercial;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>Parametrizacion del arbol comercial de N niveles (R2).</summary>
public interface IArbolComercialServicio
{
    /// <summary>Niveles ordenados por profundidad, marcando el nivel hoja.</summary>
    Task<IReadOnlyList<NivelRespuesta>> ObtenerNivelesAsync(CancellationToken ct);

    /// <summary>Arbol completo en preorden con porcentaje acumulado y ruta legible.</summary>
    Task<IReadOnlyList<NodoArbolRespuesta>> ObtenerArbolAsync(CancellationToken ct);

    Task<IReadOnlyList<ConceptoRespuesta>> ListarConceptosAsync(int? nivelJerarquiaId, CancellationToken ct);

    /// <summary>Crea un concepto. Nivel inexistente o reglas de unidades por espacio → 400; duplicado en el nivel → 409.</summary>
    Task<ConceptoRespuesta> CrearConceptoAsync(CrearConceptoSolicitud solicitud, CancellationToken ct);

    /// <summary>Ubica un concepto bajo un padre. Concepto inexistente o reglas del arbol → 400; padre inexistente → 404; duplicado → 409.</summary>
    Task<NodoArbolRespuesta> CrearNodoAsync(CrearNodoSolicitud solicitud, CancellationToken ct);

    /// <summary>Cambia el % de participacion de un nodo. Nodo inexistente → 404; suma de hermanos mayor a 1 → 400.</summary>
    Task<NodoArbolRespuesta> ActualizarPorcentajeAsync(int nodoId, ActualizarPorcentajeSolicitud solicitud, CancellationToken ct);
}
