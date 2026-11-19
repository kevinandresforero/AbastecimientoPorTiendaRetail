namespace Abastecimiento.Front.Contratos;

/// <summary>
/// Cuerpo <c>application/problem+json</c> de los errores de la API. <see cref="Errores"/> trae las reglas de negocio
/// violadas; <see cref="Errors"/> los errores de validacion por campo, con claves en PascalCase
/// (<c>"CodigoHanna"</c>, <c>"Espacios[0].CantidadEspacios"</c>).
/// </summary>
public sealed class ProblemaApi
{
    /// <summary>URI del tipo de problema.</summary>
    public string? Type { get; set; }

    /// <summary>Titulo corto.</summary>
    public string? Title { get; set; }

    /// <summary>Codigo HTTP.</summary>
    public int? Status { get; set; }

    /// <summary>Detalle legible.</summary>
    public string? Detail { get; set; }

    /// <summary>Reglas de negocio violadas (400).</summary>
    public List<string>? Errores { get; set; }

    /// <summary>Errores de validacion de campos (400 de model binding).</summary>
    public Dictionary<string, string[]>? Errors { get; set; }
}
