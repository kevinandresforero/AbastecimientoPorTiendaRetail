using Abastecimiento.Front.Clientes;
using Abastecimiento.Front.Componentes;
using Abastecimiento.Front.Configuracion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// La URL de la API se valida al arrancar: sin una URL absoluta el front no tiene a quien llamar.
var urlBase = OpcionesApi.ObtenerUrlBase(builder.Configuration);
builder.Services.AgregarClientesApi(urlBase);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
