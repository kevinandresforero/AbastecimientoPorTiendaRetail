# AGENT SPECIFICATION: Frontend (Blazor Web App / .NET 8)

## 1. Contexto y Objetivo del Agente
Actua como un **Desarrollador Senior Blazor (.NET 8)**. Tu tarea es construir la interfaz web del sistema de calculo de
carga de abastecimiento retail: un proyecto **aparte** en `front/` que consume **solo por HTTP** la API REST ya
implementada, probada y documentada en Swagger. No se modifica el backend.

Documentos de referencia, **en este orden de prioridad**:
1. `Prueba Técnica - Desarrollador .md` (enunciado: reglas de calculo, requerimientos R1–R4, tabla esperada de HANNA 5503).
2. `agent_spec_capa_de_aplicacion_net_8.md` §6 (endpoints), §7 (contratos JSON) y §8 (errores): **es el contrato**.
   Ante cualquier duda, manda el Swagger real: `http://localhost:8080/swagger` (Docker) o `http://localhost:5080/swagger`.
3. `README.md`, secciones "Endpoints" y "Consumir desde un front".
4. Este documento.

### 1.1. Trazabilidad con el enunciado
| Enunciado | Pantalla (§5) |
|---|---|
| R1 — Gestion de tiendas y mobiliario (Espacios Totales automaticos, Regla 1) | Tiendas, Nueva tienda, Detalle de tienda, Catalogos |
| R2 — Parametrizacion del arbol de 7 niveles (% por nodo, unidades por espacio en la hoja) | Arbol comercial |
| R3 — Motor de calculo por codigo HANNA (espacios proporcionales y Carga Total) | Detalle de tienda → pestaña Carga |
| R4a — Desglose de carga por producto de una tienda (ej. 5503) | Detalle de tienda → tabla de lineas |
| R4b — Consolidado por Tienda, Marca o Regional | Reportes |

---

## 2. Reglas de Arquitectura y Restricciones (STRICT)
* **Alcance:** pantallas para **todos** los endpoints de la API (§4). Fuera de alcance: autenticacion, editar o eliminar
  tiendas, conceptos o nodos (la API no tiene esos endpoints), internacionalizacion.
* **Proyecto aparte:** `front/Abastecimiento.Front` con su propia solucion `front/Abastecimiento.Front.sln`.
  **PROHIBIDO** referenciar proyectos del backend (`Abastecimiento.Dominio`, `.Aplicacion`, `.Infraestructura`, `.Api`).
  Los modelos del contrato se escriben en el front (§3, `Contratos/`).
* **Tecnologia:** Blazor Web App .NET 8 (`dotnet new blazor --interactivity Server --all-interactive`), modo de render
  **`InteractiveServer` global**. Las llamadas a la API salen del servidor de Blazor (no del navegador): no dependen de CORS
  y la URL de la API no se expone al cliente.
* **UI:** Bootstrap 5 (incluido en la plantilla) + `Microsoft.AspNetCore.Components.QuickGrid` 8.0.x para tablas.
  Sin otras librerias de componentes (ni MudBlazor, ni Radzen, ni Syncfusion).
* **Capas dentro del front (estilo MVC):**
  * **Modelo:** `Contratos/` (records que reflejan el JSON de la API) y `Formularios/` (modelos de formulario con DataAnnotations).
  * **Acceso a datos remoto:** `Clientes/` — un cliente tipado por recurso con su interfaz (`IClienteTiendas`, …).
  * **Vista:** archivos `.razor` **solo con marcado** y binding.
  * **Controlador:** code-behind `.razor.cs` (clase `partial`) con la logica de pantalla: carga de datos, estado,
    manejo de errores y navegacion. **PROHIBIDO** un bloque `@code` con logica en el `.razor`.
  * Ningun componente usa `HttpClient` directamente: solo `IClienteX` inyectados.
* **Sin logica de negocio:** el front **no** recalcula espacios ni cargas, ni redondea; muestra lo que entrega la API.
  Solo valida la **forma** de las entradas (§6). Las reglas de negocio (suma de hermanos ≤ 100%, duplicados, saltos de
  nivel) las valida la API y el front muestra sus errores.
* **Async** de punta a punta, con `CancellationToken` en los clientes. Cancelar las cargas pendientes al salir de la
  pagina (`IDisposable` + `CancellationTokenSource`).
* **Idioma:** espanol **sin tildes ni ñ** en clases, metodos, propiedades, rutas y archivos. Los **textos visibles** de la UI
  si llevan tildes y ñ ("Categoría", "Pestañina"). Los valores de datos se muestran tal como llegan.
* **Formato de numeros (cultura `es-CO`):** espacios y porcentajes con **4 decimales** y el valor completo en el atributo
  `title` (tooltip); cargas en unidades como **enteros con separador de miles** (2.966); porcentajes como % (0.94 → 94,0000 %).
* **Nulabilidad** habilitada y compilacion **sin warnings** (`TreatWarningsAsErrors` llega por el `Directory.Build.props`
  de la raiz; no desactivarlo).
* **Accesibilidad basica:** `label` en cada campo, botones con texto, estados de carga y de error visibles.
* **Colores y contraste:** paleta basada en #014B43 y #F9ECE5 (orientativa); **todo texto ≥ 4.5:1 y toda UI/foco ≥ 3:1**
  contra su fondo, sin excepciones (§7). Colores solo via tokens de `tema.css`.

---

## 3. Estructura de Archivos
```
front/
  Abastecimiento.Front.sln
  Dockerfile
  Abastecimiento.Front/
    Abastecimiento.Front.csproj
    Program.cs
    appsettings.json                    (Api:UrlBase)
    Properties/launchSettings.json      (perfil http → http://localhost:5090)
    Configuracion/OpcionesApi.cs
    Contratos/                          (records del JSON: Tiendas, Catalogos, ArbolComercial, Carga, Reportes)
    Contratos/ProblemaApi.cs            (problem+json)
    Clientes/IClienteTiendas.cs + ClienteTiendas.cs
    Clientes/IClienteCatalogos.cs + ClienteCatalogos.cs      (marcas, regionales, tipos de tienda, responsables)
    Clientes/IClienteTiposEspacio.cs + ClienteTiposEspacio.cs
    Clientes/IClienteArbolComercial.cs + ClienteArbolComercial.cs   (niveles, arbol, conceptos, nodos)
    Clientes/IClienteCarga.cs + ClienteCarga.cs
    Clientes/IClienteReportes.cs + ClienteReportes.cs
    Clientes/ExcepcionApi.cs + LectorRespuestas.cs   (deserializa exito o problem+json)
    Formularios/                        (modelos de formulario + validadores propios)
    Componentes/App.razor, Routes.razor, Layout/MainLayout.razor, Layout/NavMenu.razor
    Componentes/Pages/                  (§5; cada pagina .razor + .razor.cs)
    Componentes/Compartidos/            (§5.8)
    Utilidades/Formato.cs               (formato de numeros y porcentajes)
    wwwroot/css/tema.css                (tokens de color y variables de Bootstrap, §7)
  tests/Abastecimiento.Front.Pruebas/   (xUnit + bUnit, §9)
```

---

## 4. Contrato con la API

### 4.1. Cliente HTTP
* `OpcionesApi { const string Seccion = "Api"; string UrlBase }`; validar al arrancar que `UrlBase` sea una URL absoluta.
* Registrar cada cliente con `builder.Services.AddHttpClient<IClienteX, ClienteX>(c => c.BaseAddress = new(urlBase))`.
* Un unico `JsonSerializerOptions` compartido: `JsonSerializerDefaults.Web` (camelCase, case-insensitive) +
  `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)` (la API envia y recibe enums como texto camelCase:
  `"mobiliario"`, `"perimetral"`, `"marcaTienda"`).
* `LectorRespuestas`: si la respuesta es 2xx deserializa el tipo esperado; si no, lee `application/problem+json` y lanza
  `ExcepcionApi { int Estado; string Titulo; string? Detalle; IReadOnlyList<string> Errores; IReadOnlyDictionary<string, string[]> ErroresPorCampo }`.
  Errores de red o timeout → `ExcepcionApi` con `Estado = 0` y mensaje "No se pudo conectar con la API".

### 4.2. Endpoints
| Metodo y ruta | Cliente / metodo | Cuerpo | Respuesta | Errores |
|---|---|---|---|---|
| GET `/api/tiendas?marcaTiendaId=&regionalId=` | `IClienteTiendas.ListarAsync` | — | `TiendaRespuesta[]` | — |
| GET `/api/tiendas/{codigoHanna}` | `ObtenerAsync` | — | `TiendaRespuesta` | 404 |
| POST `/api/tiendas` | `CrearAsync` | `CrearTiendaSolicitud` | 201 `TiendaRespuesta` (+ `Location`) | 400, 409 |
| GET·POST `/api/catalogos/marcas-tienda` | `IClienteCatalogos` | `{ nombre }` | `CatalogoRespuesta[]` / 201 `CatalogoRespuesta` | 400, 409 |
| GET·POST `/api/catalogos/regionales` | idem | idem | idem | idem |
| GET·POST `/api/catalogos/tipos-tienda` | idem | idem | idem | idem |
| GET·POST `/api/catalogos/responsables-visuales` | idem | idem | idem | idem |
| GET·POST `/api/tipos-espacio` | `IClienteTiposEspacio` | `{ nombre, categoria }` | `TipoEspacioRespuesta[]` / 201 | 400, 409 |
| GET `/api/niveles-jerarquia` | `IClienteArbolComercial.ObtenerNivelesAsync` | — | `NivelRespuesta[]` | — |
| GET `/api/arbol-comercial` | `ObtenerArbolAsync` | — | `NodoArbolRespuesta[]` (preorden) | — |
| GET `/api/conceptos-comerciales?nivelJerarquiaId=` | `ListarConceptosAsync` | — | `ConceptoRespuesta[]` | — |
| POST `/api/conceptos-comerciales` | `CrearConceptoAsync` | `CrearConceptoSolicitud` | 201 `ConceptoRespuesta` | 400, 409 |
| POST `/api/nodos-comerciales` | `CrearNodoAsync` | `CrearNodoSolicitud` | 201 `NodoArbolRespuesta` | 400, 404, 409 |
| PUT `/api/nodos-comerciales/{id}/porcentaje` | `ActualizarPorcentajeAsync` | `{ porcentajeParticipacion }` | 200 `NodoArbolRespuesta` | 400, 404 |
| GET `/api/carga/{codigoHanna}` | `IClienteCarga.CalcularAsync` | — | `CargaTiendaRespuesta` | 404 |
| GET `/api/reportes/consolidado?agruparPor=&nivelId=` | `IClienteReportes.ConsolidadoAsync` | — | `ConsolidadoRespuesta` | 400 |

### 4.3. Contratos (JSON camelCase; reflejar exactamente estos campos)
| Record | Campos |
|---|---|
| `CatalogoRespuesta` | `id`, `nombre` |
| `TipoEspacioRespuesta` | `id`, `nombre`, `categoria` (`mobiliario` \| `perimetral`) |
| `TiendaRespuesta` | `id`, `codigoHanna`, `nombre`, `marcaTienda`, `regional`, `tipoTienda` (`CatalogoRespuesta`), `responsableVisual?`, `espacios[{ tipoEspacio, categoria, cantidadEspacios }]`, `espaciosTotales` |
| `CrearTiendaSolicitud` | `codigoHanna`, `nombre`, `marcaTiendaId`, `regionalId`, `tipoTiendaId`, `responsableVisualId?`, `espacios[{ tipoEspacioId, cantidadEspacios }]` |
| `NivelRespuesta` | `id`, `nombre`, `profundidad`, `esHoja` |
| `ConceptoRespuesta` | `id`, `nombre`, `nivel` (`NivelRespuesta`), `unidadesPorEspacio?` |
| `NodoArbolRespuesta` | `nodoId`, `nodoPadreId?`, `conceptoComercialId`, `nombre`, `nivel`, `profundidad`, `porcentajeParticipacion`, `porcentajeAcumulado`, `ruta`, `unidadesPorEspacio?`, `esHoja` |
| `CrearConceptoSolicitud` | `nivelJerarquiaId`, `nombre`, `unidadesPorEspacio?` |
| `CrearNodoSolicitud` | `nodoPadreId?`, `conceptoComercialId`, `porcentajeParticipacion` (fraccion: 0.3 = 30%) |
| `CargaTiendaRespuesta` | `codigoHanna`, `nombreTienda`, `espaciosTotales`, `lineas[]`, `nodos[]`, `totalEspaciosAsignados`, `totalCarga`, `advertencias[]` |
| `LineaCargaRespuesta` | `nodoId`, `tipoProducto`, `ruta[{ nivel, nombre, porcentajeParticipacion }]`, `espaciosCalculados`, `unidadesPorEspacio`, `cargaCalculada`, `cargaTotal` |
| `NodoCalculadoRespuesta` | `nodoId`, `nodoPadreId?`, `nivel`, `nombre`, `porcentajeParticipacion`, `espaciosCalculados` |
| `ConsolidadoRespuesta` | `agruparPor`, `nivel?` (`NivelRespuesta`), `grupos[]`, `total` |
| `GrupoConsolidadoRespuesta` | `clave`, `nombre`, `cantidadTiendas`, `espaciosTotales?`, `espaciosAsignados`, `cargaTotal` |
| `ProblemaApi` | `type`, `title`, `status`, `detail?`, `errores?[]` (reglas de negocio), `errors?{campo: [mensajes]}` (validacion de campos; **claves en PascalCase**: `"CodigoHanna"`, `"Nombre"`, `"Espacios[0].CantidadEspacios"`) |

* Todos los numeros decimales (`espaciosCalculados`, `cargaCalculada`, porcentajes) son `decimal` en C#. **PROHIBIDO** `double`/`float`.
* `agruparPor` en la query: `tienda`, `marcaTienda`, `regional`, `nivel` (con `nivelId` obligatorio).

---

## 5. Pantallas
Menu lateral (`NavMenu`): Inicio, Tiendas, Arbol comercial, Reportes, Catalogos. Cada pagina muestra `Cargando` mientras
espera y `MensajeError` si la API falla (con boton "Reintentar").

### 5.1. Inicio — `/`
Tarjetas de acceso a cada seccion y un buscador "Calcular carga por codigo HANNA" que navega a `/tiendas/{codigoHanna}`.

### 5.2. Tiendas — `/tiendas`
* `QuickGrid` con: codigo HANNA, nombre, marca, regional, tipo, espacios totales; clic en la fila → detalle.
* Filtros por marca de tienda y regional (selects con `GET /api/catalogos/*`), aplicados con la query de la API.
* Boton "Nueva tienda".

### 5.3. Nueva tienda — `/tiendas/nueva`
* Formulario (§6.1): codigo HANNA, nombre, marca, regional, tipo de tienda, responsable visual (opcional) y lista dinamica
  de espacios (tipo de espacio + cantidad; agregar y quitar filas).
* Muestra **en vivo** los Espacios Totales (suma de cantidades; Regla 1) y cuantos tipos de mobiliario lleva ("2 de 3").
* 201 → navega a `/tiendas/{codigoHanna}` con un aviso "Tienda creada". 409 → error en el campo codigo HANNA.

### 5.4. Detalle de tienda y carga — `/tiendas/{codigoHanna:int}`
* Encabezado: datos de la tienda y tabla de espacios (mobiliario y perimetrales) con el total (`GET /api/tiendas/{hanna}`).
* **Pestaña "Carga"** (`GET /api/carga/{hanna}`):
  * `TablaCarga` con las columnas del enunciado: Marca de producto, Perfil (% part.), Tipo de producto (% part.),
    Espacios calculados, Unidades por espacio, Carga calculada, **Carga total**; fila final de totales
    (`totalEspaciosAsignados`, `totalCarga`). Las columnas de marca y perfil salen de `ruta[]` por **profundidad del nivel**
    (no por indice fijo).
  * Para HANNA **5503** debe verse exactamente: 819, 670, 482, 469, 288, 238 — total **6,6000 espacios y 2.966 unidades**.
  * Advertencias (`advertencias[]`) en una alerta amarilla, si hay.
  * Seccion plegable "Desglose por nivel" con `ArbolNodos` usando `nodos[]` (espacios de cada nodo).
* 404 → mensaje "No existe una tienda con el codigo HANNA {n}" y enlace al listado.

### 5.5. Arbol comercial — `/arbol`
* `ArbolNodos` con `GET /api/arbol-comercial` (ya viene en preorden): sangria por `profundidad`, nombre, nivel,
  % de participacion, % acumulado y, en las hojas, unidades por espacio. Expandir/contraer por rama.
* Panel lateral de niveles (`GET /api/niveles-jerarquia`) marcando el nivel hoja.
* Acciones:
  * **Editar porcentaje** de un nodo (modal, §6.4) → `PUT /api/nodos-comerciales/{id}/porcentaje` → recargar el arbol.
  * **Nuevo concepto** (§6.3) → `POST /api/conceptos-comerciales`.
  * **Agregar nodo** bajo el nodo seleccionado (o como raiz): concepto del nivel siguiente
    (`GET /api/conceptos-comerciales?nivelJerarquiaId=`) y porcentaje (§6.4) → `POST /api/nodos-comerciales`.

### 5.6. Reportes — `/reportes`
* Selector `agruparPor` (Tienda, Marca de tienda, Regional, Nivel del arbol); con "Nivel del arbol" se habilita el select
  de nivel (`GET /api/niveles-jerarquia`; por defecto nivel 3 = Marca de producto).
* `QuickGrid` con clave, nombre, cantidad de tiendas, espacios totales (oculta si es `null`), espacios asignados, carga
  total, y la fila **Total general** destacada.

### 5.7. Catalogos — `/catalogos`
Pestañas: Marcas de tienda, Regionales, Tipos de tienda, Responsables visuales, Tipos de espacio. Cada una lista
(`GET`) y tiene un formulario de alta (§6.2) que agrega el elemento a la lista sin recargar la pagina.

### 5.8. Componentes compartidos (`Componentes/Compartidos/`)
| Componente | Responsabilidad |
|---|---|
| `TablaCarga` | Tabla de lineas + totales de `CargaTiendaRespuesta` (parametro de entrada; sin llamadas HTTP). |
| `ArbolNodos` | Lista jerarquica indentada generica por `profundidad` (para `NodoArbolRespuesta` y `NodoCalculadoRespuesta`). |
| `MensajeError` | Muestra una `ExcepcionApi`: titulo, detalle y lista de `Errores`; boton opcional "Reintentar". |
| `ResumenErroresApi` | Dentro de un `EditForm`: agrega los `ErroresPorCampo` de la API al `ValidationMessageStore` (mensajes junto al campo). |
| `Cargando` | Indicador de carga (spinner de Bootstrap). |
| `SelectorCatalogo` | `InputSelect` alimentado por un `IReadOnlyList<CatalogoRespuesta>`, con opcion vacia. |

---

## 6. Validacion de Entradas (formularios)
Cada formulario usa `EditForm` con un **modelo de formulario** propio (`Formularios/`), `DataAnnotationsValidator`,
`ValidationMessage` debajo de cada campo y `ValidationSummary` arriba. El boton de enviar se deshabilita mientras se
envia (evita dobles envios). Si la API responde 400/409, `ResumenErroresApi` coloca `errors{campo}` en el campo
correspondiente (mapeo de nombres PascalCase de la API al modelo del formulario) y `errores[]`/`detail` en el resumen.

### 6.1. Nueva tienda (`FormularioTienda`)
| Campo | Reglas |
|---|---|
| Codigo HANNA | Requerido, entero `[Range(1, int.MaxValue)]` |
| Nombre | `[Required]`, `[StringLength(150)]`, sin espacios al inicio/fin (se recorta al enviar) |
| Marca, Regional, Tipo de tienda | Requeridos (`[Range(1, int.MaxValue)]` sobre el id seleccionado) |
| Responsable visual | Opcional |
| Espacios | Al menos 1 fila; cada fila: tipo de espacio requerido y cantidad entera `>= 0` |
| Regla de forma `[MaximoMobiliario]` (validador propio) | Maximo **3** tipos de categoria `mobiliario`; los `perimetral` no cuentan |
| Regla de forma `[SinTiposRepetidos]` (validador propio) | Un tipo de espacio no puede repetirse |

### 6.2. Catalogos y tipos de espacio
| Formulario | Reglas |
|---|---|
| Marca de tienda, Regional, Tipo de tienda | `[Required]`, `[StringLength(50)]` |
| Responsable visual | `[Required]`, `[StringLength(100)]` |
| Tipo de espacio | Nombre `[Required]`, `[StringLength(50)]`; categoria requerida (`mobiliario` o `perimetral`) |

### 6.3. Nuevo concepto (`FormularioConcepto`)
| Campo | Reglas |
|---|---|
| Nivel | Requerido |
| Nombre | `[Required]`, `[StringLength(100)]` |
| Unidades por espacio | **Obligatorio y > 0 solo si el nivel elegido es hoja** (`esHoja`); en otro nivel el campo se oculta y se envia `null` (validador condicional propio) |

### 6.4. Porcentaje (nuevo nodo y editar porcentaje)
* El usuario escribe el porcentaje **en %** (ej. `30` o `30,5`); el front envia la **fraccion** (`0.30`, `0.305`).
* Reglas: requerido, mayor que 0 y menor o igual a 100, maximo 4 decimales en % (la BD guarda 6 decimales de fraccion).
* Como ayuda (no bloqueante), mostrar la suma actual de los hermanos y lo que queda disponible hasta 100%;
  la validacion definitiva la hace la API (400 con `errores: ["La suma de porcentajes de los nodos hermanos (1.010000) supera 1."]`).

---

## 7. Diseno Visual: Paleta de Colores y Contraste

### 7.1. Paleta base (orientativa)
La identidad visual **se basa** en dos colores: **#014B43** (verde azulado oscuro) y **#F9ECE5** (crema). La paleta de
abajo es la referencia: se pueden ajustar tonos o agregar otros **derivados** de estos dos, siempre que cumplan §7.3.
Lo unico **no negociable** es el contraste.

| Token CSS | Hex | Uso |
|---|---|---|
| `--color-primario` | `#014B43` | Botones primarios, barra de navegacion, encabezados de tabla, enlaces |
| `--color-primario-oscuro` | `#01362F` | Hover / activo del primario |
| `--color-primario-suave` | `#DCEEEA` | Fila seleccionada, chips, fondos de enfasis |
| `--color-fondo` | `#F9ECE5` | Fondo de la aplicacion |
| `--color-fondo-alterno` | `#F1DED3` | Filas alternas y hover de filas |
| `--color-superficie` | `#FFFFFF` | Tarjetas, formularios, modales, tablas |
| `--color-texto` | `#1C2B29` | Texto principal |
| `--color-texto-secundario` | `#4D5C59` | Ayudas y etiquetas secundarias |
| `--color-acento` | `#9A4321` | Acento calido (terracota derivado del crema): boton secundario, resaltes |
| `--color-borde-campo` | `#7A8A86` | Borde de inputs y selects |
| `--color-foco` | `#B4531F` | Anillo de foco (navegacion con teclado) |
| `--color-exito-texto` / `--color-exito-fondo` | `#14532D` / `#DCF2E3` | Alertas y badges de exito |
| `--color-advertencia-texto` / `--color-advertencia-fondo` | `#6B4500` / `#FFF1CC` | Advertencias (ej. `advertencias[]` de la carga) |
| `--color-error-texto` / `--color-error-fondo` | `#8A1C1C` / `#FBE3E1` | Errores de la API y de validacion |
| `--color-info-texto` / `--color-info-fondo` | `#0B4F6C` / `#DDEFF7` | Mensajes informativos |
| `--color-deshabilitado-texto` / `--color-deshabilitado-fondo` | `#5B6664` / `#E9E2DD` | Campos y botones deshabilitados |
| `--color-placeholder` | `#5E6B68` | Texto de ejemplo en campos |

### 7.2. Pares verificados (WCAG 2.1, luminancia relativa)
| Texto / elemento | Fondo | Ratio | Minimo | Uso |
|---|---|---|---|---|
| `#F9ECE5` | `#014B43` | **8.69:1** | 4.5 | Texto crema sobre la barra de navegacion |
| `#FFFFFF` | `#014B43` | 10.05:1 | 4.5 | Texto de botones primarios y encabezados de tabla |
| `#FFFFFF` | `#01362F` | 13.38:1 | 4.5 | Boton primario en hover |
| `#1C2B29` | `#F9ECE5` | 12.72:1 | 4.5 | Texto principal sobre el fondo |
| `#1C2B29` | `#FFFFFF` | 14.72:1 | 4.5 | Texto principal sobre superficies |
| `#1C2B29` | `#F1DED3` | 11.30:1 | 4.5 | Filas alternas / hover |
| `#1C2B29` | `#DCEEEA` | 12.24:1 | 4.5 | Fila seleccionada |
| `#4D5C59` | `#F9ECE5` | 6.07:1 | 4.5 | Texto secundario sobre el fondo |
| `#4D5C59` | `#FFFFFF` | 7.02:1 | 4.5 | Texto secundario sobre superficies |
| `#014B43` | `#F9ECE5` | 8.69:1 | 4.5 | Enlaces sobre el fondo |
| `#014B43` | `#FFFFFF` | 10.05:1 | 4.5 | Enlaces sobre superficies |
| `#014B43` | `#DCEEEA` | 8.36:1 | 4.5 | Texto primario sobre chips |
| `#9A4321` | `#FFFFFF` | 6.56:1 | 4.5 | Acento como texto sobre superficies |
| `#9A4321` | `#F9ECE5` | 5.68:1 | 4.5 | Acento como texto sobre el fondo |
| `#FFFFFF` | `#9A4321` | 6.56:1 | 4.5 | Texto de boton secundario |
| `#14532D` | `#DCF2E3` | 7.75:1 | 4.5 | Alerta de exito |
| `#6B4500` | `#FFF1CC` | 7.55:1 | 4.5 | Alerta de advertencia |
| `#8A1C1C` | `#FBE3E1` | 7.59:1 | 4.5 | Alerta de error |
| `#8A1C1C` | `#FFFFFF` | 9.28:1 | 4.5 | Mensaje de validacion bajo el campo |
| `#0B4F6C` | `#DDEFF7` | 7.56:1 | 4.5 | Alerta informativa |
| `#5B6664` | `#E9E2DD` | 4.64:1 | 4.5 | Campo / boton deshabilitado |
| `#5E6B68` | `#FFFFFF` | 5.56:1 | 4.5 | Placeholder |
| `#7A8A86` | `#FFFFFF` | 3.62:1 | 3.0 | Borde de campo sobre superficie (UI) |
| `#7A8A86` | `#F9ECE5` | 3.13:1 | 3.0 | Borde de campo sobre el fondo (UI) |
| `#B4531F` | `#F9ECE5` | 4.33:1 | 3.0 | Anillo de foco sobre el fondo (UI) |
| `#B4531F` | `#FFFFFF` | 5.00:1 | 3.0 | Anillo de foco sobre superficies (UI) |
| `#014B43` | `#F1DED3` | 7.72:1 | 4.5 | Enlaces y botones de contorno en filas alternas (agregado por el front) |
| `#4D5C59` | `#F1DED3` | 5.39:1 | 4.5 | Texto secundario en filas alternas (agregado) |
| `#4D5C59` | `#DCEEEA` | 5.84:1 | 4.5 | Texto secundario en fila seleccionada o de total (agregado) |
| `#8A1C1C` | `#F9ECE5` | 8.02:1 | 4.5 | Mensaje de validacion sobre el fondo (agregado) |
| `#014B43` | `#FFF1CC` | 8.96:1 | 4.5 | Boton de contorno en la barra de error de Blazor (agregado) |
| `#F9ECE5` | `#014B43` | 8.69:1 | 3.0 | Anillo de foco en la barra lateral (UI, agregado) |
| `#B4531F` | `#F1DED3` | 3.84:1 | 3.0 | Anillo de foco en filas alternas (UI, agregado) |
| `#B4531F` | `#DCEEEA` | 4.16:1 | 3.0 | Anillo de foco en fila seleccionada (UI, agregado) |
| `#B4531F` | `#FBE3E1` | 4.09:1 | 3.0 | Anillo de foco dentro de la alerta de error (UI, agregado) |
| `#B4531F` | `#FFF1CC` | 4.46:1 | 3.0 | Anillo de foco en la barra de error de Blazor (UI, agregado) |
| `#9A4321` | `#F8E6DC` | 5.42:1 | 4.5 | Chips terracota: perimetral, nivel hoja (`--color-acento-suave`, agregado) |
| `#F9ECE5` | `#01362F` | 11.57:1 | 4.5 | Menú lateral en hover e ícono de la cifra destacada (agregado) |

`--color-separador` (#EAD9CF) es solo decorativo (líneas entre filas y bordes de tarjetas): no transmite información ni
delimita un control, por eso no tiene mínimo de contraste (WCAG §1.4.11 no aplica). Sombras y radios: `--sombra-*` y
`--radio-*`, también en `tema.css`.

Formula (WCAG 2.1): `ratio = (L1 + 0.05) / (L2 + 0.05)`, con `L` = luminancia relativa
`0.2126·R + 0.7152·G + 0.0722·B` y cada canal linealizado (`c/12.92` si `c ≤ 0.04045`, si no `((c+0.055)/1.055)^2.4`).

### 7.3. Reglas de contraste (STRICT)
* **Todo texto ≥ 4.5:1** contra su fondo real (WCAG 2.1 §1.4.3, nivel AA), **sin importar el tamano de letra**. Incluye
  placeholder, campos y botones deshabilitados, enlaces (normal, hover, visitado), botones en todos sus estados, badges,
  alertas, tooltips, encabezados de tabla, filas alternas / hover / seleccionadas y mensajes de validacion.
* **Componentes de UI e indicadores de foco ≥ 3:1** contra lo que tienen al lado (WCAG §1.4.11): bordes de campos,
  checkboxes, radios, separadores interactivos y el anillo de foco. El foco debe verse siempre con teclado: **prohibido**
  `outline: none` sin un reemplazo que cumpla.
* Con transparencias (`rgba`, `opacity`), el ratio se mide sobre el **color compuesto final**.
* Prohibido texto sobre imagenes o degradados sin un fondo solido que cumpla.
* La informacion **no depende solo del color**: estados con texto o icono ("Error", "Advertencia", "Tienda creada").
* **Libertad de paleta con control:** un color nuevo solo se usa si su par texto/fondo se agrega a la tabla de §7.2 y a
  la prueba de §9, y cumple el minimo.

### 7.4. Implementacion
* Los tokens se declaran **una sola vez** como CSS custom properties en `Abastecimiento.Front/wwwroot/css/tema.css`
  (`:root { --color-primario: #014B43; ... }`), cargado despues de Bootstrap en `App.razor`.
* `tema.css` sobreescribe las variables de Bootstrap 5 con los tokens: `--bs-body-bg`, `--bs-body-color`,
  `--bs-secondary-color`, `--bs-primary` y `--bs-primary-rgb`, `--bs-link-color` y `--bs-link-hover-color`,
  `--bs-border-color`, `--bs-focus-ring-color`, y las variables de `.btn-primary`, `.btn-outline-primary`, `.table`,
  `.alert-success`, `.alert-warning`, `.alert-danger` y `.alert-info`.
* **Prohibido** escribir colores en hex, `rgb()` o nombres (`red`) en `.razor`, en estilos inline o en el CSS aislado de
  los componentes: siempre `var(--color-...)`.

---

## 8. Configuracion y Docker
* `appsettings.json`: `"Api": { "UrlBase": "http://localhost:5080" }` (API con `dotnet run`).
* `Properties/launchSettings.json`: perfil `http` en `http://localhost:5090`, `ASPNETCORE_ENVIRONMENT=Development`.
* `front/Dockerfile`: multi-stage `mcr.microsoft.com/dotnet/sdk:8.0` → `mcr.microsoft.com/dotnet/aspnet:8.0`, usuario `app`,
  `ASPNETCORE_URLS=http://+:8080`. Contexto de build: la **raiz del repo** (por `Directory.Build.props`), copiando solo
  `Directory.Build.props` y `front/`.
* `docker-compose.yml` (reemplazar el servicio `front` comentado):
  ```yaml
  front:
    build:
      context: .
      dockerfile: front/Dockerfile
    container_name: abastecimiento-front
    depends_on:
      - api
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      Api__UrlBase: "http://api:8080"
    ports:
      - "3000:8080"
    restart: on-failure
  ```
* **Contenedor e imagen propios**, pero el **levantamiento global se conserva**: `docker compose up -d --build` levanta
  db + api + front. Solo el front: `docker compose up -d --build front` (arranca tambien `api` y `db` si hacen falta).
* `.dockerignore`: el existente ya excluye `**/bin`, `**/obj` y `tests/` (solo el de la raiz); agregar `front/tests/`.
* Con `InteractiveServer` las llamadas son servidor a servidor: **no** se requiere CORS. No tocar la configuracion CORS de la API.

---

## 9. Pruebas (`front/tests/Abastecimiento.Front.Pruebas`)
xUnit + bUnit (sin otras librerias). Sin API real: clientes con `HttpMessageHandler` falso y componentes con clientes falsos.

| Tipo | Casos |
|---|---|
| Clientes | Deserializa camelCase y enums en texto (`"mobiliario"`); envia `agruparPor=marcaTienda`; 404 problem+json → `ExcepcionApi` (Estado 404, Detalle); 400 con `errors{}` → `ErroresPorCampo`; 400 con `errores[]` → `Errores`; error de red → Estado 0 |
| `TablaCarga` (bUnit) | Con la respuesta de 5503: 6 filas, cargas 819/670/482/469/288/238, fila total 2.966 y 6,6000; marca y perfil tomados de `ruta[]` |
| Formulario de tienda (bUnit) | 4 tipos de mobiliario → mensaje de `[MaximoMobiliario]` y no se envia; tipo repetido → mensaje; formulario valido → llama a `CrearAsync` una vez |
| Formulario de concepto | Nivel hoja sin unidades → invalido; nivel no hoja oculta el campo y envia `null` |
| Porcentaje | `30` → envia `0.30`; `0` y `101` → invalidos |
| `MensajeError` / `ResumenErroresApi` | Muestran titulo, detalle y errores; `errors["Nombre"]` aparece junto al campo Nombre |
| Contraste del tema (`ContrasteTemaPruebas`) | Lee `wwwroot/css/tema.css`, extrae los tokens `--color-*` y, con la formula de §7.2, verifica **cada par** de la tabla de §7.2 contra su minimo (4.5 texto, 3.0 UI). Falla si un token del par no existe o no cumple. Ejemplo obligatorio: `--color-fondo` (#F9ECE5) vs `--color-primario` (#014B43) = 8.69:1 |
| Sin colores sueltos | Recorre los `.razor`, `.razor.css` y `.cs` de `Componentes/` y falla si encuentra un color en hex, `rgb(`/`rgba(` o `hsl(` (todos deben venir de `var(--color-...)`) |

Comando: `dotnet test front/Abastecimiento.Front.sln`. Ademas, agregar al servicio `pruebas` del `docker-compose.yml`
la ejecucion de `dotnet test front/Abastecimiento.Front.sln` despues de la solucion del backend (el comando unico
`docker compose --profile pruebas run --rm pruebas` debe correr ambas).

---

## 10. Documentacion
Agregar al `README.md` una seccion **"Frontend"**: como levantarlo (Docker → `http://localhost:3000`; local →
`dotnet run --project front/Abastecimiento.Front --launch-profile http` → `http://localhost:5090` con la API en 5080),
mapa de pantallas → endpoints y como ejecutar sus pruebas. Comentarios XML en clientes, contratos y validadores propios.

---

## 11. Supuestos
* **A. Sin autenticacion:** la API es publica en esta fase; el front no maneja login ni tokens.
* **B. Solo crear y consultar:** no hay edicion ni eliminacion de tiendas, conceptos o nodos (excepto el porcentaje del nodo).
* **C. Arbol global:** el mismo arbol aplica a todas las tiendas (supuesto A del spec de aplicacion).
* **D. Formato:** los numeros se formatean en el front (4 decimales en espacios); la API entrega los decimales completos.
* **E. "Marca" en reportes:** marca de tienda = `agruparPor=marcaTienda`; marca de producto = `agruparPor=nivel&nivelId=3`.

---

## 12. Criterios de Aceptacion (Definition of Done)
1. `docker compose up -d --build` deja **db + api + front** arriba y el front responde en `http://localhost:3000`.
2. `/tiendas/5503` muestra la tabla del enunciado: 819, 670, 482, 469, 288, 238 y el total **6,6000 espacios / 2.966 unidades**.
3. Se puede crear una tienda, un catalogo, un tipo de espacio, un concepto y un nodo, y editar un porcentaje; los errores
   400/404/409 de la API se ven legibles (en el campo o en el resumen), nunca como pantalla en blanco.
4. Los validadores de §6 impiden enviar datos invalidos (4 mobiliarios, tipo repetido, porcentaje fuera de (0, 100], etc.).
5. Reportes funcionan con los 4 criterios de agrupacion y muestran el total general.
6. `dotnet build front/Abastecimiento.Front.sln` sin warnings y `dotnet test front/Abastecimiento.Front.sln` en verde.
7. El proyecto del front **no** referencia ningun proyecto del backend y ningun `.razor` contiene logica en `@code`.
8. El `README.md` incluye la seccion "Frontend".
9. La UI usa la paleta de §7 (basada en #014B43 y #F9ECE5) y **todo texto cumple ≥ 4.5:1 y toda UI/foco ≥ 3:1** contra su
   fondo: la prueba `ContrasteTemaPruebas` pasa y no hay colores fuera de `tema.css`.
