using Abastecimiento.Aplicacion.Dtos.ArbolComercial;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>Parametrizacion del arbol comercial de N niveles (R2).</summary>
public interface IArbolComercialServicio
{
    /// <summary>Niveles ordenados por profundidad, marcando el nivel hoja.</summary>
    Task<IReadOnlyList<NivelRespuesta>> ObtenerNivelesAsync(CancellationToken ct);

    /// <summary>Arbol de una tienda en preorden con porcentaje acumulado y ruta legible; tienda sin nodos → lista vacia.</summary>
    Task<IReadOnlyList<NodoArbolRespuesta>> ObtenerArbolAsync(int codigoHanna, CancellationToken ct);

    Task<IReadOnlyList<ConceptoRespuesta>> ListarConceptosAsync(int? nivelJerarquiaId, CancellationToken ct);

    /// <summary>Crea un concepto. Nivel inexistente o reglas de unidades por espacio → 400; duplicado en el nivel → 409.</summary>
    Task<ConceptoRespuesta> CrearConceptoAsync(CrearConceptoSolicitud solicitud, CancellationToken ct);

    /// <summary>
    /// Ubica un concepto en el arbol de una tienda. Concepto inexistente, padre de otra tienda o reglas del arbol → 400;
    /// padre inexistente o tienda inexistente → 404; duplicado dentro de la tienda → 409.
    /// </summary>
    Task<NodoArbolRespuesta> CrearNodoAsync(int codigoHanna, CrearNodoSolicitud solicitud, CancellationToken ct);

    /// <summary>
    /// Clona el arbol de la tienda origen en la tienda destino. Tienda inexistente → 404; destino con nodos → 409;
    /// origen sin nodos o arbol origen invalido → 400.
    /// </summary>
    Task<CopiaArbolRespuesta> CopiarArbolAsync(int codigoHannaDestino, int codigoHannaOrigen, CancellationToken ct);

    /// <summary>Cambia el % de participacion de un nodo. Nodo inexistente → 404; suma de hermanos mayor a 1 → 400.</summary>
    Task<NodoArbolRespuesta> ActualizarPorcentajeAsync(int nodoId, ActualizarPorcentajeSolicitud solicitud, CancellationToken ct);
}
