namespace Abastecimiento.Dominio.Entidades;

/// <summary>
/// Base de los catalogos simples (Id + Nombre). Cada catalogo tiene su propia tabla.
/// </summary>
public abstract class EntidadCatalogo
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
}
