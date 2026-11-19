namespace Abastecimiento.Dominio.Entidades.Seguridad;

/// <summary>Rol de autorizacion (admin, admin_tienda, Analista, Consulta). Solo datos en esta fase.</summary>
public class Rol : EntidadCatalogo
{
    public string? Descripcion { get; set; }
}

/// <summary>Usuario del sistema. La contrasena solo se guarda como hash (formato PHC).</summary>
public class Usuario
{
    public Guid Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string HashContrasena { get; set; } = string.Empty;

    /// <summary>Cambia al cambiar contrasena o roles; invalida los tokens emitidos con el sello anterior.</summary>
    public Guid SelloSeguridad { get; set; }

    public bool Activo { get; set; } = true;

    public int IntentosFallidos { get; set; }

    public DateTime? BloqueadoHastaUtc { get; set; }

    public DateTime FechaCreacionUtc { get; set; }

    public ICollection<UsuarioRol> Roles { get; set; } = new List<UsuarioRol>();

    public ICollection<SesionToken> Sesiones { get; set; } = new List<SesionToken>();
}

/// <summary>Asignacion de un rol a un usuario.</summary>
public class UsuarioRol
{
    public Guid UsuarioId { get; set; }

    public int RolId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public Rol Rol { get; set; } = null!;
}

/// <summary>Motivo por el que una sesion de token fue revocada.</summary>
public enum MotivoRevocacion
{
    Rotacion = 1,
    CierreSesion = 2,
    CambioContrasena = 3,
    ReusoDetectado = 4,
    Administrativa = 5
}

/// <summary>
/// Sesion asociada a un JWT: guarda el Jti del token de acceso (revocacion) y el hash SHA-256 del
/// refresh token. El JWT de acceso no se persiste.
/// </summary>
public class SesionToken
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid Jti { get; set; }

    public string HashTokenRefresco { get; set; } = string.Empty;

    public DateTime FechaEmisionUtc { get; set; }

    public DateTime FechaExpiracionUtc { get; set; }

    public DateTime? FechaRevocacionUtc { get; set; }

    public MotivoRevocacion? MotivoRevocacion { get; set; }

    public Guid? ReemplazadaPorSesionId { get; set; }

    public string? DireccionIp { get; set; }

    public string? AgenteUsuario { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public SesionToken? ReemplazadaPorSesion { get; set; }
}
