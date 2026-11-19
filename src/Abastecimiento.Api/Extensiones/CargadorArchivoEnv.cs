namespace Abastecimiento.Api.Extensiones;

/// <summary>
/// Carga un archivo <c>.env</c> (formato <c>CLAVE=valor</c>) como variables de entorno, para que la configuracion de
/// ASP.NET Core las lea (ej. <c>ConnectionStrings__Abastecimiento</c>). Busca el archivo desde el directorio indicado
/// hacia arriba (la raiz del repositorio es el mismo <c>.env</c> que usa docker compose).
/// Nunca sobrescribe una variable ya definida: el entorno real (Docker, CI, pruebas) tiene prioridad.
/// </summary>
public static class CargadorArchivoEnv
{
    public const string NombreArchivo = ".env";

    /// <summary>Carga el primer <c>.env</c> encontrado; devuelve su ruta o null si no hay ninguno.</summary>
    public static string? Cargar(string directorioInicial)
    {
        var ruta = Buscar(directorioInicial);
        if (ruta is null)
        {
            return null;
        }

        foreach (var lineaOriginal in File.ReadAllLines(ruta))
        {
            var linea = lineaOriginal.Trim();
            if (linea.Length == 0 || linea.StartsWith('#'))
            {
                continue;
            }

            if (linea.StartsWith("export ", StringComparison.Ordinal))
            {
                linea = linea["export ".Length..].TrimStart();
            }

            var separador = linea.IndexOf('=');
            if (separador <= 0)
            {
                continue;
            }

            var clave = linea[..separador].Trim();
            var valor = QuitarComillas(linea[(separador + 1)..].Trim());

            if (Environment.GetEnvironmentVariable(clave) is null)
            {
                Environment.SetEnvironmentVariable(clave, valor);
            }
        }

        return ruta;
    }

    private static string? Buscar(string directorioInicial)
    {
        for (var directorio = new DirectoryInfo(directorioInicial); directorio is not null; directorio = directorio.Parent)
        {
            var candidato = Path.Combine(directorio.FullName, NombreArchivo);
            if (File.Exists(candidato))
            {
                return candidato;
            }
        }

        return null;
    }

    private static string QuitarComillas(string valor) =>
        valor.Length >= 2 && (valor[0] == '"' || valor[0] == '\'') && valor[^1] == valor[0]
            ? valor[1..^1]
            : valor;
}
