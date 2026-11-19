using Abastecimiento.Dominio.Entidades.Tiendas;
using Abastecimiento.Infraestructura.Persistencia.Semilla;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Abastecimiento.Infraestructura.Persistencia.Configuraciones;

public class ConfiguracionTienda : IEntityTypeConfiguration<Tienda>
{
    public void Configure(EntityTypeBuilder<Tienda> builder)
    {
        builder.ToTable("Tiendas", Esquemas.Tiendas);
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nombre).HasMaxLength(150).IsRequired();

        builder.HasIndex(t => t.CodigoHanna).IsUnique().HasDatabaseName("UQ_Tiendas_CodigoHanna");
        builder.HasIndex(t => new { t.RegionalId, t.MarcaTiendaId }).HasDatabaseName("IX_Tiendas_RegionalId_MarcaTiendaId");
        builder.HasIndex(t => t.MarcaTiendaId).HasDatabaseName("IX_Tiendas_MarcaTiendaId");
        builder.HasIndex(t => t.TipoTiendaId).HasDatabaseName("IX_Tiendas_TipoTiendaId");
        builder.HasIndex(t => t.ResponsableVisualId).HasDatabaseName("IX_Tiendas_ResponsableVisualId");

        builder.HasOne(t => t.MarcaTienda).WithMany().HasForeignKey(t => t.MarcaTiendaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Regional).WithMany().HasForeignKey(t => t.RegionalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.TipoTienda).WithMany().HasForeignKey(t => t.TipoTiendaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.ResponsableVisual).WithMany().HasForeignKey(t => t.ResponsableVisualId).OnDelete(DeleteBehavior.Restrict);

        builder.HasData(DatosSemilla.Tiendas);
    }
}

public class ConfiguracionTipoEspacio : IEntityTypeConfiguration<TipoEspacio>
{
    public void Configure(EntityTypeBuilder<TipoEspacio> builder)
    {
        builder.ToTable("TiposEspacio", Esquemas.Tiendas, t => t.HasCheckConstraint(
            "CK_TiposEspacio_Categoria", "[Categoria] IN ('Mobiliario','Perimetral')"));
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Categoria).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(t => t.Nombre).IsUnique().HasDatabaseName("UQ_TiposEspacio_Nombre");

        builder.HasData(DatosSemilla.TiposEspacio);
    }
}

public class ConfiguracionEspacioTienda : IEntityTypeConfiguration<EspacioTienda>
{
    public void Configure(EntityTypeBuilder<EspacioTienda> builder)
    {
        builder.ToTable("EspaciosTienda", Esquemas.Tiendas, t => t.HasCheckConstraint(
            "CK_EspaciosTienda_CantidadEspacios", "[CantidadEspacios] >= 0"));
        builder.HasKey(e => new { e.TiendaId, e.TipoEspacioId }).HasName("PK_EspaciosTienda");

        builder.HasIndex(e => e.TipoEspacioId).HasDatabaseName("IX_EspaciosTienda_TipoEspacioId");

        builder.HasOne(e => e.Tienda).WithMany(t => t.Espacios).HasForeignKey(e => e.TiendaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.TipoEspacio).WithMany().HasForeignKey(e => e.TipoEspacioId).OnDelete(DeleteBehavior.Restrict);

        builder.HasData(DatosSemilla.EspaciosTienda);
    }
}
