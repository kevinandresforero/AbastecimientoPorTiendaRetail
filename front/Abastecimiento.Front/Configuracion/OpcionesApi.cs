namespace Abastecimiento.Front.Configuracion;

/// <summary>Configuracion de la API REST que consume el front (seccion <c>Api</c> de appsettings).</summary>
public sealed class OpcionesApi
{
    /// <summary>Nombre de la seccion de configuracion.</summary>
    public const string Seccion = "Api";

    /// <summary>URL base absoluta de la API, ej. <c>http://localhost:5080</c> o <c>http://api:8080</c> en Docker.</summary>
    public string UrlBase { get; set; } = string.Empty;

    /// <summary>Lee y valida <c>Api:UrlBase</c>; falla al arrancar si no es una URL http(s) absoluta.</summary>
    public static Uri ObtenerUrlBase(IConfiguration configuracion)
    {
        var opciones = configuracion.GetSection(Seccion).Get<OpcionesApi>() ?? new OpcionesApi();
        if (!Uri.TryCreate(opciones.UrlBase, UriKind.Absolute, out var url)
            || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"La configuracion '{Seccion}:UrlBase' debe ser una URL absoluta http(s). Valor actual: '{opciones.UrlBase}'.");
        }

        return url;
    }
}
