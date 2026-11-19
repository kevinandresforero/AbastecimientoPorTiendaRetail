using Abastecimiento.Dominio.Entidades.ArbolComercial;
using Abastecimiento.Infraestructura.Persistencia.Semilla;
using Abastecimiento.Dominio.Reglas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Abastecimiento.Infraestructura.Persistencia.Configuraciones;

public class ConfiguracionNivelJerarquia : IEntityTypeConfiguration<NivelJerarquia>
{
    public void Configure(EntityTypeBuilder<NivelJerarquia> builder)
    {
        builder.ToTable("NivelesJerarquia", Esquemas.NodosComerciales, t => t.HasCheckConstraint(
            "CK_NivelesJerarquia_Profundidad", "[Profundidad] >= 1"));
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.Nombre).HasMaxLength(LongitudesCampo.NombreNivelJerarquia).IsRequired();

        builder.HasIndex(n => n.Nombre).IsUnique().HasDatabaseName("UQ_NivelesJerarquia_Nombre");
        builder.HasIndex(n => n.Profundidad).IsUnique().HasDatabaseName("UQ_NivelesJerarquia_Profundidad");

        builder.HasData(DatosSemilla.NivelesJerarquia);
    }
}

public class ConfiguracionConceptoComercial : IEntityTypeConfiguration<ConceptoComercial>
{
    public void Configure(EntityTypeBuilder<ConceptoComercial> builder)
    {
        builder.ToTable("ConceptosComerciales", Esquemas.NodosComerciales, t => t.HasCheckConstraint(
            "CK_ConceptosComerciales_UnidadesPorEspacio", "[UnidadesPorEspacio] IS NULL OR [UnidadesPorEspacio] > 0"));
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre).HasMaxLength(LongitudesCampo.NombreConceptoComercial).IsRequired();

        builder.HasIndex(c => new { c.NivelJerarquiaId, c.Nombre }).IsUnique()
            .HasDatabaseName("UQ_ConceptosComerciales_NivelJerarquiaId_Nombre");

        builder.HasOne(c => c.NivelJerarquia).WithMany().HasForeignKey(c => c.NivelJerarquiaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasData(DatosSemilla.ConceptosComerciales);
    }
}

public class ConfiguracionNodoComercial : IEntityTypeConfiguration<NodoComercial>
{
    public void Configure(EntityTypeBuilder<NodoComercial> builder)
    {
        builder.ToTable("NodosComerciales", Esquemas.NodosComerciales, t => t.HasCheckConstraint(
            "CK_NodosComerciales_PorcentajeParticipacion",
            "[PorcentajeParticipacion] > 0 AND [PorcentajeParticipacion] <= 1"));
        builder.HasKey(n => n.Id);

        builder.Property(n => n.PorcentajeParticipacion).HasPrecision(18, 6);

        // SQL Server: al tener NodoPadreId nullable, EF genera el indice unico filtrado con [NodoPadreId] IS NOT NULL.
        builder.HasIndex(n => new { n.NodoPadreId, n.ConceptoComercialId }).IsUnique()
            .HasDatabaseName("UQ_NodosComerciales_NodoPadreId_ConceptoComercialId");
        builder.HasIndex(n => n.ConceptoComercialId).HasDatabaseName("IX_NodosComerciales_ConceptoComercialId");

        // Arbol por tienda: lectura del arbol de una tienda por indice y unidades de negocio (raices) sin repetir en la tienda.
        builder.HasIndex(n => n.TiendaId).HasDatabaseName("IX_NodosComerciales_TiendaId");
        builder.HasIndex(n => new { n.TiendaId, n.ConceptoComercialId }).IsUnique()
            .HasFilter("[NodoPadreId] IS NULL")
            .HasDatabaseName("UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz");

        // Unica FK entre esquemas (nodoscomerciales -> tiendas): el arbol pertenece a una tienda.
        builder.HasOne(n => n.Tienda).WithMany().HasForeignKey(n => n.TiendaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_NodosComerciales_Tiendas_TiendaId");
        builder.HasOne(n => n.NodoPadre).WithMany(n => n.NodosHijos).HasForeignKey(n => n.NodoPadreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(n => n.ConceptoComercial).WithMany().HasForeignKey(n => n.ConceptoComercialId).OnDelete(DeleteBehavior.Restrict);

        builder.HasData(DatosSemilla.NodosComerciales);
    }
}
