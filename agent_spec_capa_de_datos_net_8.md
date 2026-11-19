# AGENT SPECIFICATION: Capa de Datos y Persistencia (.NET 8 / EF Core 8 / SQL Server)

## 1. Contexto y Objetivo del Agente
Actua como un **Arquitecto Senior de Software .NET**. Tu tarea es implementar la capa de persistencia
(`Abastecimiento.Dominio` y `Abastecimiento.Infraestructura`) de un sistema de calculo de carga de
abastecimiento retail usando **C# (.NET 8)**, **Entity Framework Core 8** y **SQL Server**.

El sistema persiste:
- Tiendas fisicas con su capacidad de exhibicion (mobiliario + espacios perimetrales).
- Un arbol comercial jerarquico **dinamico** (7 niveles en la semilla, extensible sin cambiar el esquema) y **propio de cada
  tienda**: cada tienda elige sus unidades de negocio (nivel 1) con su porcentaje y su jerarquia y porcentajes debajo
  (ver §4.2 y la migracion `ArbolPorTienda`, §7.1).
- Un modulo de seguridad (usuarios, roles y sesiones de token JWT) **solo a nivel de datos**.

La base de datos se crea y se puebla automaticamente al arrancar la aplicacion (migraciones + `HasData`).

---

## 2. Reglas de Arquitectura y Restricciones (STRICT)
* **Framework:** .NET 8, EF Core 8, proveedor `Microsoft.EntityFrameworkCore.SqlServer`.
* **Arquitectura:** Clean Architecture. `Dominio` no depende de paquetes de infraestructura (ni EF Core, ni DataAnnotations de esquema). Toda configuracion de mapeo va en `IEntityTypeConfiguration<T>` dentro de `Infraestructura`.
* **Idioma:** todo el modelo en **espanol, sin tildes ni ñ** (clases, propiedades, tablas, columnas, indices, interfaces, metodos, endpoints). Los **valores** de datos conservan su texto original (ej. `PESTAÑINA`, `Góndola`, `Categoría`). **Excepcion explicita:** el esquema `auth` (abreviatura estandar, decision del usuario).
* **Precision numerica:** PROHIBIDO `float`/`double` en porcentajes o espacios. Usar `decimal`; porcentajes mapeados como `decimal(18,6)` y expresados como fraccion (`0.300000` = 30%).
* **Calculos:** todos los calculos multinivel (cascada, carga, consolidados) viven en el **backend** (`MotorCalculoCarga`, capa Aplicacion). La BD solo guarda parametros. Redondeo unicamente sobre la Carga Total final.
* **No persistir derivables:** no se guardan espacios totales, nivel del nodo, nivel hoja, espacios por nodo, cargas, consolidados, sesion activa, ultimo acceso.
* **Estandarizar repetibles:** todo valor que se repite entre filas es catalogo con FK (o enum con CHECK cuando el codigo depende de sus valores). Unico texto libre de negocio: `Tienda.Nombre`.
* **Rendimiento (anti N+1):** PROHIBIDO lazy loading (no instalar Proxies). El arbol comercial **de una tienda** se lee en
  **una sola SELECT** con `AsNoTracking()` y se procesa en memoria; los reportes leen los arboles de todas las tiendas
  tambien en una sola SELECT.
* **Integridad referencial:** `DeleteBehavior.Restrict` en relaciones a catalogos y en la relacion auto-referenciada del arbol; `NoAction` en la auto-referencia de sesiones. `Cascade` solo donde el hijo no existe sin el padre (`EspaciosTienda`, `UsuariosRoles`, `SesionesToken`).
* **Nulabilidad:** `<Nullable>enable</Nullable>` en todos los proyectos; compilacion sin warnings.
* **Esquemas por bloque:** ninguna tabla de negocio vive en `dbo` (ver seccion 4.4).
* **Indices y constraints con nombre explicito** (`UQ_`, `IX_`, `CK_`, `PK_`).

---

## 3. Convencion de Nombres
| Elemento | Convencion | Ejemplo |
|---|---|---|
| Entidad | Singular, PascalCase | `NodoComercial` |
| Esquema | Minusculas, uno por bloque | `auth`, `tiendas`, `nodoscomerciales` |
| Tabla | Plural, PascalCase, calificada por esquema | `nodoscomerciales.NodosComerciales` |
| FK | `<Entidad>Id` | `ConceptoComercialId`, `NodoPadreId` |
| Fechas | Sufijo `Utc`, tipo `datetime2` | `FechaEmisionUtc` |
| Indice unico | `UQ_<Tabla>_<Columnas>` | `UQ_Tiendas_CodigoHanna` |
| Indice | `IX_<Tabla>_<Columnas>` | `IX_Tiendas_RegionalId_MarcaTiendaId` |
| CHECK | `CK_<Tabla>_<Regla>` | `CK_NodosComerciales_PorcentajeParticipacion` |
| Interfaz repositorio | `I<Agregado>Repositorio` | `ITiendaRepositorio` |
| Metodos async | Verbo en espanol + `Async` | `ObtenerPorCodigoHannaAsync` |

Proyectos:
```
src/Abastecimiento.Dominio          Entidades, enums, excepciones, validadores, interfaces de repositorio
src/Abastecimiento.Aplicacion       Motor de calculo, DTOs, servicios
src/Abastecimiento.Infraestructura  AbastecimientoDbContext, Configuraciones, Repositorios, Migraciones
src/Abastecimiento.Api              Controladores, Swagger, middleware de excepciones
tests/Abastecimiento.Pruebas        xUnit
```

---

## 4. Especificacion de Entidades (`Dominio/Entidades`)

### 4.0. Base de catalogos
`EntidadCatalogo` (abstracta): `Id` (`int`, PK, Identity), `Nombre` (`string`, Required).
Heredan: `MarcaTienda`, `Regional`, `TipoTienda`, `ResponsableVisual`, `Rol`.
En Infraestructura, `ConfiguracionCatalogo<T>` aplica esquema, MaxLength y `UQ_<Tabla>_Nombre`.

### 4.1. Bloque Tiendas (esquema `tiendas`)

| Tabla (entidad) | Columnas | Restricciones |
|---|---|---|
| `tiendas.MarcasTienda` (`MarcaTienda`) | Id, Nombre(50) | `UQ_MarcasTienda_Nombre` |
| `tiendas.Regionales` (`Regional`) | Id, Nombre(50) | `UQ_Regionales_Nombre` |
| `tiendas.TiposTienda` (`TipoTienda`) | Id, Nombre(50) | `UQ_TiposTienda_Nombre` |
| `tiendas.ResponsablesVisuales` (`ResponsableVisual`) | Id, Nombre(100) | `UQ_ResponsablesVisuales_Nombre` |
| `tiendas.Tiendas` (`Tienda`) | Id, CodigoHanna `int`, Nombre(150), MarcaTiendaId, RegionalId, TipoTiendaId, ResponsableVisualId? | `UQ_Tiendas_CodigoHanna`; FK Restrict a los 4 catalogos |
| `tiendas.TiposEspacio` (`TipoEspacio`) | Id, Nombre(50), Categoria (enum `CategoriaEspacio`: `Mobiliario`, `Perimetral`; `nvarchar(20)` via `HasConversion<string>()`) | `UQ_TiposEspacio_Nombre`; `CK_TiposEspacio_Categoria`: `Categoria IN ('Mobiliario','Perimetral')` |
| `tiendas.EspaciosTienda` (`EspacioTienda`) | TiendaId, TipoEspacioId, CantidadEspacios `int` | `PK_EspaciosTienda (TiendaId, TipoEspacioId)`; FK Tienda **Cascade**; FK TipoEspacio Restrict; `CK_EspaciosTienda_CantidadEspacios`: `>= 0` |

**`Tienda`**
* Navegaciones: `MarcaTienda`, `Regional`, `TipoTienda`, `ResponsableVisual?`, `Espacios` (`ICollection<EspacioTienda>`, inicializada).
* Propiedad calculada (sin setter, EF no la mapea): `EspaciosTotales => Espacios.Sum(e => e.CantidadEspacios)` — **Regla 1** del enunciado.

**`ReglasEspaciosTienda`** (Dominio): una tienda admite como maximo `MaximoMobiliarioPorTienda` (= 3, constante configurable) espacios de categoria `Mobiliario`. Los `Perimetral` no cuentan. Se valida al crear/editar y lanza `ValidacionDominioExcepcion`.

> `MarcaTienda` (YAHAD, LILI PINK, YOI) es un concepto distinto de la marca de producto (nivel 3 del arbol: LILI BEAUTY, KARITE).

### 4.2. Bloque Arbol Comercial (dinamico + normalizado) (esquema `nodoscomerciales`)

| Tabla (entidad) | Columnas | Restricciones |
|---|---|---|
| `nodoscomerciales.NivelesJerarquia` (`NivelJerarquia`) | Id `int` (sin identity), Nombre(50), Profundidad `int` | `UQ_NivelesJerarquia_Nombre`; `UQ_NivelesJerarquia_Profundidad`; `CK_NivelesJerarquia_Profundidad`: `>= 1` |
| `nodoscomerciales.ConceptosComerciales` (`ConceptoComercial`) | Id, NivelJerarquiaId, Nombre(100), UnidadesPorEspacio `int?` | `UQ_ConceptosComerciales_NivelJerarquiaId_Nombre`; FK Restrict; `CK_ConceptosComerciales_UnidadesPorEspacio`: `IS NULL OR > 0` |
| `nodoscomerciales.NodosComerciales` (`NodoComercial`) | Id, **TiendaId**, NodoPadreId?, ConceptoComercialId, PorcentajeParticipacion `decimal(18,6)` | FK **`TiendaId` → `tiendas.Tiendas` Restrict** (`FK_NodosComerciales_Tiendas_TiendaId`); FK auto-referenciada **Restrict**; FK Concepto Restrict; `CK_NodosComerciales_PorcentajeParticipacion`: `> 0 AND <= 1`; `UQ_NodosComerciales_NodoPadreId_ConceptoComercialId` (hijos); **`UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz`** (filtrado `[NodoPadreId] IS NULL`: una unidad de negocio no se repite en la misma tienda); **`IX_NodosComerciales_TiendaId`** |

* **Concepto** = *que es* ("GIRLS", "LABIALES" con 660 unid./esp.), definido una sola vez y **compartido por todas las tiendas**.
* **Nodo** = *donde esta* en el arbol **de una tienda** y *con que %* respecto a su padre. "GIRLS" bajo LILI BEAUTY y bajo KARITE son 2 nodos que comparten 1 concepto; el mismo concepto en dos tiendas son nodos distintos.
* **Arbol por tienda:** cada tienda tiene su propio arbol completo. Sus **raices son sus unidades de negocio** y el
  `PorcentajeParticipacion` de una raiz es la fraccion de los espacios de la tienda que recibe esa unidad (ej. BELLEZA Y
  BIENESTAR 0.300000 + DEPORTE 0.700000). Debajo, cada tienda define su jerarquia y porcentajes. Una tienda sin nodos no
  tiene unidades de negocio (su calculo devuelve 0 con advertencia, responsabilidad de la capa de aplicacion).
* `TiendaId` se repite en todos los nodos del arbol (tambien en los hijos) en lugar de derivarse subiendo hasta la raiz:
  permite leer el arbol de una tienda en **una SELECT** por indice y validar con FK. Invariante: **padre e hijo siempre
  tienen el mismo `TiendaId`** (regla 5 del validador).
* Navegaciones: `NodoComercial.Tienda`, `NodoComercial.NodoPadre?`, `NodoComercial.NodosHijos` (inicializada), `NodoComercial.ConceptoComercial`, `ConceptoComercial.NivelJerarquia`. `Tienda` **no** expone coleccion de nodos (el arbol se lee por repositorio).
* El nivel del nodo se **deriva** de `ConceptoComercial.NivelJerarquia.Profundidad` (no se duplica en el nodo).
* El nivel hoja se **deriva**: `MAX(Profundidad)` **de `NivelesJerarquia`** (no se guarda un flag). No se deriva de los
  nodos presentes: el arbol de una tienda puede estar a medio armar.
* `UnidadesPorEspacio` pertenece al concepto del nivel hoja (Tipo de Producto).

**`ValidadorArbolComercial`** (Dominio) — reglas que cruzan filas (no expresables como CHECK):
1. Raiz ⇔ `Profundidad = 1`; en otro caso `Profundidad(hijo) = Profundidad(padre) + 1`.
2. Concepto del nivel hoja ⇒ `UnidadesPorEspacio` obligatorio; concepto de otro nivel ⇒ `null`.
3. Σ `PorcentajeParticipacion` de hermanos ≤ 1 (≤ y no =, porque la semilla es parcial: BEAUTY = 30% sin hermanas).
   Hermanos = mismo `NodoPadreId` **y mismo `TiendaId`**; para las raices, las unidades de negocio de la tienda
   (ej. 0.30 + 0.70 = 1 valido; 0.30 + 0.80 rechazado). No se exige suma exacta de 1 (decision del usuario).
4. Ramas que no alcanzan el nivel hoja → advertencia en el calculo, no error.
5. **Padre e hijo pertenecen a la misma tienda** (`nodo.TiendaId == padre.TiendaId`); si no → `ValidacionDominioExcepcion`.
   `ValidarArbol` agrupa los hermanos por `(TiendaId, NodoPadreId)`.

**Extensibilidad de niveles (soportada por el modelo, NO implementada como endpoint):**
* Nivel al final: INSERT en `NivelesJerarquia` con `Profundidad = max + 1`, crear conceptos/nodos hoja y mover `UnidadesPorEspacio` al nuevo nivel hoja.
* Nivel intermedio: en una transaccion, `UPDATE Profundidad = Profundidad + 1` de los niveles inferiores, INSERT del nuevo nivel, creacion de nodos intermedios (100% por defecto) y re-asignacion de `NodoPadreId` de los hijos. Ninguna FK cambia porque `Id ≠ Profundidad`.
* Ningun CHECK ni constante de codigo menciona "7".

> Nota EF/SQL Server: el indice unico con `NodoPadreId` nullable se genera con filtro `[NodoPadreId] IS NOT NULL` y no
> restringe raices; por eso existe `UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz` con
> `HasFilter("[NodoPadreId] IS NULL")` (las raices duplicadas **en la misma tienda** quedan rechazadas por la BD).
> Por ser un indice filtrado, toda escritura sobre `NodosComerciales` requiere `SET QUOTED_IDENTIFIER ON` (EF Core/ADO.NET y SSMS lo activan por defecto; `sqlcmd` requiere `-I`).

### 4.3. Bloque Seguridad (JWT Bearer) — SOLO DATOS (esquema `auth`)
**Alcance:** entidades en `Dominio/Entidades/Seguridad`, configuraciones EF, semilla de roles y tablas en la migracion inicial. **NO** se implementa emision/validacion de JWT, hashing, login, endpoints ni `[Authorize]`; la API permanece publica.

| Tabla (entidad) | Columnas | Restricciones |
|---|---|---|
| `auth.Usuarios` (`Usuario`) | Id `uniqueidentifier`, NombreUsuario(50), Correo(150), HashContrasena(256), SelloSeguridad `uniqueidentifier`, Activo `bit` (default 1), IntentosFallidos `int` (default 0), BloqueadoHastaUtc `datetime2?`, FechaCreacionUtc `datetime2` | `UQ_Usuarios_NombreUsuario`; `UQ_Usuarios_Correo`; `CK_Usuarios_IntentosFallidos`: `>= 0` |
| `auth.Roles` (`Rol`, hereda `EntidadCatalogo`) | Id, Nombre(50), Descripcion(200)? | `UQ_Roles_Nombre` |
| `auth.UsuariosRoles` (`UsuarioRol`) | UsuarioId, RolId | `PK_UsuariosRoles (UsuarioId, RolId)`; FK Usuario **Cascade**; FK Rol Restrict; `IX_UsuariosRoles_RolId` |
| `auth.SesionesToken` (`SesionToken`) | Id `uniqueidentifier`, UsuarioId, Jti `uniqueidentifier`, HashTokenRefresco(128), FechaEmisionUtc, FechaExpiracionUtc, FechaRevocacionUtc?, MotivoRevocacion? (enum `MotivoRevocacion`: `Rotacion`, `CierreSesion`, `CambioContrasena`, `ReusoDetectado`, `Administrativa`; `nvarchar(30)`), ReemplazadaPorSesionId?, DireccionIp(45)?, AgenteUsuario(256)? | FK Usuario **Cascade**; FK auto-referenciada `ReemplazadaPorSesionId` **NoAction**; `UQ_SesionesToken_Jti`; `UQ_SesionesToken_HashTokenRefresco`; `IX_SesionesToken_UsuarioId_FechaExpiracionUtc`; CHECKs (ver abajo) |

CHECKs de `SesionesToken`:
* `CK_SesionesToken_Fechas`: `FechaExpiracionUtc > FechaEmisionUtc`.
* `CK_SesionesToken_MotivoRevocacion`: `MotivoRevocacion IS NULL OR MotivoRevocacion IN ('Rotacion','CierreSesion','CambioContrasena','ReusoDetectado','Administrativa')`.
* `CK_SesionesToken_Revocacion`: `(FechaRevocacionUtc IS NULL AND MotivoRevocacion IS NULL) OR (FechaRevocacionUtc IS NOT NULL AND MotivoRevocacion IS NOT NULL)`.
* `CK_SesionesToken_Reemplazo`: `ReemplazadaPorSesionId IS NULL OR MotivoRevocacion = 'Rotacion'`.

Reglas de diseno:
* El **JWT de acceso no se persiste** (auto-contenido, vida corta); se guarda su `Jti` para poder revocarlo.
* El **refresh token se guarda como hash SHA-256**, nunca en texto plano. La contrasena solo como hash (formato PHC con algoritmo y sal, ej. PBKDF2 de ASP.NET Identity).
* **Rotacion:** `ReemplazadaPorSesionId` encadena sesiones; el reuso de un token rotado permite detectar robo y revocar la cadena.
* `SelloSeguridad` cambia al cambiar contrasena o roles; los tokens con el sello anterior quedan invalidos.
* Ids `Guid` en usuarios y sesiones (no enumerables, van en el claim `sub`); roles `int`.
* Derivados no persistidos: sesion activa (`FechaRevocacionUtc IS NULL AND FechaExpiracionUtc > ahora`), usuario bloqueado (`BloqueadoHastaUtc > ahora`), ultimo acceso (`MAX(FechaEmisionUtc)`).

### 4.4. Esquemas
Cada bloque vive en su propio esquema. Los bloques son independientes, con **una unica FK entre esquemas**:
`nodoscomerciales.NodosComerciales.TiendaId → tiendas.Tiendas.Id` (el arbol pertenece a una tienda; ver §8).
El bloque `auth` no tiene FK hacia los demas.

| Esquema | Tablas |
|---|---|
| `auth` | `Usuarios`, `Roles`, `UsuariosRoles`, `SesionesToken` |
| `tiendas` | `MarcasTienda`, `Regionales`, `TiposTienda`, `ResponsablesVisuales`, `Tiendas`, `TiposEspacio`, `EspaciosTienda` |
| `nodoscomerciales` | `NivelesJerarquia`, `ConceptosComerciales`, `NodosComerciales` |
| `dbo` | solo `__EFMigrationsHistory` (infraestructura de EF, no del negocio) |

* Los nombres de esquema se centralizan en la clase `Esquemas` (Infraestructura) y se aplican con `ToTable("Tabla", Esquemas.X)`.
* Permiten conceder permisos por modulo (ej. `GRANT SELECT ON SCHEMA::tiendas TO rol_reportes`) cuando exista la capa de auth.
* Los nombres de indices y CHECK no llevan el esquema (son unicos por esquema en SQL Server).
* Todo SQL manual debe usar nombres calificados (`tiendas.Tiendas`, `nodoscomerciales.NodosComerciales`, `auth.Usuarios`).

---

## 5. Configuracion de Persistencia (`Infraestructura/Persistencia`)

### 5.1. `AbastecimientoDbContext`
* `DbSet`: `MarcasTienda`, `Regionales`, `TiposTienda`, `ResponsablesVisuales`, `Tiendas`, `TiposEspacio`, `EspaciosTienda`, `NivelesJerarquia`, `ConceptosComerciales`, `NodosComerciales`, `Usuarios`, `Roles`, `UsuariosRoles`, `SesionesToken`.
* `OnModelCreating`: `modelBuilder.ApplyConfigurationsFromAssembly(typeof(AbastecimientoDbContext).Assembly)`.
* Cada `IEntityTypeConfiguration<T>` define: tabla, PK, longitudes, precision (`HasPrecision(18, 6)`), conversiones de enum, indices con nombre, CHECKs (`ToTable(t => t.HasCheckConstraint(...))`), relaciones con `OnDelete` explicito y su `HasData`.

### 5.2. Estrategia de indices
| Consulta | Indice |
|---|---|
| Tienda por HANNA (motor, desglose) | `UQ_Tiendas_CodigoHanna` |
| Consolidado / filtro por regional y marca | `IX_Tiendas_RegionalId_MarcaTiendaId` + `IX_Tiendas_MarcaTiendaId`, `IX_Tiendas_TipoTiendaId`, `IX_Tiendas_ResponsableVisualId` |
| Espacios de una tienda | `PK_EspaciosTienda (TiendaId, TipoEspacioId)` + `IX_EspaciosTienda_TipoEspacioId` |
| Arbol de una tienda (calculo de carga, lectura del arbol) | `IX_NodosComerciales_TiendaId` |
| Unidades de negocio de una tienda / raiz repetida | `UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz` (filtrado `[NodoPadreId] IS NULL`) |
| Hijos de un nodo | `UQ_NodosComerciales_NodoPadreId_ConceptoComercialId` (columna lider = padre) |
| Nodos por concepto | `IX_NodosComerciales_ConceptoComercialId` |
| Conceptos por nivel | `UQ_ConceptosComerciales_NivelJerarquiaId_Nombre` |
| Catalogos por nombre | `UQ_<Tabla>_Nombre` |
| Login / unicidad | `UQ_Usuarios_NombreUsuario`, `UQ_Usuarios_Correo` |
| Revocacion / refresco | `UQ_SesionesToken_Jti`, `UQ_SesionesToken_HashTokenRefresco` |
| Sesiones activas de un usuario | `IX_SesionesToken_UsuarioId_FechaExpiracionUtc` |
| Cadena de rotacion de tokens (FK auto-referenciada) | `IX_SesionesToken_ReemplazadaPorSesionId` |

### 5.3. Contratos de Repositorio (`Dominio/Interfaces`)
Lecturas con `AsNoTracking()`.
* `ITiendaRepositorio`
  * `Task<Tienda?> ObtenerPorCodigoHannaAsync(int codigoHanna, CancellationToken ct = default);` (incluye catalogos y `Espacios.TipoEspacio`)
  * `Task<IReadOnlyList<Tienda>> ObtenerTodasAsync(int? marcaTiendaId = null, int? regionalId = null, CancellationToken ct = default);`
  * `Task<bool> ExisteCodigoHannaAsync(int codigoHanna, CancellationToken ct = default);`
  * `Task AgregarAsync(Tienda tienda, CancellationToken ct = default);`
* `IArbolComercialRepositorio`
  * `Task<IReadOnlyList<NodoComercial>> ObtenerArbolCompletoAsync(CancellationToken ct = default);` — `Include(ConceptoComercial).ThenInclude(NivelJerarquia)`: **una sola SELECT**.
  * `Task AgregarConceptoAsync(ConceptoComercial concepto, CancellationToken ct = default);`
  * `Task AgregarNodoAsync(NodoComercial nodo, CancellationToken ct = default);`
* `ICatalogoRepositorio<T> where T : EntidadCatalogo` — `ObtenerTodosAsync`, `AgregarAsync`.
* `IUnidadDeTrabajo` — `Task<int> GuardarCambiosAsync(CancellationToken ct = default);`
* Seguridad: **sin repositorios** en esta fase (solo datos).

**Requeridos por la capa de aplicacion** (`agent_spec_capa_de_aplicacion_net_8.md` §11; no cambian tablas ni migraciones):
* `IArbolComercialRepositorio`
  * `Task<IReadOnlyList<NivelJerarquia>> ObtenerNivelesAsync(CancellationToken ct = default);` (orden por `Profundidad`)
  * `Task<IReadOnlyList<ConceptoComercial>> ObtenerConceptosAsync(int? nivelJerarquiaId = null, CancellationToken ct = default);` (con nivel)
  * `Task<ConceptoComercial?> ObtenerConceptoPorIdAsync(int id, CancellationToken ct = default);` (con nivel)
  * `Task<bool> ExisteConceptoAsync(int nivelJerarquiaId, string nombre, CancellationToken ct = default);`
  * `Task<bool> ExisteNodoAsync(int? nodoPadreId, int conceptoComercialId, CancellationToken ct = default);`
  * `Task<NodoComercial?> ObtenerNodoParaEditarAsync(int id, CancellationToken ct = default);` (**con** seguimiento, incluye concepto y nivel)
* `ICatalogoRepositorio<T>` — `Task<bool> ExisteAsync(int id, CancellationToken ct = default);`, `Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);`
* `ITipoEspacioRepositorio` (nuevo)
  * `Task<IReadOnlyList<TipoEspacio>> ObtenerTodosAsync(CancellationToken ct = default);`
  * `Task<IReadOnlyList<TipoEspacio>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct = default);`
  * `Task<bool> ExisteNombreAsync(string nombre, CancellationToken ct = default);`
  * `Task AgregarAsync(TipoEspacio tipoEspacio, CancellationToken ct = default);`

**Requeridos por el arbol por tienda** (migracion `ArbolPorTienda`, §7.1):
* `IArbolComercialRepositorio`
  * `Task<IReadOnlyList<NodoComercial>> ObtenerArbolPorTiendaAsync(int tiendaId, CancellationToken ct = default);` —
    `Where(n => n.TiendaId == tiendaId)` + `Include(ConceptoComercial).ThenInclude(NivelJerarquia)`, `AsNoTracking`,
    orden por `Id`: **una sola SELECT** (usa `IX_NodosComerciales_TiendaId`). Tienda sin nodos → lista vacia.
  * `ObtenerArbolCompletoAsync()` **se conserva** con la misma firma: devuelve los nodos de **todas** las tiendas (ahora
    con `TiendaId`) en una SELECT; la usan los reportes, que agrupan por `TiendaId` en memoria.
  * `Task<bool> ExisteNodoAsync(int tiendaId, int? nodoPadreId, int conceptoComercialId, CancellationToken ct = default);`
    — nueva sobrecarga: con `nodoPadreId` null valida la unidad de negocio repetida **dentro de la tienda**.
    La sobrecarga anterior `ExisteNodoAsync(int? nodoPadreId, int conceptoComercialId, ...)` **se mantiene** para no
    romper la capa de aplicacion; la eliminara el agente de aplicacion al adoptar la nueva.
  * `Task AgregarNodosAsync(IEnumerable<NodoComercial> nodos, CancellationToken ct = default);` — alta en lote (copia del
    arbol de una tienda a otra; los nuevos nodos se enlazan por la navegacion `NodoPadre` y se guardan con un solo
    `GuardarCambiosAsync`).
* `ObtenerNodoParaEditarAsync` incluye `TiendaId` (columna escalar, sin cambios de firma).

> Con `AsNoTracking()` EF **no** enlaza `NodoPadre`/`NodosHijos` entre las entidades devueltas. El motor construye el arbol en memoria con `nodos.ToLookup(n => n.NodoPadreId)`; no depende de esas navegaciones.

---

## 6. Especificacion Obligatoria de Datos Semilla (`HasData`, IDs explicitos)

### Catalogos de tienda
| Tabla | Registros |
|---|---|
| `MarcasTienda` | 1 YAHAD, 2 LILI PINK, 3 YOI |
| `Regionales` | 1 BOGOTA, 2 ANTIOQUIA |
| `TiposTienda` | 1 MIXTA, 2 MONOMARCA |
| `ResponsablesVisuales` | (vacio) |
| `TiposEspacio` | 1 Isla, 2 Casa David, 3 Mesa, 4 Góndola, 5 Mesa Central, 6 Exhibidor (`Mobiliario`); 7 Perimetral (`Perimetral`) |

### `Tiendas`
| Id | CodigoHanna | Nombre | MarcaTiendaId | RegionalId | TipoTiendaId | ResponsableVisualId |
|---|---|---|---|---|---|---|
| 1 | 5503 | LILI EN YOI YAHAD TOBERIN BOGOTA | 1 | 1 | 1 | null |
| 2 | 1001 | LILI PINK UNICENTRO BOGOTA | 2 | 1 | 2 | null |
| 3 | 2045 | YOI VIVA ENVIGADO | 3 | 2 | 2 | null |

### `EspaciosTienda` (10)
| TiendaId | TipoEspacioId | CantidadEspacios |
|---|---|---|
| 1 | 1 Isla | 7 |
| 1 | 2 Casa David | 4 |
| 1 | 3 Mesa | 4 |
| 1 | 7 Perimetral | 7 |
| 2 | 4 Góndola | 10 |
| 2 | 1 Isla | 6 |
| 2 | 7 Perimetral | 14 |
| 3 | 5 Mesa Central | 8 |
| 3 | 6 Exhibidor | 6 |
| 3 | 7 Perimetral | 6 |

Totales esperados: 5503 = **22**, 1001 = **30**, 2045 = **20**.

### `NivelesJerarquia` (7)
| Id | Nombre | Profundidad |
|---|---|---|
| 1 | Unidad de Negocio | 1 |
| 2 | Categoría | 2 |
| 3 | Marca de Producto | 3 |
| 4 | Segmento | 4 |
| 5 | Subcategoría | 5 |
| 6 | Perfil | 6 |
| 7 | Tipo de Producto | 7 |

### `ConceptosComerciales` (15)
| Id | NivelJerarquiaId | Nombre | UnidadesPorEspacio |
|---|---|---|---|
| 1 | 1 | BELLEZA Y BIENESTAR | null |
| 2 | 2 | BEAUTY | null |
| 3 | 3 | LILI BEAUTY | null |
| 4 | 3 | KARITE | null |
| 5 | 4 | GIRLS | null |
| 6 | 5 | MAKEUP | null |
| 7 | 6 | LABIOS | null |
| 8 | 6 | OJOS | null |
| 9 | 6 | ROSTRO | null |
| 10 | 7 | LABIALES | 660 |
| 11 | 7 | BRILLO LABIOS | 540 |
| 12 | 7 | PESTAÑINA | 288 |
| 13 | 7 | DELINEADOR | 420 |
| 14 | 7 | BASE / POLVOS | 310 |
| 15 | 7 | BÁLSAMO LABIAL | 600 |

### `NodosComerciales` (54 = 3 tiendas × 18)
Cada tienda de la semilla tiene **el mismo arbol** (BELLEZA Y BIENESTAR al 100%), por eso sus cargas no cambian respecto al
arbol global anterior. La tabla define el arbol de **5503 (TiendaId 1, Ids 1–18)**; los de 1001 y 2045 se generan a partir
de ella en `DatosSemilla` (sin duplicar literales) con esta regla:

| Tienda | TiendaId | Ids | NodoPadreId |
|---|---|---|---|
| 5503 | 1 | 1–18 (los de la tabla) | los de la tabla |
| 1001 | 2 | `2000 + Id` → 2001–2018 | `2000 + NodoPadreId` (raiz: null) |
| 2045 | 3 | `3000 + Id` → 3001–3018 | `3000 + NodoPadreId` (raiz: null) |

Mismo `ConceptoComercialId` y `PorcentajeParticipacion` que el nodo de origen. Los rangos 2000/3000 evitan choques con
nodos que un usuario ya haya creado en una BD existente (identity).
`DatosSemilla.NodosComerciales` devuelve los 54; `DatosSemilla.NodosComercialesDeTienda(int tiendaId)` devuelve los 18 de
una tienda (lo usan las pruebas).

| Id | NodoPadreId | ConceptoComercialId | PorcentajeParticipacion | (referencia) |
|---|---|---|---|---|
| 1 | null | 1 | 1.000000 | BELLEZA Y BIENESTAR |
| 2 | 1 | 2 | 0.300000 | BEAUTY |
| 3 | 2 | 3 | 0.940000 | LILI BEAUTY |
| 4 | 2 | 4 | 0.060000 | KARITE |
| 5 | 3 | 5 | 1.000000 | GIRLS (LILI BEAUTY) |
| 6 | 5 | 6 | 1.000000 | MAKEUP |
| 7 | 6 | 7 | 0.400000 | LABIOS |
| 8 | 6 | 8 | 0.450000 | OJOS |
| 9 | 6 | 9 | 0.150000 | ROSTRO |
| 10 | 7 | 10 | 0.500000 | LABIALES |
| 11 | 7 | 11 | 0.500000 | BRILLO LABIOS |
| 12 | 8 | 12 | 0.600000 | PESTAÑINA |
| 13 | 8 | 13 | 0.400000 | DELINEADOR |
| 14 | 9 | 14 | 1.000000 | BASE / POLVOS |
| 15 | 4 | 5 | 1.000000 | GIRLS (KARITE) |
| 16 | 15 | 6 | 1.000000 | MAKEUP |
| 17 | 16 | 7 | 1.000000 | LABIOS |
| 18 | 17 | 15 | 1.000000 | BÁLSAMO LABIAL |

### Seguridad

`Roles` (4):
| Id | Nombre | Descripcion |
|---|---|---|
| 1 | admin | Crea tiendas y administra usuarios, roles y catalogos |
| 2 | admin_tienda | Administra tiendas existentes (actualiza sus datos y espacios); no crea tiendas |
| 3 | Analista | Parametriza el arbol comercial (conceptos, nodos y porcentajes) |
| 4 | Consulta | Solo lectura de calculos y reportes |

| Tabla | Registros |
|---|---|
| `Usuarios`, `UsuariosRoles`, `SesionesToken` | (vacias) — no se siembra un usuario para no fijar una credencial en la migracion |

> Solo registros de datos: la asignacion de responsabilidades queda en `Descripcion`; la autorizacion por rol
> (`[Authorize(Roles = ...)]` o politicas) se implementara cuando exista la capa de auth.

### Resultado esperado del calculo (HANNA 5503, verificacion del modelo)
| Ruta | Espacios | Unid./Esp. | Carga |
|---|---|---|---|
| LILI BEAUTY > LABIOS > LABIALES | 1.2408 | 660 | 819 |
| LILI BEAUTY > LABIOS > BRILLO LABIOS | 1.2408 | 540 | 670 |
| LILI BEAUTY > OJOS > PESTAÑINA | 1.67508 | 288 | 482 |
| LILI BEAUTY > OJOS > DELINEADOR | 1.11672 | 420 | 469 |
| LILI BEAUTY > ROSTRO > BASE / POLVOS | 0.9306 | 310 | 288 |
| KARITE > LABIOS > BÁLSAMO LABIAL | 0.396 | 600 | 238 |
| **TOTAL BEAUTY** | **6.6** | | **2,966** |

Con su propio arbol (identico) las otras tiendas conservan sus resultados: **1001 = 4,046** (30 espacios) y
**2045 = 2,696** (20 espacios).

---

## 7. Inicializacion Automatica (`Program.cs`)
Metodo de extension `app.AplicarMigracionesAsync()` que resuelve `AbastecimientoDbContext` en un `IServiceScope` y ejecuta `await contexto.Database.MigrateAsync();` al arrancar.
Cadena de conexion `ConnectionStrings:Abastecimiento` en `appsettings.json` (LocalDB por defecto; alternativa Docker `mcr.microsoft.com/mssql/server:2022-latest`).

**Fuente de verdad: las migraciones EF Core** (code-first). Formas de crear la BD: (1) `dotnet run` de la API (migra al arrancar); (2) `docker compose up -d --build` (SQL Server + API en contenedores, ver spec de aplicacion §10); (3) el script SQL descrito a continuacion. Alternativa sin .NET: ejecutar `database/script_inicial.sql`
(script **idempotente**: se puede ejecutar varias veces sin duplicar objetos ni datos, y registra la migracion en
`dbo.__EFMigrationsHistory`, por lo que la API no la vuelve a aplicar). Requiere `QUOTED_IDENTIFIER ON` (`sqlcmd -I`).
Es un artefacto derivado: se regenera tras cada migracion nueva con
`dotnet ef migrations script --idempotent -p src/Abastecimiento.Infraestructura -s src/Abastecimiento.Api -o database/script_inicial.sql`.

### 7.1. Migracion `ArbolPorTienda` (cambio sobre una BD existente)
Segunda migracion; **no** se modifica `MigracionInicial`. Crear con
`dotnet ef migrations add ArbolPorTienda -p src/Abastecimiento.Infraestructura -s src/Abastecimiento.Api` y **revisar el
codigo generado** antes de aplicarlo. Orden obligatorio dentro de `Up`:
1. `AddColumn<int>("TiendaId", schema: "nodoscomerciales", table: "NodosComerciales", nullable: false, defaultValue: 1)`.
   **Editar a mano** el `defaultValue` que genera EF (0) por **1**: todos los nodos existentes, incluidos los que un
   usuario haya creado a mano con el arbol global, quedan en la tienda 1 (5503), que era la referencia del arbol global.
   Sin esto la FK del paso 4 falla.
2. Datos: `UpdateData` de los nodos 1–18 (TiendaId 1) e `InsertData` de los 36 nodos de 1001 (2001–2018) y 2045
   (3001–3018), en orden de padre a hijo.
3. Indices: `IX_NodosComerciales_TiendaId` y `UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz` (filtrado). Si la BD
   ya tuviera raices repetidas en 5503, este indice falla: se considera un dato invalido que debe corregirse a mano.
4. `AddForeignKey` `FK_NodosComerciales_Tiendas_TiendaId` → `tiendas.Tiendas(Id)` con `ReferentialAction.Restrict`.

`Down` revierte en orden inverso (FK, indices, nodos 2001–3018, columna). Tras crearla:
* `dotnet ef migrations has-pending-model-changes` → "No changes have been made to the model since the last migration".
* Regenerar `database/script_inicial.sql` (idempotente, ahora con ambas migraciones).
* Probar la migracion sobre una BD que ya tenga `MigracionInicial` aplicada (la de Docker o LocalDB) y sobre una vacia.

### 7.2. Coordinacion con la capa de aplicacion (OBLIGATORIO)
Este cambio de datos **debe entregarse junto con** el ajuste de la capa de aplicacion (`agent_spec_capa_de_aplicacion_net_8.md`),
nunca solo. Mientras la aplicacion siga leyendo `ObtenerArbolCompletoAsync()` para calcular una tienda, recibiria los
**54** nodos (los 3 arboles) y la carga de 5503 saldria triplicada. Lo que corresponde a la aplicacion (fuera de este spec):
calcular con `ObtenerArbolPorTiendaAsync`, profundidad hoja desde `NivelesJerarquia`, rutas por tienda
(`/api/tiendas/{codigoHanna}/arbol-comercial`, `/nodos-comerciales`, copia de arbol) y reportes agrupados por `TiendaId`.

Para no romper las pruebas unitarias existentes, el agente de datos ajusta los datos de prueba:
* `tests/.../Datos/ArbolDePrueba.DesdeSemilla(int tiendaId = 1)`: devuelve solo el arbol de esa tienda (18 nodos) con
  sus navegaciones enlazadas. Con el valor por defecto, las pruebas actuales del motor y de los servicios siguen
  calculando 5503 = 2,966.
* `ModeloEfPruebas` existentes que cambian: `LaSemillaTieneLosConteosDeLaSpec` (`NodoComercial` 18 → **54**) y la lista
  de indices esperados (agregar `UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz` e `IX_NodosComerciales_TiendaId`).
  `LaRelacionAutoreferenciadaDelArbolEsRestrict` no cambia (filtra por la FK hacia `NodoComercial`).
* `RepositoriosSqlServerPruebas`: `ObtenerArbolCompletoAsync` → **54** nodos en una SELECT;
  nueva prueba `ObtenerArbolPorTiendaAsync(1)` → **18** nodos, todos con `TiendaId = 1`, en una SELECT.
* Pruebas nuevas de datos (sin la capa de aplicacion): `ModeloEfPruebas` verifica la columna `TiendaId` requerida, la FK
  Restrict a `Tiendas`, el indice filtrado de raices y la semilla de 54 nodos (18 por tienda, Ids 1–18 / 2001–2018 /
  3001–3018, mismos conceptos y %); `ValidadorArbolComercialPruebas` verifica la regla 5 (padre de otra tienda → error) y
  que `ValidarArbol` sobre los 54 nodos no reporte errores (hermanos agrupados por tienda); con SQL Server, insertar una
  raiz repetida en la misma tienda es rechazado por `UQ_NodosComerciales_TiendaId_ConceptoComercialId_Raiz`, y la misma
  raiz en otra tienda es aceptada.
* Las pruebas de API (`ApiPruebas`) y de repositorios de catalogo no cambian; las de API que calculan cargas fallaran hasta
  que la capa de aplicacion adopte el arbol por tienda (es la ventana descrita arriba): no las desactives.

---

## 8. Decisiones de Diseno y Justificacion
| Decision | Justificacion |
|---|---|
| **SQL Server** | `decimal` nativo con precision real, CHECK constraints, indices filtrados. En SQLite EF guarda `decimal` como TEXT e ignora `HasPrecision`. |
| **Arbol: lista de adyacencia (auto-referencia) + catalogo de conceptos + catalogo de niveles** | Jerarquia dinamica y en 3FN: nuevas categorias/ramas son INSERTs; nuevos niveles (al final o intermedios) no cambian el esquema. Separar concepto de nodo evita repetir nombres ("GIRLS", "MAKEUP", "LABIOS") por cada ruta. Se descarto 7 tablas fijas (rigido ante cambios de niveles). |
| **Esquemas por bloque (`auth`, `tiendas`, `nodoscomerciales`)** | Agrupa las tablas por modulo (sin tablas sueltas en `dbo`), hace visible que los bloques son independientes y habilita permisos por esquema. Unica FK entre esquemas: `NodosComerciales.TiendaId → Tiendas` (ver fila siguiente). |
| **Arbol comercial por tienda (`NodosComerciales.TiendaId`)** | Cada tienda define sus unidades de negocio y porcentajes (ej. Belleza 30% + Deporte 70%), que el arbol global no permitia. Los conceptos y niveles siguen globales (no se repite el "que es"); solo los nodos ("donde" y "con que %") son de la tienda. La FK entre esquemas es la forma correcta de expresar "el arbol pertenece a una tienda" y la BD garantiza que no haya nodos huerfanos. `TiendaId` va en cada nodo (no solo en la raiz) para leer el arbol de una tienda en una SELECT por indice. |
| **`Profundidad` separada del `Id` en niveles** | Permite insertar niveles intermedios sin renumerar PKs referenciadas por FK. |
| **Capacidad fisica como `TiposEspacio` + `EspaciosTienda`** | Elimina el grupo repetido Mueble1/2/3 (1FN) y el texto duplicado de muebles; mobiliario y perimetral se estandarizan en un solo catalogo; Regla 1 = Σ espacios. El tope de 3 muebles es regla de dominio configurable. |
| **Catalogos para marca, regional, tipo de tienda, responsable visual** | Valores repetidos estandarizados; agrupaciones de reportes sobre claves, sin inconsistencias de texto. |
| **Enums con CHECK (`CategoriaEspacio`, `MotivoRevocacion`)** | Valores cerrados de los que depende el codigo; el CHECK garantiza el dominio en BD. |
| **No persistir derivables** | Espacios totales, nivel, nivel hoja, espacios por nodo, cargas, consolidados, sesion activa y ultimo acceso se calculan; nunca quedan desactualizados. |
| **Calculos en el backend** | `decimal` de C# (28-29 digitos) no pierde escala en multiplicaciones encadenadas (SQL Server si); motor testeable sin BD; regla de negocio fuera de SQL. |
| **Lectura del arbol en 1 SELECT + recorrido en memoria** | Evita N+1 y recursion en SQL; el tamano del arbol lo permite holgadamente. |
| **Seguridad: refresh token hasheado, Jti, sello de seguridad, rotacion encadenada** | Revocacion de tokens sin guardar secretos en claro; deteccion de reuso de tokens robados. |

**Descartado (con motivo):**
* **Ruta materializada / `hierarchyid` / indice jerarquico:** es un valor derivado persistido que hay que mantener al mover nodos; el motor no lo necesita. Camino de escalamiento si el arbol llegara a cientos de miles de nodos.
* **Vista indexada (materializada) de niveles 1–6:** SQL Server no admite recursion en vistas indexadas (obligaria a 6 self-joins fijos atados a 7 niveles) y la multiplicacion encadenada de `decimal` en SQL pierde escala.
* **Vista no materializada de rutas:** no sirve para calcular (precision) y duplicaria logica; la API expone ruta legible y % acumulado calculados en C#.
* **Mobiliario en columnas fijas:** grupo repetido (viola 1FN) y texto duplicado.
* **Arbol global + tabla de aplicabilidad por tienda o marca:** permitia elegir ramas por tienda pero no porcentajes
  distintos por tienda, que es lo que pide el negocio.
* **`TiendaId` solo en la raiz:** evita repetir la columna, pero obliga a recorrer el arbol hacia arriba para saber de que
  tienda es un nodo y no permite leer el arbol de una tienda con un filtro por indice.

---

## 9. Normalizacion
* Todas las tablas cumplen **3FN / BCNF**: cada determinante es clave candidata (`Id`, `CodigoHanna`, `Nombre` en catalogos, `(NivelJerarquiaId, Nombre)`, `NombreUsuario`, `Correo`, `Jti`, `HashTokenRefresco`); las tablas puente (`EspaciosTienda` con su cantidad, `UsuariosRoles`) dependen de su clave compuesta completa.
* Sin grupos repetidos (1FN), sin dependencias parciales (2FN), sin dependencias transitivas (3FN).
* **Excepcion consciente — `NodosComerciales.TiendaId`:** en un hijo, `TiendaId` se deduce del padre
  (`Id → NodoPadreId → TiendaId`), asi que repetirlo es una desnormalizacion controlada. Se acepta porque permite leer
  el arbol de una tienda con un filtro por indice en una SELECT (sin recursion) y validar la pertenencia con FK. La
  consistencia la garantiza la regla 5 del `ValidadorArbolComercial` (padre e hijo con el mismo `TiendaId`) en cada
  alta de nodo; la copia de arboles asigna el `TiendaId` destino a todos los nodos.
* `HashContrasena` se trata como valor atomico (formato PHC opaco). `DireccionIp`/`AgenteUsuario` son hechos de la sesion. `FechaExpiracionUtc` no es derivable (vida del token configurable, fijada al emitir).

---

## 10. Supuestos
* **A. Arbol por tienda:** cada tienda tiene su propio arbol (unidades de negocio, jerarquia y porcentajes). Las 3 tiendas
  de la semilla tienen el mismo arbol (BELLEZA Y BIENESTAR 100%), por eso la tabla del enunciado para 5503 se conserva.
  Conceptos y niveles son globales. Las unidades de negocio de una tienda suman ≤ 100% (no se exige 100% exacto).
  Fuera de alcance de este cambio: eliminar nodos o unidades de negocio.
* **B. `UnidadesPorEspacio` por tipo de producto** (no por ruta), segun el enunciado. Si dependiera de la ruta, se mueve a `NodoComercial`.
* **C. Tipo de tienda y responsable visual** no vienen en el enunciado: tipos MIXTA (YAHAD, "formato mixto") / MONOMARCA; responsables visuales en null.
* **D. Precision** `decimal(18,6)` en porcentajes; Σ % de hermanos ≤ 1 porque la semilla es parcial.
* **E. Redondeo:** solo en la Carga Total final, `MidpointRounding.AwayFromZero` por defecto, configurable en `Calculo:ModoRedondeo`. Total = Σ de cargas redondeadas por linea (para 5503 ambas convenciones dan 2,966).
* **F. Seguridad:** solo modelo de datos; sin usuario semilla; extension futura `Permisos` + `RolesPermisos` si se requiere autorizacion granular.

---

## 11. Criterios de Aceptacion (Definition of Done)
1. **Compilacion limpia:** `dotnet build` sin warnings de nulabilidad.
2. **Migracion generada:** `dotnet ef migrations add MigracionInicial -p src/Abastecimiento.Infraestructura -s src/Abastecimiento.Api` crea los 3 esquemas y las 14 tablas (4 en `auth`, 7 en `tiendas`, 3 en `nodoscomerciales`; en `dbo` solo `__EFMigrationsHistory`) con PKs, FKs, indices y CHECKs con los nombres de esta spec.
3. **Auto-seeding:** contra una BD vacia, al arrancar la API quedan: 3 MarcasTienda, 2 Regionales, 2 TiposTienda, 0 ResponsablesVisuales, 7 TiposEspacio, 3 Tiendas, 10 EspaciosTienda, 7 NivelesJerarquia, 15 ConceptosComerciales, **54 NodosComerciales (18 por tienda)**, 4 Roles, 0 Usuarios/UsuariosRoles/SesionesToken.
4. **Espacios totales:** 5503 = 22, 1001 = 30, 2045 = 20.
5. **Lectura del arbol:** `ObtenerArbolPorTiendaAsync(1)` retorna **18** nodos (todos `TiendaId = 1`) y `ObtenerArbolCompletoAsync()` retorna **54**, cada uno en una unica sentencia `SELECT` (verificable con `LogTo`).
6. **Integridad:** insertar un % > 1, un `CantidadEspacios` negativo, una `Categoria` invalida, una sesion revocada sin motivo, un nodo con `TiendaId` inexistente o una **unidad de negocio repetida en la misma tienda** es rechazado por la BD.
7. **Calculo de referencia:** con la semilla, el arbol de HANNA 5503 produce exactamente la tabla de la seccion 6 (total 6.6 espacios y 2,966 unidades); 1001 = 4,046 y 2045 = 2,696 con sus propios arboles (verificable con el motor sobre `DatosSemilla.NodosComercialesDeTienda(tiendaId)`).
8. **Migracion `ArbolPorTienda`** (§7.1): se aplica sin errores sobre una BD que ya tenia `MigracionInicial` (los nodos previos quedan en 5503) y sobre una vacia; `has-pending-model-changes` sin cambios; `database/script_inicial.sql` regenerado.
9. **Pruebas:** las de §7.2 en verde; las unitarias existentes siguen en verde con `ArbolDePrueba.DesdeSemilla()` (tienda 1).
