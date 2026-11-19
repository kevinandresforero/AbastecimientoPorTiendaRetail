using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Infraestructura.Persistencia.Semilla;
using Abastecimiento.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Abastecimiento.Pruebas.Infraestructura;

/// <summary>Verifica el modelo EF contra la spec (seccion 4 y 5.2) sin necesidad de base de datos.</summary>
public class ModeloEfPruebas
{
    private static AbastecimientoDbContext CrearContexto() =>
        new(new DbContextOptionsBuilder<AbastecimientoDbContext>()
            .UseSqlServer("Server=no-se-usa;Database=no-se-usa")
            .Options);

    /// <summary>Modelo de diseno: incluye semilla y CHECKs (el modelo de ejecucion optimizado no los conserva).</summary>
    private static IModel Modelo(AbastecimientoDbContext contexto) => contexto.GetService<IDesignTimeModel>().Model;

    [Fact]
    public void ElModeloTieneLasCatorceTablasEnSusEsquemas()
    {
        using var contexto = CrearContexto();

        var tablas = Modelo(contexto).GetEntityTypes()
            .Select(e => $"{e.GetSchema()}.{e.GetTableName()}")
            .OrderBy(t => t, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "auth.Roles", "auth.SesionesToken", "auth.Usuarios", "auth.UsuariosRoles",
                "nodoscomerciales.ConceptosComerciales", "nodoscomerciales.NivelesJerarquia", "nodoscomerciales.NodosComerciales",
                "tiendas.EspaciosTienda", "tiendas.MarcasTienda", "tiendas.Regionales", "tiendas.ResponsablesVisuales",
                "tiendas.Tiendas", "tiendas.TiposEspacio", "tiendas.TiposTienda"
            },
            tablas);
    }

    [Fact]
    public void NingunaTablaDeNegocioQuedaEnDbo()
    {
        using var contexto = CrearContexto();

        var sinEsquema = Modelo(contexto).GetEntityTypes()
            .Where(e => e.GetSchema() is null or "dbo")
            .Select(e => e.GetTableName());

        Assert.Empty(sinEsquema);
    }

    [Fact]
    public void LosIndicesTienenLosNombresDeLaSpec()
    {
        using var contexto = CrearContexto();

        var indices = Modelo(contexto).GetEntityTypes()
            .SelectMany(e => e.GetIndexes())
            .Select(i => i.GetDatabaseName())
            .ToHashSet();

        string[] esperados =
        [
            "UQ_Tiendas_CodigoHanna", "IX_Tiendas_RegionalId_MarcaTiendaId", "IX_Tiendas_MarcaTiendaId",
            "IX_Tiendas_TipoTiendaId", "IX_Tiendas_ResponsableVisualId", "IX_EspaciosTienda_TipoEspacioId",
            "UQ_NodosComerciales_NodoPadreId_ConceptoComercialId", "IX_NodosComerciales_ConceptoComercialId",
            "UQ_ConceptosComerciales_NivelJerarquiaId_Nombre", "UQ_NivelesJerarquia_Nombre", "UQ_NivelesJerarquia_Profundidad",
            "UQ_MarcasTienda_Nombre", "UQ_Regionales_Nombre", "UQ_TiposTienda_Nombre", "UQ_ResponsablesVisuales_Nombre",
            "UQ_TiposEspacio_Nombre", "UQ_Roles_Nombre", "UQ_Usuarios_NombreUsuario", "UQ_Usuarios_Correo",
            "IX_UsuariosRoles_RolId", "UQ_SesionesToken_Jti", "UQ_SesionesToken_HashTokenRefresco",
            "IX_SesionesToken_UsuarioId_FechaExpiracionUtc", "IX_SesionesToken_ReemplazadaPorSesionId",
            "IX_NodosComerciales_TiendaId", "UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz"
        ];

        Assert.All(esperados, nombre => Assert.Contains(nombre, indices));
        Assert.Equal(esperados.Length, indices.Count);
    }

    [Fact]
    public void LasRestriccionesCheckTienenLosNombresDeLaSpec()
    {
        using var contexto = CrearContexto();

        var checks = Modelo(contexto).GetEntityTypes()
            .SelectMany(e => e.GetCheckConstraints())
            .Select(c => c.Name)
            .OrderBy(n => n)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "CK_ConceptosComerciales_UnidadesPorEspacio", "CK_EspaciosTienda_CantidadEspacios",
                "CK_NivelesJerarquia_Profundidad", "CK_NodosComerciales_PorcentajeParticipacion",
                "CK_SesionesToken_Fechas", "CK_SesionesToken_MotivoRevocacion", "CK_SesionesToken_Reemplazo",
                "CK_SesionesToken_Revocacion", "CK_TiposEspacio_Categoria", "CK_Usuarios_IntentosFallidos"
            },
            checks);
    }

    [Fact]
    public void ElPorcentajeEsDecimal18_6YNoHayFloatNiDouble()
    {
        using var contexto = CrearContexto();

        var porcentaje = Modelo(contexto).FindEntityType(typeof(NodoComercial))!.FindProperty(nameof(NodoComercial.PorcentajeParticipacion))!;
        Assert.Equal("decimal(18,6)", porcentaje.GetColumnType());

        var flotantes = Modelo(contexto).GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(float) || p.ClrType == typeof(double) ||
                        p.ClrType == typeof(float?) || p.ClrType == typeof(double?));
        Assert.Empty(flotantes);
    }

    [Fact]
    public void LaRelacionAutoreferenciadaDelArbolEsRestrict()
    {
        using var contexto = CrearContexto();

        var fk = Modelo(contexto).FindEntityType(typeof(NodoComercial))!
            .GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(NodoComercial));

        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void LaSemillaTieneLosConteosDeLaSpec()
    {
        using var contexto = CrearContexto();

        int Conteo<T>() => Modelo(contexto).FindEntityType(typeof(T))!.GetSeedData().Count();

        Assert.Equal(3, Conteo<Abastecimiento.Dominio.Entidades.Tiendas.MarcaTienda>());
        Assert.Equal(2, Conteo<Abastecimiento.Dominio.Entidades.Tiendas.Regional>());
        Assert.Equal(2, Conteo<Abastecimiento.Dominio.Entidades.Tiendas.TipoTienda>());
        Assert.Equal(0, Conteo<Abastecimiento.Dominio.Entidades.Tiendas.ResponsableVisual>());
        Assert.Equal(7, Conteo<Abastecimiento.Dominio.Entidades.Tiendas.TipoEspacio>());
        Assert.Equal(3, Conteo<Abastecimiento.Dominio.Entidades.Tiendas.Tienda>());
        Assert.Equal(10, Conteo<Abastecimiento.Dominio.Entidades.Tiendas.EspacioTienda>());
        Assert.Equal(7, Conteo<NivelJerarquia>());
        Assert.Equal(15, Conteo<ConceptoComercial>());
        Assert.Equal(54, Conteo<NodoComercial>());
        Assert.Equal(4, Conteo<Abastecimiento.Dominio.Entidades.Seguridad.Rol>());
        Assert.Equal(0, Conteo<Abastecimiento.Dominio.Entidades.Seguridad.Usuario>());
    }

    // ---- Arbol comercial por tienda (spec de datos §4.2 y §7.2) ----

    [Fact]
    public void ElNodoTieneTiendaIdRequeridoConFkRestrictATiendas()
    {
        using var contexto = CrearContexto();
        var nodo = Modelo(contexto).FindEntityType(typeof(NodoComercial))!;

        Assert.False(nodo.FindProperty(nameof(NodoComercial.TiendaId))!.IsNullable);

        var fk = nodo.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Tienda));
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        Assert.Equal("FK_NodosComerciales_Tiendas_TiendaId", fk.GetConstraintName());
        Assert.Equal("tiendas", fk.PrincipalEntityType.GetSchema());
    }

    [Fact]
    public void LaUnidadDeNegocioNoSeRepiteEnLaMismaTienda()
    {
        using var contexto = CrearContexto();

        var indice = Modelo(contexto).FindEntityType(typeof(NodoComercial))!
            .GetIndexes()
            .Single(i => i.GetDatabaseName() == "UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz");

        Assert.True(indice.IsUnique);
        Assert.Equal("[NodoPadreId] IS NULL", indice.GetFilter());
        Assert.Equal(
            new[] { nameof(NodoComercial.TiendaId), nameof(NodoComercial.ConceptoComercialId) },
            indice.Properties.Select(p => p.Name).ToArray());
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 2000)]
    [InlineData(3, 3000)]
    public void LaSemillaTieneUnArbolIdenticoPorTienda(int tiendaId, int desplazamiento)
    {
        var baseTienda1 = DatosSemilla.NodosComercialesDeTienda(1);
        var nodos = DatosSemilla.NodosComerciales.Where(n => n.TiendaId == tiendaId).OrderBy(n => n.Id).ToArray();

        Assert.Equal(18, nodos.Length);
        Assert.Equal(Enumerable.Range(desplazamiento + 1, 18), nodos.Select(n => n.Id));
        Assert.Single(nodos, n => n.NodoPadreId is null);
        for (var i = 0; i < 18; i++)
        {
            Assert.Equal(baseTienda1[i].ConceptoComercialId, nodos[i].ConceptoComercialId);
            Assert.Equal(baseTienda1[i].PorcentajeParticipacion, nodos[i].PorcentajeParticipacion);
            Assert.Equal(baseTienda1[i].NodoPadreId is int padre ? desplazamiento + padre : null, nodos[i].NodoPadreId);
        }
    }

    [Fact]
    public void LaSemillaNoMezclaTiendasEntrePadreEHijo()
    {
        var porId = DatosSemilla.NodosComerciales.ToDictionary(n => n.Id);

        Assert.All(porId.Values.Where(n => n.NodoPadreId is not null),
            n => Assert.Equal(n.TiendaId, porId[n.NodoPadreId!.Value].TiendaId));
    }
}
