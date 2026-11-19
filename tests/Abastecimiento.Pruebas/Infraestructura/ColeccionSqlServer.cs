namespace Abastecimiento.Pruebas.Infraestructura;

/// <summary>
/// Las pruebas que usan la base de datos de <c>ABASTECIMIENTO_PRUEBAS_SQL</c> se ejecutan en serie: todas la migran al
/// iniciar y, en paralelo, compiten creando los mismos esquemas.
/// </summary>
[CollectionDefinition(Nombre)]
public sealed class ColeccionSqlServer
{
    public const string Nombre = "SqlServer";
}
