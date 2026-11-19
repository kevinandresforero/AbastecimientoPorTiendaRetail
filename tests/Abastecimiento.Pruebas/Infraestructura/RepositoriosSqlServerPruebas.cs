using Abastecimiento.Infraestructura.Persistencia;
using Abastecimiento.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Abastecimiento.Pruebas.Infraestructura;

/// <summary>
/// Se ejecuta solo si existe la variable de entorno <c>ABASTECIMIENTO_PRUEBAS_SQL</c> con una cadena de conexion
/// a SQL Server (la BD se migra al iniciar). Sin ella, xUnit marca las pruebas como omitidas.
/// </summary>
public sealed class FactSqlServerAttribute : FactAttribute
{
    public const string Variable = "ABASTECIMIENTO_PRUEBAS_SQL";

    public FactSqlServerAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
        {
            Skip = $"Defina {Variable} con una cadena de conexion de SQL Server para ejecutar esta prueba.";
        }
    }
}

public class RepositoriosSqlServerPruebas
{
    private static AbastecimientoDbContext CrearContexto(List<string> comandos)
    {
        var opciones = new DbContextOptionsBuilder<AbastecimientoDbContext>()
            .UseSqlServer(Environment.GetEnvironmentVariable(FactSqlServerAttribute.Variable))
            .LogTo(comandos.Add, new[] { RelationalEventId.CommandExecuted })
            .Options;

        var contexto = new AbastecimientoDbContext(opciones);
        contexto.Database.Migrate();
        comandos.Clear();
        return contexto;
    }

    [FactSqlServer]
    public async Task ObtenerArbolCompletoEjecutaUnaSolaSelect()
    {
        var comandos = new List<string>();
        await using var contexto = CrearContexto(comandos);

        var nodos = await new ArbolComercialRepositorio(contexto).ObtenerArbolCompletoAsync();

        Assert.Equal(18, nodos.Count);
        Assert.All(nodos, n => Assert.NotNull(n.ConceptoComercial.NivelJerarquia));
        var comando = Assert.Single(comandos);
        Assert.Contains("SELECT", comando);
    }

    [FactSqlServer]
    public async Task ObtenerTiendaPorHannaTraeCatalogosYEspacios()
    {
        var comandos = new List<string>();
        await using var contexto = CrearContexto(comandos);

        var tienda = await new TiendaRepositorio(contexto).ObtenerPorCodigoHannaAsync(5503);

        Assert.NotNull(tienda);
        Assert.Equal("YAHAD", tienda.MarcaTienda.Nombre);
        Assert.Equal("BOGOTA", tienda.Regional.Nombre);
        Assert.Equal(22, tienda.EspaciosTotales);
        Assert.Single(comandos);
    }

    [FactSqlServer]
    public async Task ObtenerTodasFiltraPorRegional()
    {
        var comandos = new List<string>();
        await using var contexto = CrearContexto(comandos);

        var tiendas = await new TiendaRepositorio(contexto).ObtenerTodasAsync(regionalId: 1);

        Assert.Equal(new[] { 1001, 5503 }, tiendas.Select(t => t.CodigoHanna).ToArray());
    }
}
