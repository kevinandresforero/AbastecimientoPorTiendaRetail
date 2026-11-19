using Abastecimiento.Dominio.Entidades;
using Abastecimiento.Dominio.Entidades.Seguridad;
using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Infraestructura.Persistencia.Semilla;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Abastecimiento.Infraestructura.Persistencia.Configuraciones;

/// <summary>
/// Configuracion comun de catalogos simples: tabla propia en su esquema, Nombre requerido con longitud maxima,
/// indice unico <c>UQ_{Tabla}_Nombre</c> y datos semilla.
/// </summary>
public abstract class ConfiguracionCatalogo<T> : IEntityTypeConfiguration<T> where T : EntidadCatalogo
{
    protected abstract string NombreTabla { get; }

    protected abstract string Esquema { get; }

    protected virtual int LongitudNombre => 50;

    protected virtual IEnumerable<T> Semilla => [];

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(NombreTabla, Esquema);
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).HasMaxLength(LongitudNombre).IsRequired();
        builder.HasIndex(c => c.Nombre).IsUnique().HasDatabaseName($"UQ_{NombreTabla}_Nombre");

        var semilla = Semilla.ToArray();
        if (semilla.Length > 0)
        {
            builder.HasData(semilla);
        }
    }
}

public class ConfiguracionMarcaTienda : ConfiguracionCatalogo<MarcaTienda>
{
    protected override string NombreTabla => "MarcasTienda";

    protected override string Esquema => Esquemas.Tiendas;

    protected override IEnumerable<MarcaTienda> Semilla => DatosSemilla.MarcasTienda;
}

public class ConfiguracionRegional : ConfiguracionCatalogo<Regional>
{
    protected override string NombreTabla => "Regionales";

    protected override string Esquema => Esquemas.Tiendas;

    protected override IEnumerable<Regional> Semilla => DatosSemilla.Regionales;
}

public class ConfiguracionTipoTienda : ConfiguracionCatalogo<TipoTienda>
{
    protected override string NombreTabla => "TiposTienda";

    protected override string Esquema => Esquemas.Tiendas;

    protected override IEnumerable<TipoTienda> Semilla => DatosSemilla.TiposTienda;
}

public class ConfiguracionResponsableVisual : ConfiguracionCatalogo<ResponsableVisual>
{
    protected override string NombreTabla => "ResponsablesVisuales";

    protected override string Esquema => Esquemas.Tiendas;

    protected override int LongitudNombre => 100;
}

public class ConfiguracionRol : ConfiguracionCatalogo<Rol>
{
    protected override string NombreTabla => "Roles";

    protected override string Esquema => Esquemas.Auth;

    protected override IEnumerable<Rol> Semilla => DatosSemilla.Roles;

    public override void Configure(EntityTypeBuilder<Rol> builder)
    {
        base.Configure(builder);
        builder.Property(r => r.Descripcion).HasMaxLength(200);
    }
}
