# Abastecimiento Retail — Calculo de Carga por Tienda

API REST en **.NET 8** que calcula cuantas unidades de cada tipo de producto se deben enviar a una tienda, distribuyendo
en cascada sus espacios fisicos por un arbol comercial de 7 niveles (Lili Pink, Yoi, Yahad).

| Regla | Formula | Donde |
|---|---|---|
| 1. Capacidad fisica total | `Espacios_Totales = Mob_1 + Mob_2 + Mob_3 + Perimetrales` | `Dominio/Entidades/Tiendas/Tienda.cs` (`EspaciosTotales`) |
| 2. Distribucion en cascada | `Espacios_Nivel(n) = Espacios_Nivel(n-1) × %_Participacion(n)` | `Aplicacion/Helpers/ArbolComercialHelper.cs` (`RecorrerEnCascada`) |
| 3. Carga total | `Carga_Total = Espacios_Tipo_Producto × Unidades_Por_Espacio` | `Aplicacion/Motor/MotorCalculoCarga.cs` |

Resultado de referencia (HANNA **5503**, 22 espacios): 819 / 670 / 482 / 469 / 288 / 238 → **6.6 espacios, 2,966 unidades**,
identico a la tabla del enunciado.

---

## 1. Como compilar, migrar y probar

### Requisitos
- Docker (opcion A) **o** .NET 8 SDK + SQL Server (opcion B).

### Opcion A — todo en Docker (recomendada)
```bash
cp .env.example .env            # contrasena de SQL Server para desarrollo (opcional cambiarla)
docker compose up -d --build    # levanta SQL Server (db) y la API (api)
```
- Swagger: **http://localhost:8080/swagger**
- Al arrancar, la API aplica las migraciones de EF Core y la semilla (Seccion 4 del enunciado). No hay pasos manuales.
- `docker compose down` / `up` conserva los datos (volumen `datos-sql`).

### Opcion B — `dotnet run` contra un SQL Server existente
```bash
cp .env.example .env            # ConnectionStrings__Abastecimiento apunta a localhost,14333
docker compose up -d db         # o usar cualquier SQL Server y ajustar la cadena en .env
dotnet run --project src/Abastecimiento.Api --launch-profile http
```
- Swagger: **http://localhost:5080/swagger**
- La API lee el `.env` de la raiz al arrancar (sin pisar variables ya definidas). Sin `.env` usa la cadena LocalDB de
  `appsettings.json`.
- Si solo tienes un runtime mas nuevo (ej. .NET 10), antepone `DOTNET_ROLL_FORWARD=Major`.
- Migraciones manuales (opcional; la API ya las aplica): `dotnet tool restore && dotnet ef database update --project src/Abastecimiento.Infraestructura --startup-project src/Abastecimiento.Api`.

### Pruebas — un solo comando
```bash
docker compose --profile pruebas run --rm pruebas
```
Ejecuta **todas** las pruebas (116: unitarias, integracion con SQL Server y API) con el SDK de .NET 8, sin instalar nada
mas que Docker:
1. Levanta el servicio `db` (SQL Server) si no esta corriendo y espera a que este sano.
2. Copia el codigo a un contenedor `mcr.microsoft.com/dotnet/sdk:8.0` (sin `bin/`, `obj/`, `.git` ni `.env`).
3. Corre `dotnet test` contra la base `Abastecimiento_Pruebas` (separada de `Abastecimiento`: la semilla no se altera).
4. Termina con `Test Run Successful` y codigo de salida 0, o con el detalle de cada prueba fallida y codigo distinto de 0.

Si el puerto 14333 ya esta ocupado por otro SQL Server: `SQL_PUERTO=14334 docker compose --profile pruebas run --rm pruebas`.

Alternativas con el SDK instalado:
```bash
dotnet test        # solo unitarias (102); las de integracion aparecen como omitidas
ABASTECIMIENTO_PRUEBAS_SQL="Server=localhost,14333;Database=Abastecimiento_Pruebas;User Id=sa;Password=Abastecimiento#2026;TrustServerCertificate=True" dotnet test   # todas
```
Las pruebas de API usan `WebApplicationFactory` de ASP.NET Core 8: requieren el runtime de .NET 8 (el comando de Docker
ya lo trae).

| Tipo | Archivo (`tests/Abastecimiento.Pruebas/`) | Que verifica |
|---|---|---|
| Unitaria | `Aplicacion/MotorCalculoCargaPruebas.cs` | Tabla del enunciado para 5503 linea por linea (819 … 238), 6.6 espacios y 2,966 unidades; reparto de BEAUTY / LILI BEAUTY / KARITE; 1001 y 2045; reglas de redondeo; rama incompleta; arbol de 8 niveles; sin redondeo intermedio |
| Unitaria | `Aplicacion/HelpersPruebas.cs`, `MapeoHelperPruebas.cs` | % acumulado (LABIALES 0.0564), ruta legible, preorden, redondeo, normalizacion de decimales, mapeo a DTO |
| Unitaria | `Aplicacion/ServiciosPruebas.cs`, `ServicioArbolComercialPruebas.cs`, `ServicioCatalogosPruebas.cs`, `ServicioTiposEspacioPruebas.cs` | Reglas de negocio de cada servicio con repositorios en memoria: duplicados → 409, catalogos inexistentes y 4 muebles → 400, saltos de nivel y suma de hermanos > 100% → 400, listados con filtros, consolidados |
| Unitaria | `Dominio/*.cs` | Regla 1 (espacios totales) y validador del arbol |
| Unitaria | `Api/ManejadorExcepcionesPruebas.cs`, `Api/CargadorArchivoEnvPruebas.cs` | Excepciones → ProblemDetails (400/404/409/500 sin filtrar detalles); lectura del `.env` |
| Unitaria | `Infraestructura/ModeloEfPruebas.cs` | Modelo EF sin conectarse: esquemas, precision `decimal(18,6)`, restricciones e indices unicos, semilla |
| Integracion (SQL Server) | `Infraestructura/RepositoriosSqlServerPruebas.cs` | El arbol se lee en una sola SELECT; tienda con catalogos y espacios; filtro por regional |
| API (SQL Server) | `Api/ApiPruebas.cs` | `GET /api/carga/5503` → 2,966; 404/400/409 en `problem+json`; crear tienda 201 + `Location`; consolidados; niveles, arbol, tiendas; Swagger |

### Probar a mano
```bash
curl -s http://localhost:8080/api/carga/5503                                   # desglose 5503 → totalCarga 2966
curl -s "http://localhost:8080/api/reportes/consolidado?agruparPor=regional"   # BOGOTA / ANTIOQUIA
curl -s "http://localhost:8080/api/reportes/consolidado?agruparPor=nivel&nivelId=3"  # por marca de producto
curl -si http://localhost:8080/api/carga/9999                                   # 404 application/problem+json
```
(Con la opcion B el puerto es 5080.)

---

## 2. Endpoints

| Metodo y ruta | Controller (`src/Abastecimiento.Api/Controllers/`) | Respuestas |
|---|---|---|
| POST `/api/tiendas` | `TiendasController.cs:19` | 201 / 400 / 409 |
| GET `/api/tiendas?marcaTiendaId=&regionalId=` | `TiendasController.cs:34` | 200 |
| GET `/api/tiendas/{codigoHanna}` | `TiendasController.cs:45` | 200 / 404 |
| GET·POST `/api/catalogos/marcas-tienda` | `CatalogosController.cs:17` / `:28` | 200 / 201, 400, 409 |
| GET·POST `/api/catalogos/regionales` | `CatalogosController.cs:38` / `:49` | 200 / 201, 400, 409 |
| GET·POST `/api/catalogos/tipos-tienda` | `CatalogosController.cs:59` / `:70` | 200 / 201, 400, 409 |
| GET·POST `/api/catalogos/responsables-visuales` | `CatalogosController.cs:80` / `:91` | 200 / 201, 400, 409 |
| GET·POST `/api/tipos-espacio` | `TiposEspacioController.cs:16` / `:27` | 200 / 201, 400, 409 |
| GET `/api/niveles-jerarquia` | `NivelesJerarquiaController.cs:16` | 200 |
| GET `/api/arbol-comercial` | `ArbolComercialController.cs:16` | 200 |
| GET `/api/conceptos-comerciales?nivelJerarquiaId=` | `ConceptosComercialesController.cs:17` | 200 |
| POST `/api/conceptos-comerciales` | `ConceptosComercialesController.cs:28` | 201 / 400 / 409 |
| POST `/api/nodos-comerciales` | `NodosComercialesController.cs:20` | 201 / 400 / 404 / 409 |
| PUT `/api/nodos-comerciales/{id}/porcentaje` | `NodosComercialesController.cs:35` | 200 / 400 / 404 |
| **GET `/api/carga/{codigoHanna}`** (R3, R4a) | `CargaController.cs:21` | 200 / 404 |
| **GET `/api/reportes/consolidado?agruparPor=tienda\|marcaTienda\|regional\|nivel&nivelId=`** (R4b) | `ReportesController.cs:19` | 200 / 400 |

Todos los endpoints estan documentados en Swagger con comentarios XML y codigos de respuesta.

### Consumir desde un front
- **URL base:** `http://localhost:8080` (Docker) o `http://localhost:5080` (`dotnet run`). Contrato OpenAPI en
  `/swagger/v1/swagger.json` (sirve para generar clientes tipados, ej. `openapi-typescript`).
- **CORS:** permitidos `http://localhost:3000` (React/Next), `http://localhost:5173` (Vite) y `http://localhost:4200`
  (Angular), en `appsettings.json` → `Cors:OrigenesPermitidos`. Otro origen sin tocar codigo:
  `Cors__OrigenesPermitidos__3=http://localhost:8081` (en `.env` o en el `environment` del servicio `api`).
- **Formato:** JSON en camelCase; enums como texto en camelCase (`"mobiliario"`, `agruparPor=marcaTienda`);
  decimales completos (formatearlos en el front, ej. 4 decimales); `cargaTotal`/`totalCarga` son enteros.
- **Errores:** `application/problem+json` con `status`, `title` y `detail`; las reglas de negocio agregan `errores[]` y
  la validacion de campos agrega `errors{campo: [mensajes]}`. Codigos: 400 datos invalidos, 404 no existe, 409 duplicado.
- **Creacion:** `POST` responde 201 con el recurso creado; `POST /api/tiendas` ademas trae la cabecera `Location`
  (expuesta por CORS).
- **Pantallas tipicas → endpoints:**

| Pantalla | Endpoints |
|---|---|
| Listado de tiendas (filtros marca/regional) | `GET /api/tiendas?marcaTiendaId=&regionalId=`, `GET /api/catalogos/marcas-tienda`, `GET /api/catalogos/regionales` |
| Crear tienda | `GET /api/catalogos/*`, `GET /api/tipos-espacio`, `POST /api/tiendas` |
| Detalle / carga de una tienda (R3, R4a) | `GET /api/tiendas/{codigoHanna}`, `GET /api/carga/{codigoHanna}` (`lineas` para la tabla, `nodos` para el arbol con espacios) |
| Arbol comercial (R2) | `GET /api/niveles-jerarquia`, `GET /api/arbol-comercial`, `GET/POST /api/conceptos-comerciales`, `POST /api/nodos-comerciales`, `PUT /api/nodos-comerciales/{id}/porcentaje` |
| Reportes (R4b) | `GET /api/reportes/consolidado?agruparPor=tienda\|marcaTienda\|regional` y `?agruparPor=nivel&nivelId=3` |

---

## 3. Decisiones tecnicas

### Arquitectura (Clean Architecture)
```
Api  ──►  Aplicacion  ──►  Dominio  ◄──  Infraestructura
```
| Proyecto | Responsabilidad |
|---|---|
| `Abastecimiento.Dominio` | Entidades, reglas de negocio puras (`ReglasEspaciosTienda`, `ValidadorArbolComercial`), excepciones e interfaces de repositorio. Sin dependencias. |
| `Abastecimiento.Aplicacion` | Motor de calculo (puro, sin BD), servicios (devuelven solo DTOs), helpers estaticos y DTOs. Depende solo de Dominio. |
| `Abastecimiento.Infraestructura` | EF Core + SQL Server: `DbContext`, configuraciones, migraciones, semilla y repositorios. |
| `Abastecimiento.Api` | Controllers delgados (una llamada a un servicio), Swagger y manejador global de excepciones. |

- **SOLID:** cada servicio tiene su interfaz y una sola responsabilidad; el motor se inyecta (`IMotorCalculoCarga`) y
  se prueba aislado; los repositorios se abstraen en Dominio (inversion de dependencias).
- **Errores:** los servicios lanzan excepciones de dominio y `Api/Middleware/ManejadorExcepciones.cs` las traduce a
  `application/problem+json` (RFC 7807): validacion → 400, no encontrado → 404, conflicto → 409, restriccion de BD → 409,
  inesperado → 500 sin stack trace.
- Sin librerias extra (ni MediatR, ni AutoMapper, ni FluentValidation): DataAnnotations para la forma y reglas de dominio
  para el negocio.

### Diseno relacional de los 7 niveles (dinamico y normalizado)
| Tabla | Rol |
|---|---|
| `NivelesJerarquia` | Los niveles (Unidad de Negocio … Tipo de Producto) con su `Profundidad`. El nivel hoja **se deriva** (mayor profundidad); no hay un "7" en el codigo. |
| `ConceptosComerciales` | *Que es* cada elemento (GIRLS, LABIALES…), definido una vez por nivel. `UnidadesPorEspacio` solo en el nivel hoja. |
| `NodosComerciales` | *Donde esta* cada concepto y con que % respecto a su padre: lista de adyacencia (`NodoPadreId`). GIRLS bajo LILI BEAUTY y bajo KARITE son 2 nodos que comparten 1 concepto. |

- Agregar tiendas, categorias, marcas o tipos de producto es insertar filas; agregar un nivel nuevo no requiere cambiar
  codigo (hay una prueba con un arbol de 8 niveles que calcula sin cambios).
- El arbol completo se lee en **una sola consulta** y se recorre en memoria (preorden).
- Tiendas: `Tiendas` + catalogos (`MarcasTienda`, `Regionales`, `TiposTienda`, `ResponsablesVisuales`) +
  `TiposEspacio` (mobiliario o perimetral) + `EspaciosTienda` (cantidad por tipo). Maximo 3 tipos de mobiliario por tienda.

---

## 4. Supuestos y regla de redondeo

### Redondeo
- **Unico punto de redondeo:** la Carga Total de cada linea (`Aplicacion/Helpers/RedondeoHelper.cs`). Los espacios de la
  cascada y la carga calculada conservan todos sus decimales (nota tecnica del enunciado).
- **Regla por defecto: entero mas cercano, empates hacia arriba** (`MidpointRounding.AwayFromZero`). Es configurable en
  `appsettings.json` → `"Calculo": { "ModoRedondeo": "AwayFromZero" }` o con la variable `Calculo__ModoRedondeo`:
  `ToEven` (bancario) o `ToZero` (entero inferior).
- **Total de la tienda** = suma de las lineas ya redondeadas (como la tabla del enunciado).
- **Capacidad:** redondear cada linea al mas cercano puede quedar hasta media unidad por linea por encima de la carga exacta.
  Con la semilla, 5503 (2,966 sobre 2,966.49 exactas) y 2045 (2,696 sobre 2,696.81) no superan; 1001 asigna 4,046 sobre
  4,045.2156 exactas. El enunciado no exige un tope; si el negocio no admite exceso, usar `ToZero` (nunca supera; 5503 → 2,964).

### Precision decimal
| Donde | Tipo | Decimales | Por que |
|---|---|---|---|
| Calculo y respuestas de la API | `decimal` (C#) | **hasta 28** | Base 10: representa 0.3 o 0.94 exactos, sin el error binario de `float`/`double`. Tiene 28-29 digitos significativos y escala maxima 28. Con porcentajes de 6 decimales la escala crece 6 por nivel (6, 12, 18, 24) y desde el nivel 5 queda en 28. El error maximo posible (< 1e-28 espacios) nunca cambia una unidad. |
| `NodosComerciales.PorcentajeParticipacion` (BD) | `decimal(18,6)` | **6** | El porcentaje se guarda como fraccion (0.300000 = 30%): granularidad de 0.0001%, de sobra para porcentajes de negocio (el enunciado usa enteros: 94%, 6%). Un valor con mas de 6 decimales se redondea al guardarse. |
| Espacios de la tienda, unidades por espacio | `int` | 0 | Son conteos. |

Los espacios y cargas calculados no se guardan en la BD: se calculan en cada consulta. La API devuelve los decimales
significativos completos (ej. 1.67508, sin ceros sobrantes); mostrarlos con 4 decimales, como el enunciado, es tarea del cliente.

### Otros supuestos
- **Arbol global:** todas las tiendas usan el mismo arbol comercial (1001 y 2045 se calculan con el arbol de 5503).
- **"Marca" en el consolidado (R4b):** se interpreta como marca de tienda (`agruparPor=marcaTienda`); la marca de
  producto se consulta con `agruparPor=nivel&nivelId=3` (LILI BEAUTY / KARITE).
- **Suma de porcentajes de hermanos ≤ 1** (no = 1): la semilla es parcial (BEAUTY = 30% sin categorias hermanas).
- **Ramas incompletas:** si una rama no llega al nivel hoja se devuelve una advertencia, no un error.
- **Fuera de alcance:** editar/eliminar tiendas y nodos, autenticacion y autorizacion (los roles existen solo como datos).
