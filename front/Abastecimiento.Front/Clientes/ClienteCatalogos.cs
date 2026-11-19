using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>Implementacion HTTP de <see cref="IClienteCatalogos"/>.</summary>
public sealed class ClienteCatalogos(HttpClient http) : ClienteApiBase(http), IClienteCatalogos
{
    /// <inheritdoc />
    public Task<IReadOnlyList<CatalogoRespuesta>> ListarAsync(TipoCatalogo tipo, CancellationToken ct) =>
        GetAsync<IReadOnlyList<CatalogoRespuesta>>(Ruta(tipo), ct);

    /// <inheritdoc />
    public Task<CatalogoRespuesta> CrearAsync(TipoCatalogo tipo, CrearCatalogoSolicitud solicitud, CancellationToken ct) =>
        PostAsync<CrearCatalogoSolicitud, CatalogoRespuesta>(Ruta(tipo), solicitud, ct);

    /// <summary>Ruta relativa del catalogo.</summary>
    public static string Ruta(TipoCatalogo tipo) => tipo switch
    {
        TipoCatalogo.MarcasTienda => "api/catalogos/marcas-tienda",
        TipoCatalogo.Regionales => "api/catalogos/regionales",
        TipoCatalogo.TiposTienda => "api/catalogos/tipos-tienda",
        TipoCatalogo.ResponsablesVisuales => "api/catalogos/responsables-visuales",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Catalogo desconocido.")
    };
}
