using System.ComponentModel.DataAnnotations;

namespace Abastecimiento.Aplicacion.Dtos.Catalogos;

/// <summary>Elemento de un catalogo simple (marca de tienda, regional, tipo de tienda, responsable visual).</summary>
public sealed record CatalogoRespuesta(int Id, string Nombre);

/// <summary>Datos para crear un elemento de catalogo.</summary>
public sealed record CrearCatalogoSolicitud
{
    /// <summary>Nombre unico dentro del catalogo (maximo 50 caracteres; 100 en responsables visuales).</summary>
    [Required]
    [StringLength(100)]
    public string Nombre { get; init; } = string.Empty;
}
