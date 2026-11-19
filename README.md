# Abastecimiento Retail — Calculo de Carga por Tienda

API REST en **.NET 8** que calcula cuantas unidades de cada tipo de producto se deben enviar a una tienda, distribuyendo
en cascada sus espacios fisicos por un arbol comercial de 7 niveles (Lili Pink, Yoi, Yahad). Incluye un **frontend
Blazor .NET 8** que consume la API (ver [Seccion 5](#5-frontend)); `docker compose up -d --build` levanta los tres
contenedores: base de datos, API y front.

| Regla | Formula | Donde |
|---|---|---|
| 1. Capacidad fisica total | `Espacios_Totales = Mob_1 + Mob_2 + Mob_3 + Perimetrales` | `Dominio/Entidades/Tiendas/Tienda.cs` (`EspaciosTotales`) |
| 2. Distribucion en cascada | `Espacios_Nivel(n) = Espacios_Nivel(n-1) × %_Participacion(n)` | `Aplicacion/Helpers/ArbolComercialHelper.cs` (`RecorrerEnCascada`) |
| 3. Carga total | `Carga_Total = Espacios_Tipo_Producto × Unidades_Por_Espacio` | `Aplicacion/Motor/MotorCalculoCarga.cs` |

Resultado de referencia (HANNA **5503**, 22 espacios): 819 / 670 / 482 / 469 / 288 / 238 → **6.6 espacios, 2,966 unidades**,
identico a la tabla del enunciado.

> **Regla de redondeo (resumen):** solo se redondea la carga total de cada linea, al final; los espacios de la cascada
> conservan todos sus decimales. Regla por defecto: entero mas cercano con empates hacia arriba
> (`MidpointRounding.AwayFromZero`), configurable con `Calculo__ModoRedondeo` (`ToEven` o `ToZero`). Ver la
> [seccion 4](#4-supuestos-y-regla-de-redondeo) para el detalle y los numeros exactos.

---

## 1. Como levantar el proyecto en un computador limpio

Hay dos rutas. Elige **una**:

| | Ruta A — con Docker (recomendada) | Ruta B — sin Docker |
|---|---|---|
| Instalas | Git + Docker | Git + .NET 8 SDK + SQL Server |
| Comandos para levantar | 3 (instalar Docker, copiar `.env`, `docker compose up -d --build`) | 1 (despues de instalar y configurar SQL Server) |
| URL de la API / Swagger | http://localhost:8080/swagger | http://localhost:5080/swagger |
| URL del front | http://localhost:3000 | http://localhost:5090 |
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
   **5080** (API con `dotnet run`), **14333** (SQL Server en Docker), **3000** (front en Docker) y **5090** (front con
   `dotnet run`) sin usar.

> En Windows los comandos de esta guia van en **PowerShell**; donde la sintaxis cambia se indica la variante.

### 1.2 Ruta A — con Docker (recomendada)

#### Paso A.1: Instalar Docker (una sola vez)

| Sistema | Como |
|---|---|
| Windows 10/11 | Instalar **WSL 2**: `wsl --install` (reiniciar). Despues `winget install Docker.DockerDesktop`, abrir Docker Desktop y esperar a que diga *Engine running*. |
| macOS | Instalar [Docker Desktop](https://www.docker.com/products/docker-desktop/). En **Apple Silicon (M1/M2/M3)**: *Settings → General → "Use Rosetta for x86_64/amd64 emulation"*, porque la imagen de SQL Server solo existe para amd64. |
| Linux (Ubuntu) | Docker Engine + plugin compose segun [docs.docker.com/engine/install](https://docs.docker.com/engine/install/ubuntu/); luego `sudo usermod -aG docker $USER` y cerrar/abrir sesion. |

Comprobar la instalacion:

```bash
docker --version          # p. ej. Docker version 27.x
docker compose version    # debe mostrar la version 2.x
```

#### Paso A.2: Copiar la plantilla de entorno

```bash
cp .env.example .env            # Windows: copy .env.example .env
```

El `.env` solo define la contrasena de `sa` de SQL Server (`MSSQL_SA_PASSWORD`) y los puertos; los valores por defecto
funcionan para desarrollo. **No se sube a Git** (esta en `.gitignore`).

#### Paso A.3: Levantar los tres contenedores

```bash
docker compose up -d --build
```

Este unico comando:
1. Descarga las imagenes (~2 GB la primera vez: SQL Server, SDK .NET 8 para compilar y runtime para la API y el front).
2. Arranca **db** (SQL Server) y espera a que este *healthy*.
3. Compila y arranca **api** (`http://localhost:8080`) y **front** (`http://localhost:3000`).

Espera 1-2 minutos la primera vez. Comprueba el estado:

```bash
docker compose ps            # db debe decir "healthy" y api/front "running"
docker compose logs -f api   # la API se reinicia sola hasta que db este lista (restart: on-failure)
```

- Front: **http://localhost:3000**
- Swagger: **http://localhost:8080/swagger**
- Detener: `docker compose down` (los datos se conservan en el volumen `datos-sql`).
- Empezar de cero (borra la base): `docker compose down -v`.

#### Migraciones y semilla: **automatico**

`docker compose up` **no requiere ejecutar migraciones a mano**. La API, en `Program.cs` (ver
`Api/Extensiones/ExtensionesMigracion.cs`), al arrancar:

1. Aplica las migraciones pendientes de EF Core con `Database.MigrateAsync()` (`MigracionInicial` y `ArbolPorTienda`).
2. La semilla esta incluida en esas migraciones (EF Core `HasData` → `InsertData`): catalogos, niveles, conceptos,
   las 3 tiendas y sus 54 nodos. Por eso la base queda poblada al primer arranque.
3. Por eso **no hay que correr `dotnet ef database update` ni scripts SQL**: con levantar el compose queda todo listo.

> Si necesitas recrear la base desde cero (p. ej. tras tocar la semilla o una migracion): `docker compose down -v` y
> `docker compose up -d --build`. El volumen `datos-sql` guarda la base entre `down` y `up`.

#### Compilar sin Docker (alternativa a A.3)

Si solo quieres compilar el backend localmente, sin contenedores:

```bash
dotnet build Abastecimiento.sln
dotnet run --project src/Abastecimiento.Api --launch-profile http   # Swagger en http://localhost:5080/swagger
```

Aqui **si** debes tener un SQL Server accesible (ver Ruta B) porque la API usa la cadena de `appsettings.json`/`.env`;
la migracion y la semilla tambien se aplican automaticamente al arrancar la API.

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
dotnet test                     # unitarias (125); las 26 de base de datos aparecen como omitidas
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
Ejecuta **todas** las pruebas (268: 151 del backend —125 unitarias y 26 de integracion/API— y 117 del front) con el SDK de .NET 8, sin instalar nada
mas que Docker:
1. Levanta el servicio `db` (SQL Server) si no esta corriendo y espera a que este sano.
2. Copia el codigo a un contenedor `mcr.microsoft.com/dotnet/sdk:8.0` (sin `bin/`, `obj/`, `.git` ni `.env`).
3. Corre `dotnet test` contra la base `Abastecimiento_Pruebas` (separada de `Abastecimiento`: la semilla no se altera).
   Despues corre las pruebas del front (`dotnet test front/Abastecimiento.Front.sln`, sin API ni BD; ver Seccion 5).
4. Termina con `Test Run Successful` y codigo de salida 0, o con el detalle de cada prueba fallida y codigo distinto de 0.

Si el puerto 14333 ya esta ocupado por otro SQL Server: `SQL_PUERTO=14334 docker compose --profile pruebas run --rm pruebas`.

Alternativas con el SDK instalado:
```bash
dotnet test        # solo unitarias (125); las 26 de integracion aparecen como omitidas
ABASTECIMIENTO_PRUEBAS_SQL="Server=localhost,14333;Database=Abastecimiento_Pruebas;User Id=sa;Password=Abastecimiento#2026;TrustServerCertificate=True" dotnet test   # todas
```
Las pruebas de API usan `WebApplicationFactory` de ASP.NET Core 8: requieren el runtime de .NET 8 (el comando de Docker
ya lo trae).

| Tipo | Archivo (`tests/Abastecimiento.Pruebas/`) | Que verifica |
|---|---|---|
| Unitaria | `Aplicacion/MotorCalculoCargaPruebas.cs` | Tabla del enunciado para 5503 linea por linea (819 … 238), 6.6 espacios y 2,966 unidades; reparto de BEAUTY / LILI BEAUTY / KARITE; 1001 y 2045; reglas de redondeo; rama incompleta; arbol de 8 niveles; sin redondeo intermedio |
| Unitaria | `Aplicacion/HelpersPruebas.cs`, `MapeoHelperPruebas.cs` | % acumulado (LABIALES 0.0564), ruta legible, preorden, redondeo, normalizacion de decimales, mapeo a DTO |
| Unitaria | `Aplicacion/ServiciosPruebas.cs`, `ServicioArbolComercialPruebas.cs`, `ServicioCatalogosPruebas.cs`, `ServicioTiposEspacioPruebas.cs` | Reglas de negocio de cada servicio con repositorios en memoria: duplicados → 409, catalogos inexistentes y 4 muebles → 400, saltos de nivel y suma de hermanos > 100% → 400, arboles aislados por tienda, tienda nueva vacia, copia de arbol, listados con filtros, consolidados |
| Unitaria | `Dominio/*.cs` | Regla 1 (espacios totales) y validador del arbol |
| Unitaria | `Api/ManejadorExcepcionesPruebas.cs`, `Api/CargadorArchivoEnvPruebas.cs` | Excepciones → ProblemDetails (400/404/409/500 sin filtrar detalles); lectura del `.env` |
| Unitaria | `Infraestructura/ModeloEfPruebas.cs` | Modelo EF sin conectarse: esquemas, precision `decimal(18,6)`, restricciones e indices unicos, semilla |
| Integracion (SQL Server) | `Infraestructura/RepositoriosSqlServerPruebas.cs` | El arbol se lee en una sola SELECT; tienda con catalogos y espacios; filtro por regional |
| API (SQL Server) | `Api/ApiPruebas.cs` | `GET /api/carga/5503` → 2,966; 404/400/409 en `problem+json`; crear tienda 201 + `Location`; consolidados; niveles, arbol por tienda y copia entre tiendas; Swagger |

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
| GET `/api/tiendas/{codigoHanna}/arbol-comercial` | `ArbolComercialController.cs:21` | 200 / 404 |
| POST `/api/tiendas/{codigoHanna}/arbol-comercial/copiar-desde/{codigoHannaOrigen}` | `ArbolComercialController.cs:38` | 201 / 400 / 404 / 409 |
| GET `/api/conceptos-comerciales?nivelJerarquiaId=` | `ConceptosComercialesController.cs:17` | 200 |
| POST `/api/conceptos-comerciales` | `ConceptosComercialesController.cs:28` | 201 / 400 / 409 |
| POST `/api/tiendas/{codigoHanna}/nodos-comerciales` | `NodosComercialesPorTiendaController.cs:21` | 201 / 400 / 404 / 409 |
| PUT `/api/nodos-comerciales/{id}/porcentaje` | `NodosComercialesController.cs:20` | 200 / 400 / 404 |
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
| Arbol comercial (R2) | `GET /api/tiendas/{codigoHanna}`, `GET /api/niveles-jerarquia`, `GET /api/tiendas/{codigoHanna}/arbol-comercial`, `GET/POST /api/conceptos-comerciales`, `POST /api/tiendas/{codigoHanna}/nodos-comerciales`, `PUT /api/nodos-comerciales/{id}/porcentaje`, `POST /api/tiendas/{destino}/arbol-comercial/copiar-desde/{origen}` |
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
| `NodosComerciales` | *Donde esta* cada concepto, en que tienda (`TiendaId`) y con que % respecto a su padre: lista de adyacencia (`NodoPadreId`). GIRLS bajo LILI BEAUTY y bajo KARITE son 2 nodos que comparten 1 concepto. |

- Agregar tiendas, categorias, marcas o tipos de producto es insertar filas; agregar un nivel nuevo no requiere cambiar
  codigo (hay una prueba con un arbol de 8 niveles que calcula sin cambios).
- **Arboles independientes:** cada tienda tiene su propio conjunto de nodos; los conceptos (el "que") se comparten, pero
  los nodos y sus porcentajes (el "donde" y "cuanto") son de la tienda. Crear un nodo exige que el padre sea de la misma
  tienda y la copia entre tiendas siempre crea nodos nuevos.
- El arbol de una tienda se lee en **una sola consulta** y se recorre en memoria (preorden).
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

### Supuestos aplicados en esta implementacion (arbol por tienda)
- **Tienda nueva = arbol vacio.** `POST /api/tiendas` nunca crea nodos; el alta avisa y el detalle enlaza a
  `/arbol?tienda={hanna}`.
- **Copia = nodos nuevos e independientes.** `copiar-desde` clona la estructura y los porcentajes con IDs nuevos; editar
  despues un nodo de la copia no toca al origen. Solo se permite si el destino no tiene nodos (409 si ya tiene) y si el
  origen tiene arbol valido (400 si esta vacio).
- **Padre del mismo arbol.** Al crear un nodo, el `nodoPadreId` debe pertenecer a la tienda indicada en la ruta; si no,
  400. La raiz (sin padre) debe ser de profundidad 1.
- **Profundidad hoja derivada.** El nivel hoja es el de mayor `Profundidad` en `NivelesJerarquia` (no un "7" fijo); se
  pasa explicitamente al helper y al motor (`ArbolComercialHelper.Indexar(nodos, profundidadHoja)`).
- **Porcentaje como fraccion.** El usuario escribe 30 y se guarda `0.300000`; limite `decimal(18,6)` (6 decimales).
- **Suma de hermanos ≤ 1** (no = 1) y cada porcentaje en (0, 1]; lo valida `ValidadorArbolComercial`.
- **Ramas incompletas:** no lanzan error; el resultado trae `advertencias[]`.
- **Consultas secuenciales.** Los repositorios se ejecutan de a una porque comparten el mismo `DbContext` (evita
  "multiple active result sets").
- **Integridad referencial:** `NodosComerciales.TiendaId` y `NodoPadreId` son `Restrict`; para borrar una tienda de prueba
  hay que quitar antes sus nodos (lo hacen las pruebas de API).

### Otros supuestos
- **Arbol por tienda:** cada tienda tiene su propio arbol (`NodosComerciales.TiendaId`); los nodos son independientes y
  editar una tienda no modifica otra. La semilla crea el mismo arbol para 5503, 1001 y 2045, pero cada uno con sus propios
  nodos. Una tienda nueva nace **sin arbol**; para reutilizar una estructura existe
  `POST /api/tiendas/{destino}/arbol-comercial/copiar-desde/{origen}`, que clona los nodos (mismos conceptos y porcentajes,
  ids nuevos) y solo se permite si el destino esta vacio.
- **"Marca" en el consolidado (R4b):** se interpreta como marca de tienda (`agruparPor=marcaTienda`); la marca de
  producto se consulta con `agruparPor=nivel&nivelId=3` (LILI BEAUTY / KARITE).
- **Suma de porcentajes de hermanos ≤ 1** (no = 1): la semilla es parcial (BEAUTY = 30% sin categorias hermanas).
- **Ramas incompletas:** si una rama no llega al nivel hoja se devuelve una advertencia, no un error.
- **Fuera de alcance:** editar/eliminar tiendas y nodos, autenticacion y autorizacion (los roles existen solo como datos).

---

## 5. Frontend

Blazor Web App **.NET 8** (`front/Abastecimiento.Front`, solucion propia `front/Abastecimiento.Front.sln`) con render
**InteractiveServer** global. Consume la API **solo por HTTP** y desde el servidor de Blazor (no desde el navegador), asi
que no depende de CORS ni expone la URL de la API. No referencia ningun proyecto del backend: los contratos JSON estan
copiados como `record` en `front/Abastecimiento.Front/Contratos/`.

### 5.1 Como levantarlo
| Ruta | Comando | URL |
|---|---|---|
| Docker (todo junto) | `docker compose up -d --build` (db + api + front) | http://localhost:3000 |
| Docker (solo el front) | `docker compose up -d --build front` (arranca `api` y `db` si hacen falta) | http://localhost:3000 |
| Local (.NET 8 SDK) | API en 5080 (`dotnet run --project src/Abastecimiento.Api --launch-profile http`) y luego `dotnet run --project front/Abastecimiento.Front --launch-profile http` | http://localhost:5090 |

- La URL de la API sale de `Api:UrlBase` (`appsettings.json` → `http://localhost:5080`; en Docker
  `Api__UrlBase=http://api:8080`). Si no es una URL absoluta, el front no arranca.
- **Redes de Docker:** `datos` une `db` y `api`; `web` une `api` y `front`. El front solo ve a la API: no puede
  alcanzar la base de datos (`docker exec abastecimiento-front getent hosts db` no resuelve).

### 5.2 Pantallas → endpoints
| Pantalla | Ruta | Endpoints | Requerimiento |
|---|---|---|---|
| Inicio | `/` | — (buscador por HANNA → `/tiendas/{hanna}`) | — |
| Tiendas | `/tiendas` | `GET /api/tiendas?marcaTiendaId=&regionalId=`, `GET /api/catalogos/marcas-tienda`, `GET /api/catalogos/regionales` | R1 |
| Nueva tienda | `/tiendas/nueva` | `GET /api/catalogos/*`, `GET /api/tipos-espacio`, `POST /api/tiendas` | R1 |
| Detalle y carga | `/tiendas/{hanna}` | `GET /api/tiendas/{hanna}`, `GET /api/carga/{hanna}`, `GET /api/niveles-jerarquia` | R1, R3, R4a |
| Arbol comercial | `/arbol?tienda={hanna}` | `GET /api/tiendas`, `GET /api/tiendas/{hanna}/arbol-comercial`, `GET /api/niveles-jerarquia`, `GET/POST /api/conceptos-comerciales`, `POST /api/tiendas/{hanna}/nodos-comerciales`, `PUT /api/nodos-comerciales/{id}/porcentaje`, `POST /api/tiendas/{destino}/arbol-comercial/copiar-desde/{origen}` | R2 |
| Reportes | `/reportes` | `GET /api/reportes/consolidado?agruparPor=&nivelId=`, `GET /api/niveles-jerarquia` | R4b |
| Catalogos | `/catalogos` | `GET·POST /api/catalogos/*`, `GET·POST /api/tipos-espacio` | R1 |

`/tiendas/5503` muestra la tabla del enunciado: 819, 670, 482, 469, 288, 238 y el total **6,6000 espacios / 2.966 unidades**.

En **Árbol comercial** (`/arbol`) un selector elige la tienda y cada una muestra su propio arbol; la URL `?tienda=5503`
conserva la tienda al recargar. Si la tienda esta vacia se ofrece **Copiar árbol**: se elige la tienda origen y la API crea
nodos nuevos e independientes. Una tienda recien creada nace sin arbol y el alta lo indica.

### 5.3 Decisiones
- **Capas estilo MVC:** `Contratos/` y `Formularios/` (modelo), `Clientes/` (un cliente tipado por recurso, con interfaz),
  `.razor` solo con marcado y `.razor.cs` con la logica de pantalla. Ningun componente usa `HttpClient`.
- **Sin logica de negocio:** el front no recalcula espacios ni cargas; solo da formato (es-CO: 4 decimales en espacios y
  porcentajes con el valor completo en el `title`, cargas como enteros con punto de miles). Los validadores del front son
  de forma (maximo 3 tipos de mobiliario, tipos repetidos, porcentaje en (0, 100] con 4 decimales); las reglas de negocio
  las valida la API y sus errores 400/404/409 se muestran junto al campo o en el resumen del formulario.
- **Porcentajes:** el usuario escribe en % (`30` o `30,5`) y se envia la fraccion (`0.30`, `0.305`).
- **Arbol por tienda:** la pantalla no asume un arbol global; pide la tienda (o la lee de `?tienda=`) y llama a las rutas
  por tienda. El boton de copia solo se muestra cuando la tienda no tiene nodos y hay otra tienda de la que copiar.
- **Tabla de carga:** las columnas Marca y Perfil se ubican en `ruta[]` por la profundidad del nivel (3 y 6), buscando el
  nombre del nivel en `GET /api/niveles-jerarquia`, no por posicion.
- **Bootstrap 5.3.3** (copiado en `wwwroot/lib`, sin CDN) porque la plantilla .NET 8 trae 5.1, que no tiene las variables
  CSS (`--bs-btn-*`, `--bs-link-color`, `--bs-alert-*`) que usa el tema. Tablas con `QuickGrid` 8.0.11.
- **Colores:** tokens unicos en `wwwroot/css/tema.css` (paleta #014B43 / #F9ECE5); todo texto ≥ 4.5:1 y toda UI/foco
  ≥ 3:1, verificado por prueba.

### 5.4 Pruebas del front
```bash
docker compose --profile pruebas run --rm pruebas     # backend y luego front, con el SDK de .NET 8
dotnet test front/Abastecimiento.Front.sln            # solo el front (requiere el runtime de .NET 8)
```
117 pruebas xUnit + bUnit, sin API real (clientes con `HttpMessageHandler` falso y componentes con clientes falsos):

| Archivo (`front/tests/Abastecimiento.Front.Pruebas/`) | Que verifica |
|---|---|
| `ClientesPruebas.cs` | camelCase y enums en texto; `agruparPor=marcaTienda`; rutas por tienda (`/api/tiendas/{hanna}/arbol-comercial`, `/nodos-comerciales`) y copia sin cuerpo; 404 → `ExcepcionApi` con detalle; 400 `errors{}` y `errores[]`; error de red → Estado 0 |
| `ArbolPorTiendaPruebas.cs` | `/arbol?tienda=1001` carga esa tienda; el selector consulta el arbol de la otra; copiar solo aparece si la tienda esta vacia y envia el origen; nueva tienda avisa que nace sin arbol; el detalle enlaza a `arbol?tienda=...` |
| `TablaCargaPruebas.cs` | 5503: 6 filas, 819/670/482/469/288/238, total 2.966 y 6,6000; marca y perfil por profundidad del nivel |
| `FormularioTiendaPruebas.cs` | 4 mobiliarios y tipo repetido no se envian; formulario valido llama a `CrearAsync` una vez; errores 400/409 junto al campo |
| `FormularioConceptoPruebas.cs` | Nivel hoja sin unidades → invalido; nivel no hoja oculta el campo y envia `null` |
| `PorcentajePruebas.cs` | `30` → `0.30`; `0`, `101` y mas de 4 decimales → invalidos |
| `ErroresApiPruebas.cs` | `MensajeError` y ubicacion de las claves PascalCase de la API en el formulario |
| `ContrasteTemaPruebas.cs` | Cada par de colores del tema con la formula WCAG 2.1 (ej. #F9ECE5 vs #014B43 = 8.69:1) |
| `ArquitecturaPruebas.cs` | Sin colores fuera de `tema.css`, sin `@code` en `.razor`, sin `HttpClient` en componentes, sin referencias al backend |
