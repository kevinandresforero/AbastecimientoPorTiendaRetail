using Microsoft.AspNetCore.HttpLogging;

namespace Abastecimiento.Api.Extensiones;

/// <summary>
/// Log de peticiones HTTP con el middleware integrado de ASP.NET Core (<c>HttpLogging</c>), configurado por inyeccion
/// de dependencias (<see cref="HttpLoggingOptions"/> via <c>IOptions</c>; escribe con el <c>ILogger</c> del framework).
/// Una linea por peticion a <c>/api</c> con metodo, ruta, query, <b>estado de la respuesta</b> y duracion; sin headers ni
/// cuerpos para mantener el log pequeno. Se enciende o apaga con el nivel de la categoria
/// <c>Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware</c> en <c>appsettings.json</c>.
/// </summary>
public static class ExtensionesRegistroPeticiones
{
    /// <summary>Prefijo de las rutas registradas: Swagger y archivos estaticos quedan fuera del log.</summary>
    public const string PrefijoRutasRegistradas = "/api";

    /// <summary>Registra y configura el servicio de log de peticiones.</summary>
    public static IServiceCollection AgregarRegistroPeticiones(this IServiceCollection servicios) =>
        servicios.AddHttpLogging(opciones =>
        {
            opciones.LoggingFields = HttpLoggingFields.RequestMethod
                | HttpLoggingFields.RequestPath
                | HttpLoggingFields.RequestQuery
                | HttpLoggingFields.ResponseStatusCode
                | HttpLoggingFields.Duration;

            // Peticion y respuesta en una sola linea.
            opciones.CombineLogs = true;
        });

    /// <summary>
    /// Agrega el middleware para las rutas <c>/api</c>. Debe ir <b>primero</b> en el pipeline (antes del manejador de
    /// excepciones) para registrar el estado final de la respuesta, incluidos 404, 409 y 500.
    /// </summary>
    public static WebApplication UsarRegistroPeticiones(this WebApplication app)
    {
        app.UseWhen(
            contexto => contexto.Request.Path.StartsWithSegments(PrefijoRutasRegistradas),
            rama => rama.UseHttpLogging());
        return app;
    }
}
