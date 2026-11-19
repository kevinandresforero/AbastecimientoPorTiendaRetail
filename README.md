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

## 1. Como levantar el backend en un computador limpio

Hay dos rutas. Elige **una**:

| | Ruta A — con Docker (recomendada) | Ruta B — sin Docker |
|---|---|---|
| Instalas | Git + Docker | Git + .NET 8 SDK + SQL Server |
| Comandos para levantar | 2 | 1 (despues de instalar y configurar SQL Server) |
| URL de la API / Swagger | http://localhost:8080/swagger | http://localhost:5080/swagger |
| Sistemas | Windows, macOS, Linux | Windows y Linux (macOS solo con un SQL Server remoto) |

En ambas rutas la API **crea la base de datos, aplica las migraciones de EF Core y carga la semilla** (Seccion 4 del
enunciado) al arrancar. No hay que ejecutar scripts SQL ni `dotnet ef`.

### 1.1 Paso 0 (ambas rutas): Git y el codigo
1. Instala Git: Windows `winget install Git.Git`; macOS `xcode-select --install`; Ubuntu/Debian `sudo apt install git`.
2. Clona el repositorio y entra a la carpeta:
   ```bash
   git clone <URL-del-repositorio> abastecimiento_retail
   cd abastecimiento_retail
   ```
3. Requisitos del equipo: **4 GB de RAM libres** (SQL Server pide minimo 2 GB) y los puertos **8080** (API en Docker),
   **5080** (API con `dotnet run`) y **14333** (SQL Server en Docker) sin usar.

> En Windows los comandos de esta guia van en **PowerShell**; donde la sintaxis cambia se indica la variante.

### 1.2 Ruta A — con Docker (recomendada)
**Instalar Docker** (una sola vez):

| Sistema | Como |
|---|---|
| Windows 10/11 | `wsl --install` (reiniciar) y luego `winget install Docker.DockerDesktop`. Abrir Docker Desktop y esperar a que diga *Engine running*. |
| macOS | Instalar [Docker Desktop](https://www.docker.com/products/docker-desktop/). En **Apple Silicon (M1/M2/M3)**: *Settings → General → "Use Rosetta for x86_64/amd64 emulation"*, porque la imagen de SQL Server solo existe para amd64. |
| Linux (Ubuntu) | Docker Engine + plugin compose segun [docs.docker.com/engine/install](https://docs.docker.com/engine/install/ubuntu/); luego `sudo usermod -aG docker $USER` y cerrar/abrir sesion. |

Comprobar: `docker compose version` debe mostrar la version 2.x.

**Levantar:**
```bash
cp .env.example .env            # Windows: copy .env.example .env   (contrasena de SQL Server de desarrollo)
docker compose up -d --build    # SQL Server (db) + API (api); la primera vez descarga ~2 GB de imagenes
docker compose ps               # esperar a que db diga "healthy" y api "running" (1-2 minutos la primera vez)
```
- Swagger: **http://localhost:8080/swagger**
- Detener: `docker compose down` (los datos se conservan en el volumen `datos-sql`).
- Empezar de cero (borra la base): `docker compose down -v`.
- Ver logs de la API: `docker compose logs -f api`.

### 1.3 Ruta B — sin Docker
**1. Instalar el SDK de .NET 8**

| Sistema | Como |
|---|---|
| Windows | `winget install Microsoft.DotNet.SDK.8` |
| Ubuntu 22.04+ | `sudo apt update && sudo apt install -y dotnet-sdk-8.0` |
| macOS / otras distros Linux | Script oficial: `curl -sSL https://dot.net/v1/dotnet-install.sh \| bash -s -- --channel 8.0` y agregar `~/.dotnet` al `PATH` |

Comprobar: `dotnet --list-sdks` debe listar una version `8.0.x`.
(Si solo tienes un .NET mas nuevo, p. ej. 10, antepone `DOTNET_ROLL_FORWARD=Major` a los comandos `dotnet`.)

**2. Instalar SQL Server y obtener la cadena de conexion**

| Sistema | Opcion | Cadena de conexion |
|---|---|---|
| Windows (mas simple) | **SQL Server LocalDB**: instalador de [SQL Server 2022 Express](https://www.microsoft.com/es-es/sql-server/sql-server-downloads) → *Descargar medios* → *LocalDB* (o viene con Visual Studio). | Ya es la cadena por defecto de `appsettings.json`: **no necesitas `.env`**. |
| Windows | SQL Server 2022 Express o Developer (instalacion basica). | `Server=localhost\SQLEXPRESS;Database=Abastecimiento;Trusted_Connection=True;TrustServerCertificate=True` |
| Linux (Ubuntu 22.04 / RHEL) | SQL Server 2022 para Linux segun [learn.microsoft.com](https://learn.microsoft.com/es-es/sql/linux/quickstart-install-connect-ubuntu): paquete `mssql-server` y `sudo /opt/mssql/bin/mssql-conf setup` (elige la edicion *Developer* y una contrasena para `sa`). | `Server=localhost,1433;Database=Abastecimiento;User Id=sa;Password=<tu-contrasena>;TrustServerCertificate=True` |
| macOS | SQL Server no se instala de forma nativa en macOS. Sin Docker, usa un SQL Server accesible por red (otro equipo, una VM Windows/Linux o Azure SQL). | La cadena de ese servidor. |

**3. Configurar la conexion** (no aplica a LocalDB):
```bash
cp .env.example .env            # Windows: copy .env.example .env
```
Edita `.env` y reemplaza el valor de `ConnectionStrings__Abastecimiento` por la cadena de la tabla anterior.
Con **LocalDB** no crees el `.env` (o borra esa linea): si existe, su cadena apunta a `localhost,14333` (el SQL Server
de Docker) y tiene prioridad sobre `appsettings.json`.

**4. Levantar la API**
```bash
dotnet run --project src/Abastecimiento.Api --launch-profile http
```
- Swagger: **http://localhost:5080/swagger** (con el perfil `http` el navegador se abre solo).
- La primera vez restaura paquetes NuGet (requiere internet) y crea la base `Abastecimiento` con la semilla.
- Detener: `Ctrl+C`.
- La API busca el `.env` desde la carpeta donde ejecutas el comando hacia arriba: ejecutalo desde la raiz del repositorio.
- Opcional (la API ya lo hace sola): `dotnet tool restore && dotnet ef database update --project src/Abastecimiento.Infraestructura --startup-project src/Abastecimiento.Api`.

**5. Pruebas sin Docker**
```bash
dotnet test                     # unitarias (102); las de base de datos aparecen como omitidas
```
Todas, incluidas integracion y API, contra tu SQL Server (base aparte `Abastecimiento_Pruebas`):
```bash
# bash
ABASTECIMIENTO_PRUEBAS_SQL="<tu cadena con Database=Abastecimiento_Pruebas>" dotnet test
```
```powershell
# PowerShell
$env:ABASTECIMIENTO_PRUEBAS_SQL = "<tu cadena con Database=Abastecimiento_Pruebas>"; dotnet test
```

### 1.4 Comprobar que funciona (ambas rutas)
```bash
curl -s http://localhost:8080/api/carga/5503          # Ruta B: puerto 5080
```
```powershell
(Invoke-RestMethod http://localhost:8080/api/carga/5503).totalCarga     # Ruta B: puerto 5080
```
Debe devolver `totalCarga` **2966** (6 lineas: 819, 670, 482, 469, 288, 238), igual que la tabla del enunciado.

### 1.5 Problemas comunes
| Sintoma | Causa y solucion |
|---|---|
| `port is already allocated` (14333 u 8080) | Otro programa usa el puerto. SQL: `SQL_PUERTO=14334 docker compose up -d` (PowerShell: `$env:SQL_PUERTO=14334; docker compose up -d`) y ajusta el puerto en `.env` si usas `dotnet run`. API: libera el 8080. |
| `db` no pasa a *healthy* / se reinicia | Poca memoria (SQL Server necesita 2 GB; en Docker Desktop: *Settings → Resources*) o contrasena debil: `MSSQL_SA_PASSWORD` necesita 8+ caracteres con mayusculas, minusculas, numeros y simbolos. Revisa `docker compose logs db`. |
| La API reinicia en Docker al primer arranque | Espera: arranca en cuanto `db` esta *healthy* (`restart: on-failure`). |
| `A network-related or instance-specific error` / `Login failed` | Cadena de conexion incorrecta o SQL Server apagado: revisa `ConnectionStrings__Abastecimiento` en `.env` (servidor, puerto, usuario, contrasena). |
| `The certificate chain was issued by an authority that is not trusted` | Falta `TrustServerCertificate=True` en la cadena. |
| `You must install or update .NET to run this application` / framework `8.0` no encontrado | Instala el SDK o runtime de .NET 8, o antepone `DOTNET_ROLL_FORWARD=Major` si solo tienes una version mas nueva. |
| `LocalDB is not supported on this platform` | LocalDB solo existe en Windows: en Linux/macOS usa otra opcion de la tabla del paso 2 (o la Ruta A). |
| Apple Silicon: `no matching manifest for linux/arm64` o SQL Server se cae | Activa Rosetta en Docker Desktop (ver tabla de instalacion de Docker). |
| El front en el navegador recibe error de CORS | Agrega su origen: `Cors__OrigenesPermitidos__3=http://localhost:<puerto>` (ver "Consumir desde un front"). |

### 1.6 Pruebas — un solo comando (requiere Docker)
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

### 1.7 Probar a mano
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
