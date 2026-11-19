namespace Abastecimiento.Front.Pruebas.Apoyo;

/// <summary>Ubica el codigo fuente del front (para las pruebas que leen archivos: tema y colores).</summary>
public static class RutasFront
{
    public static string Proyecto { get; } = Buscar();

    public static string Tema => Path.Combine(Proyecto, "wwwroot", "css", "tema.css");

    private static string Buscar()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidato = Path.Combine(dir.FullName, "Abastecimiento.Front");
            if (File.Exists(Path.Combine(candidato, "Abastecimiento.Front.csproj")))
            {
                return candidato;
            }
        }

        throw new DirectoryNotFoundException("No se encontro el proyecto Abastecimiento.Front desde " + AppContext.BaseDirectory);
    }
}
