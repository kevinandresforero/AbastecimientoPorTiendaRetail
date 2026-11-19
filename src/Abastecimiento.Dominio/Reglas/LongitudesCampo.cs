namespace Abastecimiento.Dominio.Reglas;

/// <summary>
/// Longitudes maximas de los campos de texto del modelo. Unica fuente para las columnas de EF Core
/// (<c>HasMaxLength</c>), la validacion de forma de los DTOs (<c>[StringLength]</c>) y las reglas de los servicios.
/// Cambiar un valor requiere una migracion nueva.
/// </summary>
public static class LongitudesCampo
{
    /// <summary>Nombre de los catalogos simples: marcas de tienda, regionales, tipos de tienda y roles.</summary>
    public const int NombreCatalogo = 50;

    /// <summary>Nombre de un responsable visual (nombre de persona).</summary>
    public const int NombreResponsableVisual = 100;

    /// <summary>El mayor de los nombres de catalogo; limite de forma comun antes de la regla de cada catalogo.</summary>
    public const int NombreCatalogoMaximo = NombreResponsableVisual;

    public const int NombreTienda = 150;

    public const int NombreTipoEspacio = 50;

    /// <summary>Texto de <c>CategoriaEspacio</c> guardado como cadena (Mobiliario / Perimetral).</summary>
    public const int CategoriaEspacio = 20;

    public const int NombreNivelJerarquia = 50;

    public const int NombreConceptoComercial = 100;
}
