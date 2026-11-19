using Abastecimiento.Aplicacion.Dtos.Catalogos;
using Abastecimiento.Dominio.Entidades;

namespace Abastecimiento.Aplicacion.Servicios;

/// <summary>Catalogos simples de tienda: marcas, regionales, tipos de tienda y responsables visuales (R1).</summary>
public interface ICatalogoServicio
{
    Task<IReadOnlyList<CatalogoRespuesta>> ListarAsync<T>(CancellationToken ct) where T : EntidadCatalogo;

    /// <summary>Crea un elemento; nombre duplicado → 409, longitud excedida → 400.</summary>
    Task<CatalogoRespuesta> CrearAsync<T>(CrearCatalogoSolicitud solicitud, CancellationToken ct) where T : EntidadCatalogo, new();
}
