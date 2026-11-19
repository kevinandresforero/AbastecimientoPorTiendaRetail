using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Abastecimiento.Api.Extensiones;
using Abastecimiento.Api.Middleware;
using Abastecimiento.Aplicacion;
using Abastecimiento.Aplicacion.Configuracion;
using Abastecimiento.Infraestructura;
using Microsoft.OpenApi.Models;

// .env (raiz del repo) → variables de entorno; no pisa las ya definidas (Docker / CI / pruebas).
CargadorArchivoEnv.Cargar(Directory.GetCurrentDirectory());

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("Abastecimiento")
    ?? throw new InvalidOperationException("Falta la cadena de conexion 'ConnectionStrings:Abastecimiento'.");

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.Configure<OpcionesCalculo>(builder.Configuration.GetSection(OpcionesCalculo.Seccion));
builder.Services.AgregarInfraestructura(cadenaConexion);
builder.Services.AgregarAplicacion();

builder.Services.AddExceptionHandler<ManejadorExcepciones>();
builder.Services.AddProblemDetails();

// CORS para el front: solo los origenes de "Cors:OrigenesPermitidos" (lista vacia = ninguno). Se expone "Location"
// para que el front lea la URL del recurso creado (201).
const string PoliticaCorsFront = "Front";
var origenesPermitidos = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(opciones => opciones.AddPolicy(PoliticaCorsFront, politica => politica
    .WithOrigins(origenesPermitidos)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Location")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Abastecimiento Retail",
        Version = "v1",
        Description =
            "Calculo de carga de abastecimiento por tienda.\n\n" +
            "* **Regla 1 — Capacidad fisica total:** Espacios_Totales = Mob_1 + Mob_2 + Mob_3 + Perimetrales.\n" +
            "* **Regla 2 — Distribucion en cascada:** Espacios_Nivel(n) = Espacios_Nivel(n-1) × %_Participacion(n), " +
            "sin redondeos intermedios.\n" +
            "* **Regla 3 — Carga total:** Carga_Total = Espacios_Tipo_Producto × Unidades_Por_Espacio; " +
            "unico redondeo (configurable en `Calculo:ModoRedondeo`, por defecto AwayFromZero).\n\n" +
            "**Precision:** el calculo usa `decimal` con hasta 28 decimales (sin redondeo intermedio); la base de datos " +
            "guarda los porcentajes de participacion con 6 decimales (`decimal(18,6)`: 0.300000 = 30%). " +
            "Las respuestas devuelven los decimales significativos completos; solo las cargas totales son enteras."
    });

    foreach (var ensamblado in new[] { Assembly.GetExecutingAssembly(), typeof(OpcionesCalculo).Assembly })
    {
        var rutaXml = Path.Combine(AppContext.BaseDirectory, $"{ensamblado.GetName().Name}.xml");
        if (File.Exists(rutaXml))
        {
            opciones.IncludeXmlComments(rutaXml);
        }
    }
});

var app = builder.Build();

app.UseExceptionHandler();

// Swagger habilitado en todos los entornos para que el evaluador lo encuentre sin configuracion.
app.UseSwagger();
app.UseSwaggerUI(opciones => opciones.DocumentTitle = "API Abastecimiento Retail");

// Despues del manejador de excepciones: las respuestas de error (problem+json) tambien llevan cabeceras CORS.
app.UseCors(PoliticaCorsFront);

app.MapControllers();

await app.AplicarMigracionesAsync();

app.Run();

/// <summary>Expuesto para las pruebas de integracion (<c>WebApplicationFactory&lt;Program&gt;</c>).</summary>
public partial class Program;
