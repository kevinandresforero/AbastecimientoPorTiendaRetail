namespace Abastecimiento.Front.Contratos;

/// <summary>Tienda con sus catalogos y espacios (<c>GET /api/tiendas</c>).</summary>
public sealed record TiendaRespuesta(
    int Id,
    int CodigoHanna,
    string Nombre,
    CatalogoRespuesta MarcaTienda,
    CatalogoRespuesta Regional,
    CatalogoRespuesta TipoTienda,
    IReadOnlyList<EspacioTiendaRespuesta> Espacios,
    int EspaciosTotales);

/// <summary>Cantidad de espacios de un tipo dentro de una tienda.</summary>
public sealed record EspacioTiendaRespuesta(string TipoEspacio, CategoriaEspacio Categoria, int CantidadEspacios);

/// <summary>Cuerpo de <c>POST /api/tiendas</c>.</summary>
public sealed record CrearTiendaSolicitud(
    int CodigoHanna,
    string Nombre,
    int MarcaTiendaId,
    int RegionalId,
    int TipoTiendaId,
    IReadOnlyList<EspacioSolicitud> Espacios);

/// <summary>Espacios de un tipo al crear una tienda.</summary>
public sealed record EspacioSolicitud(int TipoEspacioId, int CantidadEspacios);
