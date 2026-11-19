# AGENT SPECIFICATION: Capa de Aplicacion y API (.NET 8 / ASP.NET Core)

## 1. Contexto y Objetivo del Agente
Actua como un **Desarrollador Senior .NET**. Tu tarea es implementar el backend del sistema de calculo de carga de
abastecimiento retail: los proyectos `Abastecimiento.Aplicacion` (servicios, motor de calculo, helpers, DTOs) y
`Abastecimiento.Api` (controllers, Swagger, manejo de excepciones), sobre la capa de datos ya implementada y validada.

Documentos de referencia, **en este orden de prioridad**:
1. `Prueba Técnica - Desarrollador .md` (el enunciado: reglas de calculo, requerimientos R1–R4, datos esperados).
2. `agent_spec_capa_de_datos_net_8.md` (entidades, esquemas, repositorios, semilla). **No se modifica el modelo.**
3. Este documento.

Criterios de evaluacion del enunciado que guian este spec: Logica de negocio **40%**, Modelado 30% (ya cubierto),
Calidad del codigo **20%**, Documentacion **10%**.

### 1.1. Trazabilidad con el enunciado
| Enunciado | Donde se cumple |
|---|---|
| Regla 1 — Capacidad fisica total | `Tienda.EspaciosTotales` (dominio); expuesto en `TiendaRespuesta` y `CargaTiendaRespuesta` |
| Regla 2 — Distribucion en cascada | `MotorCalculoCarga` + `ArbolComercialHelper` (§4) |
| Regla 3 — Carga total en unidades | `MotorCalculoCarga` + `RedondeoHelper` (§4) |
| Nota de precision (redondeo solo al final) | §2 y §4: `decimal` sin redondeo intermedio; redondeo solo en `cargaTotal` |
| R1 — Gestion de tiendas y mobiliario | `ServicioTiendas`, `ServicioCatalogos`, `ServicioTiposEspacio` (§5); endpoints §6.1 |
| R2 — Parametrizacion del arbol (7 niveles) | `ServicioArbolComercial` (§5); endpoints §6.2 |
| R3 — Motor de calculo por codigo HANNA | `ServicioCarga` + motor; `GET /api/carga/{codigoHanna}` |
| R4a — Desglose por producto de una tienda | `GET /api/carga/{codigoHanna}` (ej. 5503) |
| R4b — Consolidado por Tienda, Marca o Regional | `ServicioReportes`; `GET /api/reportes/consolidado` |
| API REST documentada con Swagger | §6 y §9 |
| SOLID, separacion de responsabilidades, excepciones | §2, §3 y §8 |

---

## 2. Reglas de Arquitectura y Restricciones (STRICT)
* **Alcance:** solo lo que pide el enunciado. Fuera de alcance (extension documentada): editar/eliminar tiendas, autenticacion y autorizacion por roles (los roles existen solo como datos).
* **Capas y dependencias:**
  * `Api` → `Aplicacion` → `Dominio`; `Api` → `Infraestructura` solo para el registro de dependencias y las migraciones.
  * `Aplicacion` depende **solo** de `Dominio` (interfaces de repositorio, entidades, reglas y excepciones). No referencia EF Core ni ASP.NET Core.
* **Controllers delgados:** reciben el DTO, llaman **un** metodo de un servicio y devuelven el codigo HTTP. Sin logica de negocio, sin repositorios, sin `DbContext`, sin try/catch.
* **Services:** contienen la logica de negocio y orquestan repositorios, reglas de dominio y el motor. Devuelven **solo DTOs**, nunca entidades. Cada servicio tiene su interfaz (`I<Nombre>`), registrada como `Scoped`.
* **Helpers:** clases `static` puras (sin estado, sin E/S, sin dependencias inyectadas) para operaciones que se repiten en varios servicios: recorrido del arbol, redondeo y mapeo entidad → DTO.
* **Reglas de dominio existentes se reutilizan, no se duplican:** `ReglasEspaciosTienda.Validar`, `ValidadorArbolComercial.ValidarConcepto / ValidarNodo / ObtenerProfundidadHoja`.
* **Precision:** `decimal` en todo calculo de espacios, porcentajes y carga. PROHIBIDO `float`/`double`. **Ningun redondeo intermedio**; el unico redondeo es la Carga Total de cada linea (nota tecnica del enunciado).
* **Generico en profundidad:** ningun numero de nivel fijo (ni "7") en el codigo; la hoja se deriva con `ValidadorArbolComercial.ObtenerProfundidadHoja`.
* **Idioma:** espanol sin tildes ni ñ en clases, metodos, rutas, parametros y propiedades JSON (camelCase). Los valores de datos conservan su texto original.
* **Async** de punta a punta con `CancellationToken` propagado desde el controller hasta el repositorio.
* **Sin librerias extra:** sin MediatR, AutoMapper ni FluentValidation. Validacion de forma con DataAnnotations en los DTOs de solicitud; validacion de negocio con las reglas de dominio.
* **Errores:** los servicios lanzan las excepciones de dominio (`NoEncontradoExcepcion`, `ConflictoExcepcion`, `ValidacionDominioExcepcion`); un unico manejador global las traduce a `ProblemDetails` (§8).
* **Nulabilidad** habilitada y compilacion sin warnings (`TreatWarningsAsErrors` ya activo en `Directory.Build.props`).

---

## 3. Estructura de Archivos
```
src/Abastecimiento.Aplicacion/
  Configuracion/OpcionesCalculo.cs
  Helpers/ArbolComercialHelper.cs
  Helpers/RedondeoHelper.cs
  Helpers/MapeoHelper.cs
  Motor/IMotorCalculoCarga.cs
  Motor/MotorCalculoCarga.cs
  Motor/ResultadoCalculo.cs              (modelos internos del motor)
  Servicios/ITiendaServicio.cs           + ServicioTiendas.cs
  Servicios/ICatalogoServicio.cs         + ServicioCatalogos.cs
  Servicios/ITipoEspacioServicio.cs      + ServicioTiposEspacio.cs
  Servicios/IArbolComercialServicio.cs   + ServicioArbolComercial.cs
  Servicios/ICargaServicio.cs            + ServicioCarga.cs
  Servicios/IReporteServicio.cs          + ServicioReportes.cs
  Dtos/Tiendas/...  Dtos/Catalogos/...  Dtos/ArbolComercial/...  Dtos/Carga/...  Dtos/Reportes/...
  InyeccionDependencias.cs               (AgregarAplicacion)

src/Abastecimiento.Api/
  Controllers/TiendasController.cs
  Controllers/CatalogosController.cs
  Controllers/TiposEspacioController.cs
  Controllers/NivelesJerarquiaController.cs
  Controllers/ArbolComercialController.cs
  Controllers/ConceptosComercialesController.cs
  Controllers/NodosComercialesController.cs
  Controllers/CargaController.cs
  Controllers/ReportesController.cs
  Middleware/ManejadorExcepciones.cs
  Extensiones/ExtensionesMigracion.cs    (ya existe)
  Properties/launchSettings.json
  Program.cs
  appsettings.json                       (agrega seccion "Calculo")
  Dockerfile                             (ya existe, §10)
docker-compose.yml, .dockerignore, .env.example   (raiz, ya existen, §10)
```
Paquetes nuevos: `Microsoft.AspNetCore.Mvc.Testing` 8.0.11 (solo en `Abastecimiento.Pruebas`). `Abastecimiento.Aplicacion` ya referencia `Microsoft.Extensions.Options`.

---

## 4. Motor de Calculo (R3, Reglas 2 y 3) — nucleo del criterio de 40%

### 4.1. Configuracion
```csharp
public class OpcionesCalculo
{
    public const string Seccion = "Calculo";
    public MidpointRounding ModoRedondeo { get; set; } = MidpointRounding.AwayFromZero;
}
```
`appsettings.json`: `"Calculo": { "ModoRedondeo": "AwayFromZero" }`. Valores validos: los de `MidpointRounding` (ej. `ToEven`).

### 4.2. `ArbolComercialHelper` (operaciones recurrentes sobre el arbol)
Recibe los nodos tal como los entrega `IArbolComercialRepositorio.ObtenerArbolCompletoAsync()` (con `ConceptoComercial.NivelJerarquia` cargado; **sin** depender de `NodoPadre`/`NodosHijos`).
* `ArbolIndexado Indexar(IReadOnlyList<NodoComercial> nodos)` → `PorId` (diccionario), `HijosDe` (`ToLookup(n => n.NodoPadreId)`), `Raices` (hijos de `null`), `ProfundidadHoja` (max `Profundidad` de los niveles presentes).
* `IEnumerable<NodoRecorrido> RecorrerEnCascada(ArbolIndexado arbol, decimal valorRaiz)` → DFS desde las raices; para cada nodo produce `{ Nodo, Valor, Camino }` con `Valor(hijo) = Valor(padre) × PorcentajeParticipacion` y `Camino` = lista de nodos desde la raiz. Con `valorRaiz = 1` el valor es el **% acumulado**; con `valorRaiz = EspaciosTotales` son los **espacios calculados**.
* `string RutaLegible(IEnumerable<NodoComercial> camino)` → `"BELLEZA Y BIENESTAR > BEAUTY > ..."`.
* `bool EsHoja(NodoComercial nodo, int profundidadHoja)`.

### 4.3. `RedondeoHelper`
* `int RedondearCarga(decimal cargaCalculada, MidpointRounding modo)` → `(int)Math.Round(cargaCalculada, 0, modo)`. Es el **unico** punto de redondeo del sistema.

### 4.4. `IMotorCalculoCarga` / `MotorCalculoCarga`
Servicio puro (sin BD; recibe `IOptions<OpcionesCalculo>`), testeable de forma aislada.
```csharp
ResultadoCalculo Calcular(int espaciosTotales, IReadOnlyList<NodoComercial> nodos);
```
Algoritmo:
1. `arbol = ArbolComercialHelper.Indexar(nodos)`.
2. Para cada `NodoRecorrido` de `RecorrerEnCascada(arbol, espaciosTotales)` (Regla 2, sin redondeo):
   * Registrar el nodo con sus `EspaciosCalculados`.
   * Si es hoja (`Profundidad == ProfundidadHoja`): `CargaCalculada = EspaciosCalculados × UnidadesPorEspacio` y `CargaTotal = RedondeoHelper.RedondearCarga(CargaCalculada, modo)` (Regla 3).
   * Si no es hoja y no tiene hijos: agregar **advertencia** `"La rama <ruta> termina en profundidad X sin llegar al nivel hoja (Y)."`; no lanza excepcion.
3. Totales: `TotalEspaciosAsignados = Σ EspaciosCalculados de las hojas` (sin redondear); `TotalCarga = Σ CargaTotal de las lineas` (suma de lineas ya redondeadas, como la tabla del enunciado).

`ResultadoCalculo { Nodos[], Lineas[], TotalEspaciosAsignados, TotalCarga, Advertencias[] }` (modelos internos; el servicio los mapea a DTO).

### 4.5. Resultado esperado con la semilla (HANNA 5503, 22 espacios)
| Ruta (marca > perfil > tipo) | Espacios calculados | Unid./Esp. | Carga calculada | Carga total |
|---|---|---|---|---|
| LILI BEAUTY > LABIOS > LABIALES | 1.2408 | 660 | 818.928 | 819 |
| LILI BEAUTY > LABIOS > BRILLO LABIOS | 1.2408 | 540 | 670.032 | 670 |
| LILI BEAUTY > OJOS > PESTAÑINA | 1.67508 | 288 | 482.42304 | 482 |
| LILI BEAUTY > OJOS > DELINEADOR | 1.11672 | 420 | 469.0224 | 469 |
| LILI BEAUTY > ROSTRO > BASE / POLVOS | 0.9306 | 310 | 288.486 | 288 |
| KARITE > LABIOS > BÁLSAMO LABIAL | 0.396 | 600 | 237.6 | 238 |
| **Total** | **6.6** | | | **2,966** |

---

## 5. Servicios (logica de negocio)

### 5.1. `ITiendaServicio` / `ServicioTiendas` (R1)
| Metodo | Comportamiento |
|---|---|
| `Task<TiendaRespuesta> CrearAsync(CrearTiendaSolicitud s, CancellationToken ct)` | 1) `ExisteCodigoHannaAsync` → `ConflictoExcepcion` (409). 2) Verificar que existan `MarcaTiendaId`, `RegionalId`, `TipoTiendaId`, `ResponsableVisualId?` (`ICatalogoRepositorio<T>.ExisteAsync`) y los `TipoEspacioId` (`ITipoEspacioRepositorio.ObtenerPorIdsAsync`); los inexistentes se acumulan en una `ValidacionDominioExcepcion` (400). 3) Construir `Tienda` + `EspacioTienda` (con `TipoEspacio` asignado) y aplicar `ReglasEspaciosTienda.Validar` (max 3 mobiliario, sin repetidos, ≥ 0). 4) `AgregarAsync` + `GuardarCambiosAsync` (una sola transaccion). 5) Devolver `ObtenerPorCodigoHannaAsync` mapeado. |
| `Task<TiendaRespuesta> ObtenerPorCodigoHannaAsync(int codigoHanna, CancellationToken ct)` | No existe → `NoEncontradoExcepcion` (404). |
| `Task<IReadOnlyList<TiendaRespuesta>> ListarAsync(int? marcaTiendaId, int? regionalId, CancellationToken ct)` | Filtros opcionales; orden por `CodigoHanna`. |

### 5.2. `ICatalogoServicio` / `ServicioCatalogos` (R1)
Generico sobre los catalogos simples (`MarcaTienda`, `Regional`, `TipoTienda`, `ResponsableVisual`):
* `Task<IReadOnlyList<CatalogoRespuesta>> ListarAsync<T>(CancellationToken ct) where T : EntidadCatalogo`
* `Task<CatalogoRespuesta> CrearAsync<T>(CrearCatalogoSolicitud s, CancellationToken ct) where T : EntidadCatalogo, new()` — nombre recortado; duplicado (`ExisteNombreAsync`) → 409.

### 5.3. `ITipoEspacioServicio` / `ServicioTiposEspacio` (R1)
* `ListarAsync` y `CrearAsync(CrearTipoEspacioSolicitud)` — nombre duplicado → 409. Permite agregar muebles o tipos de pared nuevos sin cambiar el esquema.

### 5.4. `IArbolComercialServicio` / `ServicioArbolComercial` (R2)
| Metodo | Comportamiento |
|---|---|
| `ObtenerNivelesAsync` | Niveles ordenados por `Profundidad`, marcando `esHoja`. |
| `ObtenerArbolAsync` | Lista plana de nodos con nivel, ruta legible, `porcentajeAcumulado` (`RecorrerEnCascada(arbol, 1)`) y `esHoja`; ordenada en preorden (DFS). |
| `ListarConceptosAsync(int? nivelJerarquiaId)` | Conceptos con su nivel. |
| `CrearConceptoAsync(CrearConceptoSolicitud)` | Nivel inexistente → 400; nombre duplicado en el nivel → 409; `ValidadorArbolComercial.ValidarConcepto` (hoja ⇒ `unidadesPorEspacio` obligatorio > 0; no hoja ⇒ null) → 400. |
| `CrearNodoAsync(CrearNodoSolicitud)` | Concepto inexistente → 400; padre inexistente → 404; mismo concepto bajo el mismo padre → 409; `ValidadorArbolComercial.ValidarNodo` (profundidad = padre + 1, raiz ⇒ profundidad 1, % en (0, 1], Σ hermanos ≤ 1) → 400. |
| `ActualizarPorcentajeAsync(int nodoId, ActualizarPorcentajeSolicitud)` | Nodo inexistente → 404; revalida con `ValidarNodo` excluyendo al propio nodo de los hermanos → 400; guarda con `ObtenerNodoParaEditarAsync` + `GuardarCambiosAsync`. |

### 5.5. `ICargaServicio` / `ServicioCarga` (R3, R4a)
* `Task<CargaTiendaRespuesta> CalcularPorCodigoHannaAsync(int codigoHanna, CancellationToken ct)` — tienda inexistente → 404; lee el arbol completo (1 SELECT), llama a `IMotorCalculoCarga.Calcular(tienda.EspaciosTotales, nodos)` y mapea.

### 5.6. `IReporteServicio` / `ServicioReportes` (R4b)
* `Task<ConsolidadoRespuesta> ObtenerConsolidadoAsync(AgruparPor agruparPor, int? nivelId, CancellationToken ct)`
  * `enum AgruparPor { Tienda, MarcaTienda, Regional, Nivel }` (en la query string: `tienda`, `marcaTienda`, `regional`, `nivel`).
  * Lee **una vez** todas las tiendas y el arbol; ejecuta el motor por tienda en memoria.
  * `Tienda` / `MarcaTienda` / `Regional`: agrupa tiendas → `cantidadTiendas`, `espaciosTotales` (fisicos, Regla 1), `espaciosAsignados` (Σ hojas) y `cargaTotal` (Σ cargas de lineas).
  * `Nivel`: requiere `nivelId` existente (si falta o no existe → 400); agrupa las lineas de todas las tiendas por el ancestro de ese nivel (ej. `nivelId=3` = marca de producto: LILI BEAUTY / KARITE); `espaciosTotales` = null.
  * Incluye una fila de **total general**.

---

## 6. Endpoints (Controllers)
Todos bajo `[ApiController]`, `[Route("api/...")]`, `[Produces("application/json")]`, con `[ProducesResponseType]` por codigo y comentarios XML `<summary>` / `<param>` / `<response>`.

### 6.1. Tiendas y catalogos (R1)
| Metodo y ruta | Servicio | Respuestas |
|---|---|---|
| `POST /api/tiendas` | `ITiendaServicio.CrearAsync` | 201 + `Location: /api/tiendas/{codigoHanna}` / 400 / 409 |
| `GET /api/tiendas?marcaTiendaId=&regionalId=` | `ListarAsync` | 200 |
| `GET /api/tiendas/{codigoHanna}` | `ObtenerPorCodigoHannaAsync` | 200 / 404 |
| `GET /api/catalogos/marcas-tienda` · `POST` | `ICatalogoServicio` (`MarcaTienda`) | 200 / 201 / 400 / 409 |
| `GET /api/catalogos/regionales` · `POST` | (`Regional`) | 200 / 201 / 400 / 409 |
| `GET /api/catalogos/tipos-tienda` · `POST` | (`TipoTienda`) | 200 / 201 / 400 / 409 |
| `GET /api/catalogos/responsables-visuales` · `POST` | (`ResponsableVisual`) | 200 / 201 / 400 / 409 |
| `GET /api/tipos-espacio` · `POST` | `ITipoEspacioServicio` | 200 / 201 / 400 / 409 |

### 6.2. Arbol comercial (R2)
| Metodo y ruta | Servicio | Respuestas |
|---|---|---|
| `GET /api/niveles-jerarquia` | `ObtenerNivelesAsync` | 200 |
| `GET /api/arbol-comercial` | `ObtenerArbolAsync` | 200 |
| `GET /api/conceptos-comerciales?nivelJerarquiaId=` | `ListarConceptosAsync` | 200 |
| `POST /api/conceptos-comerciales` | `CrearConceptoAsync` | 201 / 400 / 409 |
| `POST /api/nodos-comerciales` | `CrearNodoAsync` | 201 / 400 / 404 / 409 |
| `PUT /api/nodos-comerciales/{id}/porcentaje` | `ActualizarPorcentajeAsync` | 200 / 400 / 404 |

### 6.3. Calculo y reportes (R3, R4)
| Metodo y ruta | Servicio | Respuestas |
|---|---|---|
| `GET /api/carga/{codigoHanna}` | `ICargaServicio.CalcularPorCodigoHannaAsync` | 200 / 404 |
| `GET /api/reportes/consolidado?agruparPor=tienda\|marcaTienda\|regional\|nivel&nivelId=` | `IReporteServicio.ObtenerConsolidadoAsync` | 200 / 400 |

---

## 7. Contratos (DTOs)
Records inmutables; nombres en JSON en camelCase. Decimales **completos** (sin redondear) salvo `cargaTotal` y `totalCarga` (enteros).

### 7.1. Solicitudes (con DataAnnotations)
| DTO | Campos y validaciones |
|---|---|
| `CrearTiendaSolicitud` | `codigoHanna` `[Range(1, int.MaxValue)]`; `nombre` `[Required, StringLength(150)]`; `marcaTiendaId`, `regionalId`, `tipoTiendaId` `[Range(1, …)]`; `responsableVisualId?`; `espacios` `[Required, MinLength(1)]` de `EspacioSolicitud { tipoEspacioId [Range(1,…)], cantidadEspacios [Range(0,…)] }` |
| `CrearCatalogoSolicitud` | `nombre` `[Required, StringLength(100)]`. El servicio valida ademas la longitud de cada catalogo segun el spec de datos (50; 100 en responsables visuales) → 400 |
| `CrearTipoEspacioSolicitud` | `nombre` `[Required, StringLength(50)]`; `categoria` (`Mobiliario` \| `Perimetral`) |
| `CrearConceptoSolicitud` | `nivelJerarquiaId` `[Range(1,…)]`; `nombre` `[Required, StringLength(100)]`; `unidadesPorEspacio?` `[Range(1,…)]` |
| `CrearNodoSolicitud` | `nodoPadreId?`; `conceptoComercialId` `[Range(1,…)]`; `porcentajeParticipacion` `[Range(typeof(decimal), "0.000001", "1")]` |
| `ActualizarPorcentajeSolicitud` | `porcentajeParticipacion` `[Range(typeof(decimal), "0.000001", "1")]` |

### 7.2. Respuestas
| DTO | Campos |
|---|---|
| `CatalogoRespuesta` | `id`, `nombre` |
| `TipoEspacioRespuesta` | `id`, `nombre`, `categoria` |
| `TiendaRespuesta` | `id`, `codigoHanna`, `nombre`, `marcaTienda`, `regional`, `tipoTienda` (`CatalogoRespuesta`), `responsableVisual?`, `espacios[{ tipoEspacio, categoria, cantidadEspacios }]`, `espaciosTotales` |
| `NivelRespuesta` | `id`, `nombre`, `profundidad`, `esHoja` |
| `ConceptoRespuesta` | `id`, `nombre`, `nivel` (`NivelRespuesta`), `unidadesPorEspacio?` |
| `NodoArbolRespuesta` | `nodoId`, `nodoPadreId?`, `conceptoComercialId`, `nombre`, `nivel`, `profundidad`, `porcentajeParticipacion`, `porcentajeAcumulado`, `ruta`, `unidadesPorEspacio?`, `esHoja` |
| `CargaTiendaRespuesta` | `codigoHanna`, `nombreTienda`, `espaciosTotales`, `lineas[]`, `nodos[]`, `totalEspaciosAsignados`, `totalCarga`, `advertencias[]` |
| `LineaCargaRespuesta` | `nodoId`, `tipoProducto`, `ruta[{ nivel, nombre, porcentajeParticipacion }]`, `espaciosCalculados`, `unidadesPorEspacio`, `cargaCalculada`, `cargaTotal` |
| `NodoCalculadoRespuesta` | `nodoId`, `nodoPadreId?`, `nivel`, `nombre`, `porcentajeParticipacion`, `espaciosCalculados` |
| `ConsolidadoRespuesta` | `agruparPor`, `nivel?`, `grupos[]`, `total` |
| `GrupoConsolidadoRespuesta` | `clave`, `nombre`, `cantidadTiendas`, `espaciosTotales?`, `espaciosAsignados`, `cargaTotal` |

`MapeoHelper` concentra las conversiones entidad → DTO (`ATiendaRespuesta`, `ACatalogoRespuesta`, `ANivelRespuesta`, …).

---

## 8. Manejo de Errores
`Middleware/ManejadorExcepciones.cs` implementa `IExceptionHandler` y se registra con `AddExceptionHandler` + `AddProblemDetails` + `app.UseExceptionHandler()`. Respuesta `application/problem+json` (RFC 7807):

| Excepcion | HTTP | Detalle |
|---|---|---|
| `ValidacionDominioExcepcion` | 400 | `title` "Regla de negocio violada", extension `errores[]` |
| `NoEncontradoExcepcion` | 404 | mensaje de la excepcion |
| `ConflictoExcepcion` | 409 | mensaje de la excepcion |
| `DbUpdateException` (respaldo: UQ / CHECK de la BD) | 409 | "El registro viola una restriccion de la base de datos." |
| Cualquier otra | 500 | mensaje generico; el detalle solo va al log (nunca stack trace al cliente) |

Validacion de forma: `[ApiController]` responde automaticamente 400 con `ValidationProblemDetails` cuando falla una DataAnnotation.

---

## 9. `Program.cs`, Swagger y configuracion
* `AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)))`.
* `Configure<OpcionesCalculo>(builder.Configuration.GetSection(OpcionesCalculo.Seccion))`.
* `AgregarInfraestructura(cadenaConexion)` (existente) + `AgregarAplicacion()` (nuevo).
* Swagger (`Swashbuckle.AspNetCore`, ya referenciado): titulo "API Abastecimiento Retail", descripcion con las 3 reglas de calculo, comentarios XML (`GenerateDocumentationFile` ya activo), enums como texto. **Habilitado en todos los entornos** para que el evaluador lo encuentre sin configuracion.
* `Properties/launchSettings.json`: perfil `http` en `http://localhost:5080`, `ASPNETCORE_ENVIRONMENT=Development`, `launchUrl: swagger`.
* `await app.AplicarMigracionesAsync();` se mantiene antes de `app.Run()`.

---

## 10. Contenedores (docker compose)
Un contenedor por servicio, orquestados con un unico `docker-compose.yml` en la raiz: `docker compose up -d --build`.

| Servicio | Imagen | Puerto host | Detalle |
|---|---|---|---|
| `db` (`abastecimiento-db`) | `mcr.microsoft.com/mssql/server:2022-latest` | 14333 | volumen `datos-sql`; healthcheck `sqlcmd SELECT 1` |
| `api` (`abastecimiento-api`) | `src/Abastecimiento.Api/Dockerfile` (multi-stage: `sdk:8.0` → `aspnet:8.0`, usuario `app`) | 8080 | `depends_on: db (service_healthy)`; aplica migraciones y semilla al arrancar; `ConnectionStrings__Abastecimiento` por variable de entorno |
| `front` | (futuro) | — | reservado como comentario en el compose |

* Contrasena de SQL Server en `MSSQL_SA_PASSWORD` (valor por defecto de desarrollo; `.env.example` documenta como cambiarla). `.env` no se versiona.
* `appsettings.json` (LocalDB) no cambia: en Docker la cadena de conexion llega por variable de entorno.
* El contexto de build es la raiz del repo (por `Directory.Build.props`); `.dockerignore` excluye `tests/`, `bin/`, `obj/`, docs y scripts.
* Swagger queda en `http://localhost:8080/swagger` (habilitado en todos los entornos, §9).

---

## 11. Ajustes requeridos en la capa de datos
Estos metodos se agregan a `agent_spec_capa_de_datos_net_8.md` §5.3 (marcados "requeridos por la capa de aplicacion") **antes** de implementarlos. No cambian tablas ni migraciones.
* `IArbolComercialRepositorio`: `ObtenerNivelesAsync`, `ObtenerConceptosAsync(int? nivelJerarquiaId)`, `ObtenerConceptoPorIdAsync(int id)` (con nivel), `ExisteConceptoAsync(int nivelJerarquiaId, string nombre)`, `ExisteNodoAsync(int? nodoPadreId, int conceptoComercialId)`, `ObtenerNodoParaEditarAsync(int id)` (con seguimiento y concepto/nivel).
* `ICatalogoRepositorio<T>`: `ExisteAsync(int id)`, `ExisteNombreAsync(string nombre)`.
* Nuevo `ITipoEspacioRepositorio`: `ObtenerTodosAsync`, `ObtenerPorIdsAsync(IReadOnlyCollection<int> ids)`, `ExisteNombreAsync`, `AgregarAsync`.

---

## 12. Pruebas (`tests/Abastecimiento.Pruebas`)
| Tipo | Casos |
|---|---|
| Motor (unitario, semilla en memoria) | 5503 reproduce la tabla §4.5 linea por linea y totales (6.6 / 2,966); 1001 y 2045 calculan con 30 y 20 espacios; `ModoRedondeo = ToEven` cambia solo donde hay empate; rama incompleta genera advertencia; arbol con nivel 8 calcula sin cambios de codigo; no hay redondeo intermedio (espacios con todos los decimales) |
| Helpers | `% acumulado` de LABIALES = 0.0564; ruta legible; preorden del recorrido |
| Servicios (repositorios falsos en memoria) | crear tienda valida; HANNA duplicado → `ConflictoExcepcion`; 4 muebles → `ValidacionDominioExcepcion`; catalogo o tipo de espacio inexistente → `ValidacionDominioExcepcion`; nodo que salta nivel / Σ hermanos > 1 → `ValidacionDominioExcepcion`; tienda inexistente en carga → `NoEncontradoExcepcion`; consolidado por regional y por nivel 3 |
| API (`WebApplicationFactory` + SQL Server; omitidas sin `ABASTECIMIENTO_PRUEBAS_SQL`) | `GET /api/carga/5503` → 200 con `totalCarga` 2966; `GET /api/carga/9999` → 404 `problem+json`; `POST /api/tiendas` → 201 y repetido → 409; `GET /api/reportes/consolidado?agruparPor=regional` → BOGOTA y ANTIOQUIA; `?agruparPor=nivel` sin `nivelId` → 400 |

Las pruebas de API que crean datos usan una base de datos propia (`Abastecimiento_Pruebas`) o revierten sus cambios, para no alterar la semilla.

---

## 13. Supuestos
* **A. Arbol global** (heredado): todas las tiendas usan el mismo arbol; 1001 y 2045 se calculan con el mismo arbol que 5503.
* **B. Redondeo:** `AwayFromZero` por defecto, configurable; se aplica por linea. `totalCarga` = Σ lineas redondeadas (coincide con el enunciado; para 5503 ambas convenciones dan 2,966).
* **C. Precision en la respuesta:** la API devuelve los decimales completos; el formato visual (ej. 4 decimales del enunciado) es responsabilidad del cliente.
* **D. "Marca" en R4b:** se interpreta como marca de tienda (`agruparPor=marcaTienda`); la marca de producto se obtiene con `agruparPor=nivel&nivelId=3`.
* **E. Fuera de alcance:** editar/eliminar tiendas y nodos, autenticacion y autorizacion por roles.

---

## 14. Criterios de Aceptacion (Definition of Done)
1. `dotnet build` sin warnings; `dotnet test` con todas las pruebas en verde (incluidas las de API con SQL Server).
2. Ningun controller referencia repositorios ni `DbContext`; ningun servicio devuelve entidades.
3. `GET /api/carga/5503` reproduce exactamente la tabla §4.5 (espacios, unidades, cargas y total 2,966).
4. `GET /api/reportes/consolidado` funciona para `tienda`, `marcaTienda`, `regional` y `nivel` con la semilla.
5. Errores de negocio devuelven `ProblemDetails` con el codigo de §8; nunca stack traces.
6. Swagger (`/swagger`) documenta todos los endpoints de §6 con comentarios XML y codigos de respuesta.
7. `agent_spec_capa_de_datos_net_8.md` §5.3 incluye los metodos de §11.
8. `docker compose up -d --build` deja la BD migrada con la semilla y la API respondiendo en `http://localhost:8080/swagger`; un `down`/`up` conserva los datos (volumen) sin re-aplicar migraciones.
