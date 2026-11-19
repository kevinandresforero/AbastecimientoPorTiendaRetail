using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Catalogos simples de <c>/api/catalogos/*</c>.</summary>
public enum TipoCatalogo
{
    /// <summary><c>marcas-tienda</c></summary>
    MarcasTienda,

    /// <summary><c>regionales</c></summary>
    Regionales,

    /// <summary><c>tipos-tienda</c></summary>
    TiposTienda
}

/// <summary>Marcas de tienda, regionales y tipos de tienda.</summary>
public interface IClienteCatalogos
{
    /// <summary>GET <c>/api/catalogos/{tipo}</c>.</summary>
    Task<IReadOnlyList<CatalogoRespuesta>> ListarAsync(TipoCatalogo tipo, CancellationToken ct);

    /// <summary>POST <c>/api/catalogos/{tipo}</c>; 400 nombre invalido, 409 duplicado.</summary>
    Task<CatalogoRespuesta> CrearAsync(TipoCatalogo tipo, CrearCatalogoSolicitud solicitud, CancellationToken ct);
}
