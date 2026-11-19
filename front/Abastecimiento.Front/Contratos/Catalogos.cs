namespace Abastecimiento.Front.Contratos;

/// <summary>Elemento de un catalogo simple (marca de tienda, regional, tipo de tienda, responsable visual).</summary>
public sealed record CatalogoRespuesta(int Id, string Nombre);

/// <summary>Cuerpo de <c>POST /api/catalogos/*</c>.</summary>
public sealed record CrearCatalogoSolicitud(string Nombre);

/// <summary>Categoria de un tipo de espacio; la API la envia como texto camelCase.</summary>
public enum CategoriaEspacio
{
    /// <summary>Mueble (isla, mesa, gondola...). Una tienda admite maximo 3 tipos.</summary>
    Mobiliario,

    /// <summary>Espacios en pared; no cuentan en el maximo de mobiliario.</summary>
    Perimetral
}

/// <summary>Tipo de espacio (<c>GET /api/tipos-espacio</c>).</summary>
public sealed record TipoEspacioRespuesta(int Id, string Nombre, CategoriaEspacio Categoria);

/// <summary>Cuerpo de <c>POST /api/tipos-espacio</c>.</summary>
public sealed record CrearTipoEspacioSolicitud(string Nombre, CategoriaEspacio Categoria);
