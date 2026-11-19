using Abastecimiento.Front.Contratos;

namespace Abastecimiento.Front.Clientes;

/// <summary>
/// Error al llamar a la API: respuesta no exitosa (problem+json) o falla de red. <see cref="Estado"/> = 0 significa que
/// no hubo respuesta (API caida, timeout).
/// </summary>
public sealed class ExcepcionApi : Exception
{
    /// <summary>Mensaje cuando no hay respuesta de la API.</summary>
    public const string MensajeSinConexion = "No se pudo conectar con la API";

    private static readonly IReadOnlyDictionary<string, string[]> SinErroresPorCampo = new Dictionary<string, string[]>();

    /// <summary>Crea la excepcion con los datos del problema.</summary>
    public ExcepcionApi(
        int estado,
        string titulo,
        string? detalle = null,
        IReadOnlyList<string>? errores = null,
        IReadOnlyDictionary<string, string[]>? erroresPorCampo = null,
        Exception? excepcionInterna = null)
        : base(titulo, excepcionInterna)
    {
        Estado = estado;
        Titulo = titulo;
        Detalle = detalle;
        Errores = errores ?? [];
        ErroresPorCampo = erroresPorCampo ?? SinErroresPorCampo;
    }

    /// <summary>Codigo HTTP; 0 si no hubo respuesta.</summary>
    public int Estado { get; }

    /// <summary>Titulo del problema.</summary>
    public string Titulo { get; }

    /// <summary>Detalle legible, si la API lo envio.</summary>
    public string? Detalle { get; }

    /// <summary>Reglas de negocio violadas (<c>errores[]</c>).</summary>
    public IReadOnlyList<string> Errores { get; }

    /// <summary>Errores de validacion por campo (<c>errors{}</c>, claves PascalCase de la API).</summary>
    public IReadOnlyDictionary<string, string[]> ErroresPorCampo { get; }

    /// <summary>True si la API respondio 404.</summary>
    public bool EsNoEncontrado => Estado == 404;

    /// <summary>Error de red o timeout: no hubo respuesta.</summary>
    public static ExcepcionApi SinConexion(Exception interna) => new(
        0,
        MensajeSinConexion,
        "Verifique que la API esté en ejecución e intente de nuevo.",
        excepcionInterna: interna);

    /// <summary>Construye la excepcion desde un problem+json (o desde el codigo HTTP si el cuerpo no es legible).</summary>
    public static ExcepcionApi DesdeProblema(int estado, ProblemaApi? problema, string? razon) => new(
        estado,
        string.IsNullOrWhiteSpace(problema?.Title) ? $"Error {estado}{(string.IsNullOrWhiteSpace(razon) ? "" : $" ({razon})")}" : problema.Title,
        problema?.Detail,
        problema?.Errores,
        problema?.Errors);
}
