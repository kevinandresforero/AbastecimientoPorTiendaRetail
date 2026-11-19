using Abastecimiento.Api.Extensiones;

namespace Abastecimiento.Pruebas.Api;

/// <summary>Lectura del archivo .env en un directorio temporal, con variables de nombre unico.</summary>
public sealed class CargadorArchivoEnvPruebas : IDisposable
{
    private readonly string raiz = Path.Combine(Path.GetTempPath(), $"abastecimiento-env-{Guid.NewGuid():N}");
    private readonly string prefijo = $"PRUEBA_ENV_{Guid.NewGuid():N}_";

    public CargadorArchivoEnvPruebas()
    {
        Directory.CreateDirectory(Path.Combine(raiz, "sub", "proyecto"));
    }

    private string Variable(string nombre) => prefijo + nombre;

    [Fact]
    public void CargaClavesValoresYRespetaVariablesExistentes()
    {
        Environment.SetEnvironmentVariable(Variable("EXISTENTE"), "original");
        File.WriteAllLines(Path.Combine(raiz, CargadorArchivoEnv.NombreArchivo),
        [
            "# comentario",
            "",
            $"{Variable("SIMPLE")}=valor simple",
            $"export {Variable("EXPORT")}=con export",
            $"{Variable("DOBLES")}=\"con comillas\"",
            $"{Variable("SIMPLES")}='simples'",
            $"{Variable("CADENA")}=Server=db;Password=a#1",
            $"{Variable("EXISTENTE")}=nuevo",
            "linea sin signo igual"
        ]);

        var ruta = CargadorArchivoEnv.Cargar(Path.Combine(raiz, "sub", "proyecto"));

        Assert.Equal(Path.Combine(raiz, CargadorArchivoEnv.NombreArchivo), ruta);
        Assert.Equal("valor simple", Environment.GetEnvironmentVariable(Variable("SIMPLE")));
        Assert.Equal("con export", Environment.GetEnvironmentVariable(Variable("EXPORT")));
        Assert.Equal("con comillas", Environment.GetEnvironmentVariable(Variable("DOBLES")));
        Assert.Equal("simples", Environment.GetEnvironmentVariable(Variable("SIMPLES")));
        Assert.Equal("Server=db;Password=a#1", Environment.GetEnvironmentVariable(Variable("CADENA")));
        Assert.Equal("original", Environment.GetEnvironmentVariable(Variable("EXISTENTE")));
    }

    [Fact]
    public void SinArchivoDevuelveNull()
    {
        Assert.Null(CargadorArchivoEnv.Cargar(Path.Combine(raiz, "sub", "proyecto")));
    }

    public void Dispose()
    {
        foreach (var nombre in new[] { "SIMPLE", "EXPORT", "DOBLES", "SIMPLES", "CADENA", "EXISTENTE" })
        {
            Environment.SetEnvironmentVariable(Variable(nombre), null);
        }

        Directory.Delete(raiz, recursive: true);
    }
}
