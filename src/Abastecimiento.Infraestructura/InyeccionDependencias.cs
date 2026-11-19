using Abastecimiento.Dominio.Interfaces;
using Abastecimiento.Infraestructura.Persistencia;
using Abastecimiento.Infraestructura.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Infraestructura;

public static class InyeccionDependencias
{
    /// <summary>Registra el DbContext (SQL Server, sin lazy loading), la unidad de trabajo y los repositorios.</summary>
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddDbContext<AbastecimientoDbContext>(opciones => opciones.UseSqlServer(cadenaConexion));

        servicios.AddScoped<IUnidadDeTrabajo>(sp => sp.GetRequiredService<AbastecimientoDbContext>());
        servicios.AddScoped<ITiendaRepositorio, TiendaRepositorio>();
        servicios.AddScoped<IArbolComercialRepositorio, ArbolComercialRepositorio>();
        servicios.AddScoped(typeof(ICatalogoRepositorio<>), typeof(CatalogoRepositorio<>));
        servicios.AddScoped<ICatalogoRepositorioFabrica, CatalogoRepositorioFabrica>();
        servicios.AddScoped<ITipoEspacioRepositorio, TipoEspacioRepositorio>();

        return servicios;
    }
}
