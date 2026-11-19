using Abastecimiento.Dominio.Entidades.Seguridad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Abastecimiento.Infraestructura.Persistencia.Configuraciones;

public class ConfiguracionUsuario : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios", Esquemas.Auth, t => t.HasCheckConstraint(
            "CK_Usuarios_IntentosFallidos", "[IntentosFallidos] >= 0"));
        builder.HasKey(u => u.Id);

        builder.Property(u => u.NombreUsuario).HasMaxLength(50).IsRequired();
        builder.Property(u => u.Correo).HasMaxLength(150).IsRequired();
        builder.Property(u => u.HashContrasena).HasMaxLength(256).IsRequired();

        // Sentinel = true: un Activo en true se omite en el INSERT y lo pone el DEFAULT; false se envia explicito.
        builder.Property(u => u.Activo).HasDefaultValue(true).HasSentinel(true);
        builder.Property(u => u.IntentosFallidos).HasDefaultValue(0);

        builder.HasIndex(u => u.NombreUsuario).IsUnique().HasDatabaseName("UQ_Usuarios_NombreUsuario");
        builder.HasIndex(u => u.Correo).IsUnique().HasDatabaseName("UQ_Usuarios_Correo");
    }
}

public class ConfiguracionUsuarioRol : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> builder)
    {
        builder.ToTable("UsuariosRoles", Esquemas.Auth);
        builder.HasKey(ur => new { ur.UsuarioId, ur.RolId }).HasName("PK_UsuariosRoles");

        builder.HasIndex(ur => ur.RolId).HasDatabaseName("IX_UsuariosRoles_RolId");

        builder.HasOne(ur => ur.Usuario).WithMany(u => u.Roles).HasForeignKey(ur => ur.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ur => ur.Rol).WithMany().HasForeignKey(ur => ur.RolId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConfiguracionSesionToken : IEntityTypeConfiguration<SesionToken>
{
    public void Configure(EntityTypeBuilder<SesionToken> builder)
    {
        builder.ToTable("SesionesToken", Esquemas.Auth, t =>
        {
            t.HasCheckConstraint("CK_SesionesToken_Fechas", "[FechaExpiracionUtc] > [FechaEmisionUtc]");
            t.HasCheckConstraint("CK_SesionesToken_MotivoRevocacion",
                "[MotivoRevocacion] IS NULL OR [MotivoRevocacion] IN ('Rotacion','CierreSesion','CambioContrasena','ReusoDetectado','Administrativa')");
            t.HasCheckConstraint("CK_SesionesToken_Revocacion",
                "([FechaRevocacionUtc] IS NULL AND [MotivoRevocacion] IS NULL) OR ([FechaRevocacionUtc] IS NOT NULL AND [MotivoRevocacion] IS NOT NULL)");
            t.HasCheckConstraint("CK_SesionesToken_Reemplazo",
                "[ReemplazadaPorSesionId] IS NULL OR [MotivoRevocacion] = 'Rotacion'");
        });
        builder.HasKey(s => s.Id);

        builder.Property(s => s.HashTokenRefresco).HasMaxLength(128).IsRequired();
        builder.Property(s => s.MotivoRevocacion).HasConversion<string>().HasMaxLength(30);
        builder.Property(s => s.DireccionIp).HasMaxLength(45);
        builder.Property(s => s.AgenteUsuario).HasMaxLength(256);

        builder.HasIndex(s => s.Jti).IsUnique().HasDatabaseName("UQ_SesionesToken_Jti");
        builder.HasIndex(s => s.HashTokenRefresco).IsUnique().HasDatabaseName("UQ_SesionesToken_HashTokenRefresco");
        builder.HasIndex(s => new { s.UsuarioId, s.FechaExpiracionUtc }).HasDatabaseName("IX_SesionesToken_UsuarioId_FechaExpiracionUtc");

        builder.HasOne(s => s.Usuario).WithMany(u => u.Sesiones).HasForeignKey(s => s.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.ReemplazadaPorSesion).WithMany().HasForeignKey(s => s.ReemplazadaPorSesionId).OnDelete(DeleteBehavior.NoAction);
    }
}
