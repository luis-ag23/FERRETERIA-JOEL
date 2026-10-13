# Guía de Defensa — Ferretería Joel (Arquitectura, Patrones, SOLID, Async)

Guía de estudio para responder preguntas del panel. Cada respuesta sigue la
fórmula: **concepto → ejemplo en el proyecto → ubicación exacta**.

> Estado actual del código: solución .NET 10, **5 proyectos por capa**,
> arquitectura **hexagonal (puertos y adaptadores)** dentro de un **monolito
> por responsabilidad técnica**, **3 fachadas** en el módulo de ventas y
> **todo el flujo asíncrono** (`async`/`await` de extremo a extremo).

---

## 1. ¿Qué arquitectura seguimos y por qué?

**Respuesta corta:**
Es un **monolito** (un único despliegue: la app web) organizado internamente
como **arquitectura hexagonal (puertos y adaptadores)**, con el código dividido
en **proyectos por responsabilidad técnica**.

- **Monolito** → un solo ejecutable (el proyecto web `FERRETERIA  Joel`) que
  contiene todos los módulos: catálogo, seguridad, ventas.
- **Hexagonal** → el núcleo de negocio (**Dominio**) define **puertos**
  (interfaces) y expone **casos de uso** (**Aplicación**); los detalles de
  infraestructura (MySQL, claims HTTP, hash Argon2) se conectan como
  **adaptadores** (**Infraestructura** y **Web**). El núcleo no conoce MySQL ni
  ASP.NET.

### Mapa de la solución (proyectos)

| Proyecto | Responsabilidad | Depende de |
|----------|-----------------|------------|
| `FERRETERIA__Joel.Dominio` | Núcleo: `Entidades/`, `Puertos/`, `Validaciones/`, `ConfiguracionValidacion/`, `Constantes/`, `Utilidades/` | (nada) |
| `FERRETERIA__Joel.Aplicacion` | Casos de uso: `Servicios/` (servicios + **fachadas**), `Modelos/` (DTOs) y `Politicas/` (restricciones de descuento) | Dominio |
| `FERRETERIA__Joel.Infraestructura` | Adaptadores de salida: `Repositorios/` (MySQL), `Conexion/`, `Factories/`, `Seguridad/` (Argon2) y `MySqlUnidadDeTrabajo` | Dominio |
| `FERRETERIA  Joel` (Web) | Adaptador de entrada: `Pages/` (Razor Pages), `Servicios/ContextoUsuarioHttp`, `Helpers/` y `Program.cs` (**Composition Root**) | Dominio + Aplicación + Infraestructura |
| `FERRETERIA__Joel.Pruebas` | Pruebas unitarias e integración (xUnit) | Todas |

**¿Por qué esta separación?**
- Cada capa tiene **una sola razón para cambiar** (responsabilidad única).
- La UI **no conoce** MySQL: depende de **puertos** y de **casos de uso**.
- Cambiar de base de datos = cambiar `Repositorios/` y los `Factories/`; el
  Dominio, la Aplicación y las páginas no se tocan.
- Es **testeable**: los `Puertos` se sustituyen por fakes en memoria
  (`FERRETERIA__Joel.Pruebas/Compartido/FakesVenta.cs`).

**Regla de dependencias (para dibujar en la pizarra):**
```
Web ─▶ Aplicacion ─▶ Dominio ◀─ Infraestructura
```
Las flechas **siempre apuntan hacia el Dominio**. Infraestructura implementa los
puertos del Dominio (inversión de dependencias); Web los compone y los consume.

---

## 2. Puertos y adaptadores (hexagonal) con ejemplos exactos

**Puertos (definidos en el Dominio):** `FERRETERIA__Joel.Dominio/Puertos/`

| Puerto | Rol |
|--------|-----|
| `IRepository<T>` | Contrato base: `Task<List<T>> ObtenerTodosAsync()`, `Task<int> InsertarAsync(T)` |
| `IGestionarRepository<T>` | Extiende el anterior: `ActualizarAsync`, `CambiarEstadoAsync`, `MostrarActivosAsync` |
| `IUnidadDeTrabajo` | Transacción (Unit of Work) |
| `IHasheadorContrasena` | Hash/verificación de contraseñas |
| `IContextoUsuario` | Identidad del usuario en sesión |

> Ya **no** existen puertos específicos por entidad (`IVentaRepository`,
> `IClienteRepository`, …). Los repositorios concretos implementan **solo** los
> dos contratos genéricos; las operaciones que no encajan en un CRUD
> (folios/venta, idempotencia, anulación, stock, precio vigente, bitácora) se
> exponen como **métodos concretos** e se inyectan directamente por DI.

**Factory Method (mecanismo, ya no en el Dominio):**
`FERRETERIA__Joel.Infraestructura/Factories/RepositoryCreator.cs` define
`RepositoryCreator<TContrato>` abstracto; hay un creador por contrato genérico
(`ProductoRepositoryCreator`, `CategoriaRepositoryCreator`, `MarcaRepositoryCreator`,
`ProveedorRepositoryCreator`, `EmpleadoRepositoryCreator`).

**Adaptadores (implementados en Infraestructura/Web):**
- `MySql*Repository` implementan `IRepository<T>` / `IGestionarRepository<T>`
  (los métodos de operaciones especiales se marcan `virtual` para poder
  sustituirlos en pruebas).
- Los repositorios concretos de operación especial (`MySqlVentaRepository`,
  `MySqlDetalleVentaRepository`, `MySqlInventarioRepository`,
  `MySqlHistoricoPrecioRepository`, `MySqlBitacoraRepository`) se inyectan como
  clase concreta.
- `MySqlUnidadDeTrabajo` implementa `IUnidadDeTrabajo`.
- `Argon2HasheadorContrasena` implementa `IHasheadorContrasena`.
- `ContextoUsuarioHttp` (en Web) implementa `IContextoUsuario` leyendo las
  *claims* de la cookie.

**Clave de defensa:** *"El Dominio declara el 'qué' (puertos); la
Infraestructura aporta el 'cómo' (adaptadores). Por eso puedo cambiar MySQL por
otro motor sin tocar la lógica de negocio."*

---

## 3. ¿Qué es inyección de dependencias (DI) y dónde la hacemos?

**Respuesta corta:** DI = entregar a cada clase sus dependencias por
**constructor** en lugar de crearlas con `new`. En el proyecto:

**Paso 1 — Composición (`FERRETERIA  Joel/Program.cs`)** — es el *Composition
Root*, único lugar donde se conocen las implementaciones concretas:

```csharp
// Puertos base
builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
builder.Services.AddScoped<IConexionTransaccional, MySqlConexionTransaccional>();
builder.Services.AddScoped<IUnidadDeTrabajo, MySqlUnidadDeTrabajo>();

// Factory Method: un RepositoryCreator<> por contrato genérico
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Producto>>, ProductoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Categoria>>, CategoriaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Empleado>>, EmpleadoRepositoryCreator>();
// ...

// Repositorios de operación especial (se inyectan como clase concreta)
builder.Services.AddScoped<MySqlVentaRepository>();
builder.Services.AddScoped<MySqlInventarioRepository>();
builder.Services.AddScoped<MySqlClienteRepository>();

// Casos de uso
builder.Services.AddScoped<ServicioProducto>();
builder.Services.AddScoped<ServicioInicioSesion>();

// Fachadas del módulo de ventas (patrón Facade)
builder.Services.AddScoped<FachadaActualizacionStock>();
builder.Services.AddScoped<FachadaVenta>();
builder.Services.AddScoped<FachadaAnulacion>();

// Adaptadores
builder.Services.AddScoped<IContextoUsuario, ContextoUsuarioHttp>();
builder.Services.AddSingleton<IHasheadorContrasena, Argon2HasheadorContrasena>();
```

- `AddScoped` = una instancia por request HTTP; `AddSingleton` = una para toda
  la app (el hasheador no tiene estado, la fábrica de conexión tampoco).

**Paso 2 — Consumo (constructor del Page Model).** Ejemplo
`FERRETERIA  Joel/Pages/Venta.cshtml.cs`: el modelo recibe **solo** la fachada
de venta y el `ILogger`; nunca crea repositorios ni conexiones.

**¿Por qué no `new MySqlProductoRepository()` dentro de la página?** Porque
acoplaría la UI a MySQL (violaría DIP) y haría imposible probar sin base de
datos.

---

## 4. Patrones de diseño usados (para la rúbrica de "Patrones")

Se piden **al menos 3**, con **Facade obligatorio**. Aquí hay 5:

### 4.1 Facade (fachadas del módulo de ventas) — obligatorio
`FERRETERIA__Joel.Aplicacion/Servicios/`

- **`FachadaVenta`** → registro de venta: `BuscarClientesAsync`,
  `BuscarProductosAsync`, `ObtenerProductoAsync`, `CrearClienteAsync`,
  `CalcularLineaAsync`, `CalcularResumen`, `RegistrarVentaAsync`,
  `ObtenerComprobanteAsync`. Consume la fachada de stock y `ServicioCliente`.
- **`FachadaAnulacion`** → historial y anulación: `ListarVentasListadoAsync`,
  `ObtenerVentaAsync`, `AnularVentaAsync`. También delega el stock.
- **`FachadaActualizacionStock`** → inventario: `ListarActivosAsync`,
  `ObtenerParaVentaAsync`, `ObtenerPorIdsAsync`, `DescontarAsync`,
  `ReponerAsync`, `MapearLineasAsync`.

**Por qué:** cada **pantalla consume UNA sola fachada**. `Venta.cshtml.cs` usa
`FachadaVenta`; `Ventas.cshtml.cs` usa `FachadaAnulacion`. La UI queda
desacoplada de repositorios, transacciones y SQL. La fachada delgada crea el
subsistema de stock reutilizable por venta y anulación.

### 4.2 Factory Method
- **`RepositoryCreator<TContrato>`**
  (`Infraestructura/Factories/RepositoryCreator.cs`) es una fábrica abstracta:
  `TContrato CreateRepository()`.
- Cada creador concreto vive también en `Infraestructura/Factories/`
  (`ProductoRepositoryCreator`, `CategoriaRepositoryCreator`, `MarcaRepositoryCreator`,
  `ProveedorRepositoryCreator`, `EmpleadoRepositoryCreator`) y decide qué
  implementación entregar. Los servicios piden, p. ej.,
  `RepositoryCreator<IGestionarRepository<Producto>>` y obtienen un repositorio
  **ligado a la transacción ambiente**.

### 4.3 Strategy
- **`Politicas/IPoliticaDescuento`** con estrategias concretas
  `DescuentoPorMonto` y `DescuentoPorCantidad`.
- **`Politicas/SelectorDescuento`** es el contexto: evalúa todas las políticas y
  aplica la que otorgue el **mayor beneficio**; si ninguna aplica, descuento 0.
- Agregar una política nueva = agregar una clase `IPoliticaDescuento`, sin tocar
  el selector (Open/Closed).

### 4.4 Repository
- Los `MySql*Repository` encapsulan el SQL/ADO.NET; la lógica no ve tablas, solo
  entidades. `MySqlProductoRepository`, `MySqlVentaRepository`, etc.

### 4.5 Unit of Work + transacción
- **`IUnidadDeTrabajo`** (Dominio) con `IniciarAsync` / `ConfirmarAsync` /
  `RevertirAsync`; implementada por **`MySqlUnidadDeTrabajo`** sobre
  `IConexionTransaccional` (`MySqlConexionTransaccional`, que abre una conexión y
  una transacción **async**: `BeginTransactionAsync`, `CommitAsync`,
  `RollbackAsync`).

---

## 5. Módulo de ventas: transacción, anulación y stock

**Registro (`FachadaVenta.RegistrarVentaAsync`):** abre la unidad de trabajo y,
en **una sola transacción**:
1. **n inserts** en `detalle_venta` (uno por línea).
2. **n updates** de `producto.CantidadStock` (descuento vía
   `FachadaActualizacionStock.DescontarAsync`).
3. `ConfirmarAsync`. Si falla algo (p. ej. stock insuficiente), `RevertirAsync`:
   **nada** queda persistido.
4. Clave de **idempotencia**: repetir la misma venta no duplica.

**Anulación (`FachadaAnulacion.AnularVentaAsync`)** — borrado **lógico**:
1. `UPDATE venta SET EstadoVenta = 'Anulada'` (+ motivo, empleado y fecha).
2. **`detalle_venta` NO se toca**: se usa como fuente para reponer.
3. **n updates** de reposición de stock (uno por línea, vía
   `FachadaActualizacionStock.ReponerAsync`).
4. Todo en la misma transacción: `ConfirmarAsync` / `RevertirAsync`.

> La anulación está disponible para el rol **`EmpleadoVenta`** (ver
> `RolesSistema`, `FachadaAnulacion` y el botón/handler en
> `FERRETERIA  Joel/Pages/Ventas.cshtml(.cs)`). El `Administrador` solo ve las
> ventas, no las anula.

**Comprobante:** `FachadaVenta.ObtenerComprobanteAsync` arma el comprobante
(cliente, empleado responsable y líneas con código/nombre del producto) y
`Utilidades/NumeroALetras` convierte el total a letras.

---

## 6. Aplicación de cada principio SOLID (con ejemplos)

### S — Single Responsibility
Cada clase/archivo hace **una cosa**:
- Persistir → `Infraestructura/Repositorios/MySql*Repository.cs`
- Validar → `Dominio/Validaciones/*` (`ProductoValidaciones`, `MarcaValidaciones`, …)
- Orquestar HTTP → Page Model (`Pages/*.cshtml.cs`)
- Caso de uso/fachada → `Aplicacion/Servicios/*`
- Mapear fila→entidad → método privado `Mapear*` dentro de cada repositorio.

### O — Open/Closed
- Nuevo descuento → nueva clase `IPoliticaDescuento`, sin tocar
  `SelectorDescuento`.
- Nuevo motor de BD → nueva implementación del puerto + ajuste **solo** en
  `Program.cs`. Ninguna página se modifica.

### L — Liskov
Los fakes de las pruebas (`FERRETERIA__Joel.Pruebas/Compartido/FakesVenta.cs`)
**heredan de los repositorios concretos** (`MySql*Repository`, pasando `null!` al
constructor) y sobrescriben sus métodos `virtual`: respetan el mismo contrato y
las fachadas funcionan igual con reales o con fakes.

### I — Interface Segregation
Puertos pequeños y específicos: `IRepository<T>` (2 métodos) e
`IGestionarRepository<T>` (3 más). Ningún consumidor recibe métodos que no usa;
las operaciones especiales no contaminan el contrato genérico porque se exponen
como métodos del repositorio concreto.

### D — Dependency Inversion
Alto nivel (Aplicación/Web) depende de **abstracciones** (`IGestionarRepository<T>`,
`IContextoUsuario`, `IUnidadDeTrabajo`, `RepositoryCreator<T>`) definidas o
usadas por el núcleo; bajo nivel (Infraestructura) **implementa** esos contratos
y los repositorios concretos. El registro vive en `Program.cs`.

---

## 7. ¿Qué es Clean Code y dónde se aplica?

Clean Code = código **legible, sin duplicación y con intención clara**.

| Práctica | Ejemplo (ubicación) |
|----------|---------------------|
| Nombres con intención | `ReponerAsync`, `DescontarAsync`, `MapearLineasAsync`, `Coincide` |
| Métodos pequeños / una tarea | `FachadaVenta` separa búsqueda, cálculo y registro |
| Sin duplicación (DRY) | `MapearLineasAsync` centraliza producto→línea en una sola fachada de stock |
| Asincronía consistente | `async`/`await` en puertos, repos, servicios y páginas |
| Lectura tipada del reader | `GetInt32`, `GetDecimal`, `GetDateTime` (sin `ToString()`) |
| Manejo seguro de nulos | `reader["X"] == DBNull.Value ? null : ...` |
| Recursos liberados | `using` / `await using` de conexión, comando y reader |
| Errores manejados | `try/catch` + `ILogger.LogError` + mensaje genérico al usuario |
| Constantes | `const string query = "..."` y `RolesSistema.*` |

---

## 8. Roles y autorización (se piden ≥2 roles diferenciados)

`Dominio/Constantes/RolesSistema.cs`:

| Rol | Alcance |
|-----|---------|
| `Administrador` | **Gestión total** del catálogo (CRUD de productos, categorías, marcas, proveedores, precios). En ventas **solo las visualiza**, no las anula. |
| `EmpleadoVenta` | Registra ventas, ve el historial de ventas y **puede anularlas**. |

- El login es **solo para empleados** (`ServicioInicioSesion`); los clientes ya
  no tienen usuario/contraseña.
- `RolesSistema.Personal` agrupa `"Administrador,EmpleadoVenta"`;
  `RolesSistema.Normalizar(rol)` convierte cualquier valor desconocido en
  `EmpleadoVenta`.
- Las páginas de gestión usan `[Authorize(Roles = RolesSistema.Administrador)]`,
  la anulación `[Authorize(Roles = RolesSistema.EmpleadoVenta)]` y el resto del
  módulo de ventas `[Authorize(Roles = RolesSistema.Personal)]`; el `Program.cs`
  registra convenciones `AuthorizePage("/...")`.
- El rol se guarda en la **cookie** al iniciar sesión (`ServicioInicioSesion`),
  y `ContextoUsuarioHttp` lo expone a las fachadas vía `IContextoUsuario`.

---

## 9. Histórico de precios (pregunta casi segura)

`Dominio/Entidades/HistoricoPrecio.cs` + `ServicioHistoricoPrecio`.

1. **Al crear un producto**: `ServicioProducto.CrearAsync` inserta el producto y
   registra su primera versión con motivo *"Precio inicial"*.
2. **Al actualizar** (`ServicioProducto.ActualizarAsync`, dentro de una
   **transacción** de `IUnidadDeTrabajo`): obtiene el precio vigente
   (`ObtenerPrecioVigenteAsync`); si el precio **cambió**, cierra el vigente
   (`CerrarPrecioVigenteAsync`) e inserta el nuevo con motivo
   *"Cambio de precio"*. Producto y histórico se confirman o se revierten juntos.
3. **Siempre hay un solo precio vigente**: la fila con `FechaFinVigencia = NULL`.
4. **Vista**: `Pages/ProductoHistoricoPrecio.cshtml`.

**Detalle SQL:** `CerrarPrecioVigente` hace
`UPDATE ... SET FechaFinVigencia = NOW() WHERE IdProducto = @id AND FechaFinVigencia IS NULL`.

---

## 10. Asincronía de extremo a extremo

**Decisión:** todo el acceso a I/O es `async` para **no bloquear hilos** del
servidor bajo carga (escenario de ventas concurrentes).

- **Puertos:** `Task`/`Task<T>` (p. ej. `ObtenerTodosAsync`, `InsertarAsync`,
  `IniciarAsync`).
- **Repositorios MySQL:** `OpenAsync`, `ExecuteReaderAsync`, `ReadAsync`,
  `ExecuteNonQueryAsync`, `ExecuteScalarAsync`.
- **Servicios y fachadas:** `async`/`await` (p. ej. `RegistrarVentaAsync`,
  `AnularVentaAsync`).
- **Transacción:** `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`.
- **Páginas:** `OnGetAsync` / `OnPost...Async`.
- Se mantienen **sincrónicos** solo los cálculos en memoria (p. ej.
  `Coincide` del login y el hash de contraseña), que no son I/O.

**Prueba de concurrencia:** `FERRETERIA__Joel.Pruebas/Integracion/FachadaVentaIntegracionPruebas.cs`
lanza dos registros/anulaciones en paralelo y verifica que **solo una gana**
(idempotencia y reposición única).

---

## 11. Otras preguntas clásicas

### ¿Por qué eliminación lógica y no `DELETE`?
Por **auditoría**: `CambiarEstadoAsync` alterna `Estado=1/0` (activo/inactivo).
En ventas, anular es un `UPDATE ... EstadoVenta='Anulada'`; el `detalle_venta`
se conserva para reponer stock y para historial.

### ¿Cómo validas los datos antes de insertar?
Dos capas: (1) **validaciones de dominio** en `Dominio/Validaciones/*` y
`ConfiguracionValidacion/*`; (2) **integridad de la BD** (constraints únicas,
error 1062 capturado en el `catch`).

### ¿Dónde está la cadena de conexión? (seguridad)
En `appsettings*.json` (y variables de entorno), leída por `IDbConnectionFactory`
(`MySqlConnectionFactory`). **Nunca** en el código.

### ¿Cómo se guardan las contraseñas?
Con **Argon2** (`Infraestructura/Seguridad/Argon2HasheadorContrasena.cs`,
adaptador de `IHasheadorContrasena`). El login detecta hashes heredados en texto
plano y los **re-hashea** al autenticar.

### ¿Qué es un Page Model?
`Pages/*.cshtml.cs`: la capa de entrada de cada página Razor. Maneja
`OnGetAsync` / `OnPost...Async`, enlaza campos con `[BindProperty]` y recibe sus
dependencias por constructor.

### ¿Cómo se ve el flujo de una petición (request → DB)?
`HTTP` → `.cshtml` (vista) → `.cshtml.cs` (Page Model, async) → **fachada**
(`FachadaVenta`/`FachadaAnulacion`) → **servicio/adaptador** →
`RepositoryCreator<T>` → `MySql*Repository` (SQL async) → MySQL → entidades →
re-render de la vista.

### ¿Por qué `decimal` en precios y `int`/`short` en claves?
`decimal` evita errores de redondeo del dinero (`float`/`double`); los `Id*` usan
tipos enteros acordes a la tabla.

### ¿Por qué Razor Pages y no MVC clásico?
Es el modelo recomendado por Microsoft para aplicaciones administrativas: cada
página agrupa vista + Page Model, sin declarar controladores/vistas por separado.

### ¿Por qué `using`/`await using`?
`MySqlConnection`, `MySqlCommand` y `MySqlDataReader` son recursos conectados;
sin liberarlos se **fuga el pool** de MySQL. Se usan `using` (o `await using` en
`MySqlConexionTransaccional`).

### ¿Es "n" capas o Clean/Hexagonal?
Es un **monolito con arquitectura hexagonal**: núcleo (Dominio) + casos de uso
(Aplicación) + adaptadores (Infraestructura y Web). Se aplican los principios de
Clean Architecture (DIP, regla de dependencias) sin el andamiaje formal.

---

## 12. Cómo defender (checklist contra la rúbrica)

1. **Monolito + Hexagonal** → sección 1 y 2.
2. **≥3 patrones con Facade** → sección 4 (Facade, Factory Method, Strategy,
   Repository, Unit of Work).
3. **≥2 roles diferenciados** → sección 8 (`Administrador` gestiona el catálogo,
   `EmpleadoVenta` registra y anula ventas).
4. **Login + Home** → `Pages/Login.cshtml.cs`, `Pages/Index.cshtml.cs`.
5. **UI del proceso principal (25 pts)** → `Pages/Venta.cshtml(.cs)` (búsqueda de
   cliente/producto, carrito, totales con descuento en vivo).
6. **Transacción (10 pts)** → sección 5 (`RegistrarVentaAsync`).
7. **Anulación lógica (8 pts)** → sección 5 (`AnularVentaAsync`).
8. **Comprobante (8 pts)** → `FachadaVenta.ObtenerComprobanteAsync` +
   `Pages/VentaComprobante.cshtml`.
9. **Patrones/SOLID (12 pts)** → secciones 4 y 6.
10. **Asincronía** → sección 10.

**Fórmula de respuesta:** *concepto (1 frase) → archivo/método exacto → frase que
cierra*: *"por eso dependemos de puertos y no de MySQL, y por eso lo podemos
probar y cambiar sin tocar la lógica."*
