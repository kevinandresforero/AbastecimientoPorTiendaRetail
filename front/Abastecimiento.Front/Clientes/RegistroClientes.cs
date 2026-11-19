namespace Abastecimiento.Front.Clientes;

/// <summary>Registro de los clientes tipados de la API en el contenedor de dependencias.</summary>
public static class RegistroClientes
{
    private static readonly TimeSpan Espera = TimeSpan.FromSeconds(30);

    /// <summary>Registra un <c>HttpClient</c> tipado por recurso apuntando a <paramref name="urlBase"/>.</summary>
    public static IServiceCollection AgregarClientesApi(this IServiceCollection servicios, Uri urlBase)
    {
        void Configurar(HttpClient c)
        {
            c.BaseAddress = urlBase;
            c.Timeout = Espera;
        }

        servicios.AddHttpClient<IClienteTiendas, ClienteTiendas>(Configurar);
        servicios.AddHttpClient<IClienteCatalogos, ClienteCatalogos>(Configurar);
        servicios.AddHttpClient<IClienteTiposEspacio, ClienteTiposEspacio>(Configurar);
        servicios.AddHttpClient<IClienteArbolComercial, ClienteArbolComercial>(Configurar);
        servicios.AddHttpClient<IClienteCarga, ClienteCarga>(Configurar);
        servicios.AddHttpClient<IClienteReportes, ClienteReportes>(Configurar);
        return servicios;
    }
}
