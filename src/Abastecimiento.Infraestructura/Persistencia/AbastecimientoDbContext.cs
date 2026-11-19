using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Dominio.Entidades.Seguridad;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Abastecimiento.Infraestructura.Persistencia;

/// <summary>Contexto EF Core de la aplicacion. Las configuraciones y semillas viven en IEntityTypeConfiguration.</summary>
public class AbastecimientoDbContext(DbContextOptions<AbastecimientoDbContext> opciones)
    : DbContext(opciones), IUnidadDeTrabajo
{
    // Tiendas
    public DbSet<MarcaTienda> MarcasTienda => Set<MarcaTienda>();

    public DbSet<Regional> Regionales => Set<Regional>();

    public DbSet<TipoTienda> TiposTienda => Set<TipoTienda>();

    public DbSet<ResponsableVisual> ResponsablesVisuales => Set<ResponsableVisual>();

    public DbSet<Tienda> Tiendas => Set<Tienda>();

    public DbSet<TipoEspacio> TiposEspacio => Set<TipoEspacio>();

    public DbSet<EspacioTienda> EspaciosTienda => Set<EspacioTienda>();

    // Arbol comercial
    public DbSet<NivelJerarquia> NivelesJerarquia => Set<NivelJerarquia>();

    public DbSet<ConceptoComercial> ConceptosComerciales => Set<ConceptoComercial>();

    public DbSet<NodoComercial> NodosComerciales => Set<NodoComercial>();

    // Seguridad (solo datos)
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<UsuarioRol> UsuariosRoles => Set<UsuarioRol>();

    public DbSet<SesionToken> SesionesToken => Set<SesionToken>();

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AbastecimientoDbContext).Assembly);
    }
}
