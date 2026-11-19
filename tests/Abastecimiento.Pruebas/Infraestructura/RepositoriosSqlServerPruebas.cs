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

[Collection(ColeccionSqlServer.Nombre)]
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

        // Los arboles de las 3 tiendas de la semilla (18 nodos cada uno).
        Assert.Equal(54, nodos.Count);
        Assert.Equal(new[] { 1, 2, 3 }, nodos.Select(n => n.TiendaId).Distinct().Order().ToArray());
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

    // ---- Arbol comercial por tienda ----

    [FactSqlServer]
    public async Task ObtenerArbolPorTiendaDevuelveSoloSusNodosEnUnaSelect()
    {
        var comandos = new List<string>();
        await using var contexto = CrearContexto(comandos);

        var nodos = await new ArbolComercialRepositorio(contexto).ObtenerArbolPorTiendaAsync(1);

        Assert.Equal(18, nodos.Count);
        Assert.All(nodos, n => Assert.Equal(1, n.TiendaId));
        Assert.All(nodos, n => Assert.NotNull(n.ConceptoComercial.NivelJerarquia));
        Assert.Single(nodos, n => n.NodoPadreId is null);
        Assert.Single(comandos);
    }

    [FactSqlServer]
    public async Task ObtenerArbolPorTiendaSinNodosDevuelveListaVacia()
    {
        await using var contexto = CrearContexto([]);

        var nodos = await new ArbolComercialRepositorio(contexto).ObtenerArbolPorTiendaAsync(-1);

        Assert.Empty(nodos);
    }

    [FactSqlServer]
    public async Task ExisteNodoDistingueLaTienda()
    {
        await using var contexto = CrearContexto([]);
        var repositorio = new ArbolComercialRepositorio(contexto);

        Assert.True(await repositorio.ExisteNodoAsync(2, null, 1));     // BELLEZA Y BIENESTAR es unidad de negocio de 1001
        Assert.False(await repositorio.ExisteNodoAsync(2, null, 2));    // BEAUTY no es raiz en 1001
        Assert.True(await repositorio.ExisteNodoAsync(3, 3001, 2));     // BEAUTY bajo la raiz de 2045
        Assert.False(await repositorio.ExisteNodoAsync(3, 1, 2));       // el nodo 1 es de 5503, no de 2045
    }

    [FactSqlServer]
    public async Task LaBdRechazaUnidadDeNegocioRepetidaEnLaMismaTienda()
    {
        await using var contexto = CrearContexto([]);
        await using var transaccion = await contexto.Database.BeginTransactionAsync();

        contexto.NodosComerciales.Add(new() { TiendaId = 1, ConceptoComercialId = 1, PorcentajeParticipacion = 0.10m });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync());
        Assert.Contains("UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz", ex.InnerException?.Message);
    }

    [FactSqlServer]
    public async Task LaBdAceptaLaMismaUnidadDeNegocioEnOtraTiendaYAgregaNodosEnLote()
    {
        await using var contexto = CrearContexto([]);
        await using var transaccion = await contexto.Database.BeginTransactionAsync();
        var tienda = new Abastecimiento.Dominio.Entidades.Tiendas.Tienda
        {
            CodigoHanna = 987654, Nombre = "TIENDA PRUEBA ARBOL", MarcaTiendaId = 1, RegionalId = 1, TipoTiendaId = 1
        };
        contexto.Tiendas.Add(tienda);
        await contexto.SaveChangesAsync();

        // Copia de un arbol en lote: los hijos se enlazan por navegacion y se guardan con un solo SaveChanges.
        var raiz = new Abastecimiento.Dominio.Entidades.ArbolComercial.NodoComercial
        {
            TiendaId = tienda.Id, ConceptoComercialId = 1, PorcentajeParticipacion = 1m
        };
        var hijo = new Abastecimiento.Dominio.Entidades.ArbolComercial.NodoComercial
        {
            TiendaId = tienda.Id, NodoPadre = raiz, ConceptoComercialId = 2, PorcentajeParticipacion = 0.30m
        };
        var repositorio = new ArbolComercialRepositorio(contexto);
        await repositorio.AgregarNodosAsync([raiz, hijo]);
        await contexto.SaveChangesAsync();

        var arbol = await repositorio.ObtenerArbolPorTiendaAsync(tienda.Id);
        Assert.Equal(2, arbol.Count);
        Assert.Equal(raiz.Id, arbol.Single(n => n.ConceptoComercialId == 2).NodoPadreId);
    }

    [FactSqlServer]
    public async Task LaBdRechazaNodoDeTiendaInexistente()
    {
        await using var contexto = CrearContexto([]);
        await using var transaccion = await contexto.Database.BeginTransactionAsync();

        contexto.NodosComerciales.Add(new() { TiendaId = 999999, ConceptoComercialId = 1, PorcentajeParticipacion = 1m });

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => contexto.SaveChangesAsync());
        Assert.Contains("FK_NodosComerciales_Tiendas_TiendaId", ex.InnerException?.Message);
    }
}
