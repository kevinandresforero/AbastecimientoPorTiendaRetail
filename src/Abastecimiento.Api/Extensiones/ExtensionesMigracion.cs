using Abastecimiento.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Abastecimiento.Api.Extensiones;

public static class ExtensionesMigracion
{
    /// <summary>Aplica las migraciones pendientes (crea la BD y la puebla con la semilla) al arrancar.</summary>
    public static async Task AplicarMigracionesAsync(this WebApplication app)
    {
        await using var alcance = app.Services.CreateAsyncScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<AbastecimientoDbContext>();
        await contexto.Database.MigrateAsync();
    }
}
