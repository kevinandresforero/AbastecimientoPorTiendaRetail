using Abastecimiento.Aplicacion.Motor;
using Abastecimiento.Aplicacion.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace Abastecimiento.Aplicacion;

public static class InyeccionDependencias
{
    /// <summary>Registra el motor de calculo y los servicios de aplicacion (Scoped).</summary>
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        servicios.AddScoped<IMotorCalculoCarga, MotorCalculoCarga>();
        servicios.AddScoped<ITiendaServicio, ServicioTiendas>();
        servicios.AddScoped<ICatalogoServicio, ServicioCatalogos>();
        servicios.AddScoped<ITipoEspacioServicio, ServicioTiposEspacio>();
        servicios.AddScoped<IArbolComercialServicio, ServicioArbolComercial>();
        servicios.AddScoped<ICargaServicio, ServicioCarga>();
        servicios.AddScoped<IReporteServicio, ServicioReportes>();

        return servicios;
    }
}
