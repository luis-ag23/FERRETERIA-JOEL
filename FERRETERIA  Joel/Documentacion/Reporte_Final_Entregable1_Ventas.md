# Reporte Final — Entregable 1: Módulo de VENTAS

**Proyecto:** FERRETERÍA JOEL — Sistema administrativo
**Módulo entregado:** Ventas y Caja (registro, historial, comprobante y anulación)
**Rama:** `feature/ventas-scope-cleanup` (aislada, **sin merge a `master`**)
**Base de datos de trabajo y pruebas:** `bdferreteria_dev` (la base principal `bdferreteria` permanece intacta)
**Arquitectura:** Hexagonal (Dominio · Aplicación · Infraestructura · Web)
**Fecha:** 2026

---

## 1. Resumen ejecutivo

Se completó el módulo de **Ventas** con estas piezas:

- Registro de venta con buscadores de cliente y producto, tabla temporal M:N de líneas y confirmación por modal.
- Alta rápida de **cliente nuevo** desde la pantalla de venta (AJAX, sin recargar ni perder los productos seleccionados).
- **Historial de ventas** con modal de detalle de solo lectura, refresco del listado y **anulación** con motivo obligatorio.
- **Comprobante/recibo** con los elementos del Anexo 2, obtenido por token y protegido por autorización.
- Backend asíncrono de extremo a extremo con **Facade + Factory Method + Repository + Unit of Work**, control de stock y concurrencia.

Resultados verificados: **114/114 pruebas** (unitarias + integración) contra MySQL, con **build sin errores**.

> Nota de vigencia: este reporte describe el módulo de Ventas. Tras su entrega, la
> solución se consolidó: los puertos específicos por entidad se eliminaron
> (quedan `IRepository<T>` / `IGestionarRepository<T>`), las fábricas se redujeron
> a contratos genéricos y los roles pasaron a `Administrador` / `EmpleadoVenta`.
> Ver `Informe_SOLID_FerreteriaJoel.md` y `Guia_Defensa_SOLID.md` para el estado
> final.
>
> En el segundo entregable (EC2) el módulo de Ventas no cambió de
> comportamiento: lo que se añadió alrededor fue el escalamiento horizontal
> (cuatro réplicas detrás de Nginx), la caché distribuida en Redis, las claves
> de Data Protection compartidas y cifradas, la baja lógica del personal y el
> traslado de los secretos a un archivo `.env` fuera del repositorio. La
> protección contra ventas duplicadas, que se había retirado del código y de la
> base de datos, no vuelve a introducirse en este entregable.

---

## 2. Alcance: qué es y qué NO es trabajo de este módulo

**Es trabajo propio de este entregable:**

- El caso de uso de venta completo (registro, listado, detalle, comprobante y anulación).
- La fachada, las fábricas, los repositorios y la unidad de trabajo del proceso de venta.
- Los repositorios MySQL de venta, detalle de venta, inventario y cliente.
- La asincronía real y los mecanismos de concurrencia del flujo de venta.

**NO es trabajo propio (se respeta como dependencia ajena):**

- **Login, roles y sesiones:** el módulo de autenticación pertenece a otro compañero. Ventas **no** implementa autenticación; solo consume la identidad ya resuelta (ver sección 8).
- **CRUD de Categorías, Marcas, Productos, Proveedores y demás catálogos:** pertenecen a otros entregables. Aquí solo se usan como contexto (por ejemplo, el stock de un producto) y no se modificaron como trabajo propio.
- **Restricciones por rol** aplicadas en la carpeta de Ventas (`[Authorize]` y rol `EmpleadoVenta` para anular) son únicamente **puntos de enganche** con la seguridad del equipo, no un sistema de roles propio.

---

## 3. Arquitectura

Arquitectura **hexagonal por capas**, con la regla de dependencia apuntando hacia el dominio:

```
FERRETERIA__Joel.Dominio          -> contratos (puertos) y modelos. No depende de ASP.NET ni de MySql.Data.
FERRETERIA__Joel.Aplicacion       -> caso de uso de venta (fachadas, DTOs). Depende del dominio y de las implementaciones concretas/fábricas de Infraestructura.
FERRETERIA__Joel.Infraestructura  -> adaptadores MySQL (repositorios, conexión transaccional, unidad de trabajo, fábricas).
FERRETERIA  Joel (Web)            -> Razor Pages y adaptador de identidad. Inyecta la fachada por DI.
FERRETERIA__Joel.Pruebas          -> pruebas unitarias (fakes) e integración (MySQL real).
```

Regla verificada: la capa Web (Page Models) **solo** inyecta `FachadaVenta` y `ServicioCliente`; nunca ve repositorios ni transacciones.

---

## 4. Patrones implementados (con clases reales)

### 4.1 Facade — `FachadaVenta`
- **Archivo:** `FERRETERIA__Joel.Aplicacion/Servicios/FachadaVenta.cs`
- Única puerta de entrada al subsistema de ventas.
- Métodos de caso de uso (todos asíncronos, ver sección 5): `BuscarClientesAsync`, `BuscarProductosAsync`, `ObtenerProductoAsync`, `CalcularLineaAsync`, `CalcularResumen`, `RegistrarVentaAsync`, `ObtenerComprobanteAsync`. El historial y la anulación se movieron a `FachadaAnulacion` (`ListarVentasListadoAsync`, `ObtenerVentaAsync`, `AnularVentaAsync`).
- Desacople: `Venta.cshtml.cs` y `Ventas.cshtml.cs` no conocen `IUnidadDeTrabajo` ni los repositorios.

### 4.2 Factory Method — `RepositoryCreator<TContrato>`
- **Base (abstracta):** `Infraestructura/Factories/RepositoryCreator.cs` → `abstract class RepositoryCreator<TContrato> { public abstract TContrato CreateRepository(); }`
- **Fábricas concretas:** `ProductoRepositoryCreator`, `CategoriaRepositoryCreator`, `MarcaRepositoryCreator`, `ProveedorRepositoryCreator` y `EmpleadoRepositoryCreator` (todas en `Infraestructura/Factories/`). Los repositorios de operación especial del módulo de venta se inyectan como clase concreta.
- **Uso:** los servicios piden, p. ej., `RepositoryCreator<IGestionarRepository<Producto>>` y obtienen el repositorio con `CreateRepository()`. No hay `new MySql*Repository(...)` dentro del caso de uso: cambiar de motor solo exige otra fábrica.

### 4.3 Unit of Work — `IUnidadDeTrabajo`
- **Puerto:** `Dominio/Puertos/IUnidadDeTrabajo.cs` → `IniciarAsync`, `ConfirmarAsync`, `RevertirAsync`.
- **Adaptador:** `Infraestructura/MySqlUnidadDeTrabajo.cs`, que delega en `IConexionTransaccional` (`Infraestructura/Conexion/MySqlConexionTransaccional.cs`).
- **Uso:** `RegistrarVentaAsync` y `AnularVentaAsync` abren **una sola transacción MySQL por operación** (`IniciarAsync`), aplican la reposición de stock + inserción/estado de la venta, y cierran con `ConfirmarAsync`; ante cualquier error, `RevertirAsync`. Resultado: o todo, o nada.

### 4.4 Puertos y adaptadores (Hexagonal)
- **Puertos (Dominio):** `IRepository<T>`, `IGestionarRepository<T>`, `IUnidadDeTrabajo`, `IContextoUsuario`.
- **Contratos de infraestructura:** `IConexionTransaccional` (agnóstica del caso de uso).
- **Adaptadores:** repositorios MySQL en `Infraestructura/Repositorios` (los de operación especial del módulo de venta se inyectan como clase concreta) y el adaptador de identidad `ContextoUsuarioHttp` en Web.
- El **dominio no depende** de ASP.NET ni de `MySql.Data`.

---

## 5. Asincronía real de extremo a extremo

Toda la ruta de entrada/salida a MySQL usa `async`/`await` real, **sin `Task.Run` ni `async void`** (verificado: 0 ocurrencias en el código del proyecto).

Ruta asíncrona:

1. **Páginas Razor (Web):** handlers `OnGetBuscarClientesAsync`, `OnGetBuscarProductosAsync`, `OnPostCrearClienteAsync`, `OnPostRegistrarAsync`, `OnGetAsync`, `OnGetDetalleAsync`, `OnPostAnularAsync`, `OnGetAsync(string token)`.
2. **Fachada:** `BuscarClientesAsync`, `RegistrarVentaAsync`, `AnularVentaAsync`, `ObtenerComprobanteAsync`, etc.
3. **Servicio de clientes:** `ServicioCliente` con `ListarAsync`, `BuscarAsync`, `CrearAsync`.
4. **Puertos y repositorios:** `DescontarAsync`, `ReponerAsync`, `InsertarAsync`, `MarcarAnuladaAsync`, `ObtenerParaVentaAsync`, etc.
5. **Conexión/transacción:** `OpenAsync`, `BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`, `DisposeAsync`.

**Evidencia:** 60 llamadas `await` a operaciones asíncronas de MySQL en `FERRETERIA__Joel.Infraestructura`; `MySqlConexionTransaccional` usa `OpenAsync`/`BeginTransactionAsync`/`CommitAsync`/`RollbackAsync` y `IAsyncDisposable`.

> Nota: `CalcularResumen` se mantiene síncrono porque es cálculo puro en memoria (sin E/S). El cálculo por línea expuesto a la Web (`CalcularLineaAsync`) sí es asíncrono porque consulta el producto.

---

## 6. Concurrencia y consistencia

Los tres escenarios críticos se garantizan en el nivel donde la concurrencia es real: **la base de datos**.

| # | Escenario | Mecanismo | Prueba |
|---|-----------|-----------|--------|
| 1 | Dos ventas simultáneas de la **última unidad**: solo una gana, el stock nunca queda negativo | `UPDATE producto SET CantidadStock = CantidadStock - @c WHERE IdProducto=@id AND CantidadStock >= @c` (descuento atómico condicional) | `RegistrarVenta_Real_Concurrente_SoloUnaGana` |
| 2 | Ventas concurrentes no repiten **código de venta** | Correlativo por `IdVenta` (`V-YYYYMMDD-######`) + índice `UNIQUE_CODIGO_VENTA` | `RegistrarVenta_Real_Concurrente_CodigosUnicosAsync` |
| 3 | **Anulación concurrente** repone el stock exactamente una vez | `UPDATE venta SET EstadoVenta='Anulada' ... WHERE IdVenta=@id AND EstadoVenta='Registrada'` (afecta 1 fila solo una vez) + reposición en la misma transacción | `AnularVenta_Real_Concurrente_RestauraStockUnaVezAsync` |

Consideración de errores transaccionales: si el `UPDATE` atómico no afecta filas (stock insuficiente por carrera) o si ocurre cualquier excepción (por ejemplo, colisión de índice único), el flujo llama a `RevertirAsync` y devuelve un fallo explícito, **sin dejar estados parciales**. Las operaciones de registrar y anular tocan el stock en direcciones opuestas, de modo que ante solapamiento la transacción revierte completa.

---

## 7. Transacciones: commit y rollback (incluido el fallo **después** de modificar inventario)

- **Commit:** `RegistrarVentaAsync` y `AnularVentaAsync` terminan con `ConfirmarAsync` solo si todos los pasos tuvieron éxito.
- **Rollback por validación previa:** `RegistrarVenta_Real_StockInsuficiente_NoPersisteAsync` comprueba que, si el stock es insuficiente, no se persiste venta ni se altera inventario.
- **Rollback por fallo DESPUÉS de modificar inventario (verificado):** se forzó un fallo en el `INSERT` de la venta (clave foránea de empleado inexistente) **una vez ya aplicado el descuento de stock**. Resultado: la operación devolvió fallo, el **stock volvió a su valor original** y **no quedó ninguna venta** registrada. Esto demuestra que la reversión cubre la mutación de inventario, no solo las validaciones.

> Verificación puntual ejecutada contra `bdferreteria_dev` con inyección de fallo; no se dejó como prueba permanente para no ampliar la suite del módulo.

---

## 8. Integración con la autenticación (dependencia ajena, documentada)

El módulo de Ventas **no** implementa login, roles ni sesiones. La identidad se obtiene a través del puerto:

- **Puerto:** `Dominio/Puertos/IContextoUsuario.cs` → `IdEmpleado`, `NombreCompleto`, `Rol`.
- **Contrato documentado:** `IdEmpleado` devuelve `0` cuando no hay autenticación; Ventas **rechaza** registrar o anular venta si `IdEmpleado <= 0`. El rol `EmpleadoVenta` habilita la anulación (el `Administrador` solo visualiza las ventas), pero la autorización definitiva corresponde al módulo de seguridad.
- **Adaptador temporal:** `Servicios/ContextoUsuarioHttp.cs` resuelve la identidad desde las claims del usuario autenticado; es reemplazable por la implementación del compañero **sin tocar el código de Ventas**.
- **En el flujo:** `Venta.IdEmpleadoResponsable` (emisor) e `IdEmpleadoAnulacion` (autor de la anulación) provienen de este puerto.

---

## 9. Clases implementadas / adaptadas (módulo de Ventas)

**Dominio (puertos):**
`IRepository<T>`, `IGestionarRepository<T>`, `IUnidadDeTrabajo`, `IContextoUsuario`.

**Aplicación:**
`FachadaVenta`, `FachadaAnulacion`, `FachadaActualizacionStock`, `ServicioCliente`.
Modelos/DTOs: `SolicitudVenta`, `LineaVentaSolicitada`, `ResultadoVenta`, `ResultadoCliente`, `ComprobanteVenta`, `VentaListado`, `LineaVentaDetalle`.

**Infraestructura:**
`MySqlConexionTransaccional`, `MySqlUnidadDeTrabajo`, `MySqlClienteRepository`, `MySqlVentaRepository`, `MySqlDetalleVentaRepository`, `MySqlInventarioRepository`, y las fábricas `ProductoRepositoryCreator`, `CategoriaRepositoryCreator`, `MarcaRepositoryCreator`, `ProveedorRepositoryCreator`, `EmpleadoRepositoryCreator`.

**Web (Razor Pages):**
`Venta.cshtml(.cs)`, `Ventas.cshtml(.cs)`, `VentaComprobante.cshtml(.cs)`, y el adaptador `ContextoUsuarioHttp`.

**Base de datos (solo `bdferreteria_dev`):**
`UNIQUE_CODIGO_VENTA` y columnas de anulación (`EstadoVenta`, `FechaAnulacion`, `MotivoAnulacion`, `IdEmpleadoAnulacion`).

---

## 10. Pruebas ejecutadas

Comando base:
`dotnet test "FERRETERIA__Joel.Pruebas\FERRETERIA__Joel.Pruebas.csproj"`

### 10.1 Suite completa (unitarias con fakes y utilidades)

```
Passed!  - Failed: 0, Passed: 119, Skipped: 0, Total: 119
```

Cubren: registro correcto (stock y totales), agrupación de líneas duplicadas, stock insuficiente, cliente inactivo, anulación (estado no registrada falla), empleado inválido (`IdEmpleado <= 0`) en registrar y anular, listado con y sin filtro de cliente, comprobante con líneas y catálogo, comprobante inexistente; `ServicioCliente` (alta válida, campos obligatorios, empleado responsable, búsqueda solo de activos); activación de cuentas y validación de credenciales; conversión de número a letras.

De las 119 pruebas, **109 son unitarias** (con fakes) y **10 son de integración
real contra MySQL** (sección 10.2).

> Las pruebas de integración (sección 10.2) se **omiten sin fallar** cuando no hay cadena de conexión; por eso pueden reportarse por separado.

### 10.2 Integración real contra MySQL (`bdferreteria_dev`)

Requiere `FERRETERIA_TEST_CONNECTION` apuntando a la base de desarrollo; se ejecutó con conexiones **independientes**.

```
Passed!  - Failed: 0, Passed: 10, Skipped: 0, Total: 10   (~0.8 s)
```

| Prueba | Qué verifica |
|--------|--------------|
| `RegistrarVenta_Real_DescuentaStockYAnulacionRestauraAsync` | commit real: descuenta stock y la anulación lo restaura |
| `RegistrarVenta_Real_StockInsuficiente_NoPersisteAsync` | rollback por validación: no persiste ni altera stock |
| `RegistrarVenta_Real_Concurrente_SoloUnaGana` | escenario 1: última unidad, gana una sola venta |
| `RegistrarVenta_Real_Concurrente_CodigosUnicosAsync` | escenario 2: códigos de venta únicos |
| `AnularVenta_Real_Concurrente_RestauraStockUnaVezAsync` | escenario 3: reposición exactamente una vez |
| `Listar_Publico_DevuelvePersonal_SinCredencialesAsync` | la proyección pública del personal no filtra contraseña ni token |
| `Listar_Publico_NoDevuelveElHashDeLaContrasenaAsync` | la proyección con credenciales solo se usa para autenticar |
| `ExistePorCorreo_UsaProyeccionSinCredencialesAsync` | la consulta de existencia no necesita credenciales |
| `BajaLogica_DesactivaLaCuenta_SinBorrarlaAsync` | la baja cambia el estado y **no borra** la fila; la reactivación la deja pendiente de activación |
| `ServicioPersonal_NoPermiteDarDeBajaLaPropiaCuentaAsync` | el administrador no puede darse de baja a sí mismo |

> Las pruebas de integración están escritas para **omitirse sin fallar** si falta la cadena de conexión; por eso se reportan **por separado** de las unitarias. Las cifras anteriores corresponden a ejecuciones **reales** contra `bdferreteria_dev` (no a omisiones).
>
> Estas pruebas detectaron dos defectos reales que no se veían sin base de datos: el script de arranque de Docker (`Scripts/init-completo.sql`) definía `detalle_venta` y `historico_precio` con columnas distintas a las que usa el código (por lo que **era imposible registrar una venta** en el despliegue Docker), y el mapeo de la proyección pública del empleado leía la columna `TokenExpira`, que esa proyección no trae.

### 10.3 Build
`dotnet build "FERRETERIA  Joel.slnx"` → **0 advertencias, 0 errores**.

---

## 11. Control de versiones (commits)

Rama **`feature/ventas-scope-cleanup`** (aislada, sin merge a `master`):

| Hash | Descripción |
|------|-------------|
| `a71326c` | limpieza de alcance: quitar del módulo lo ajeno a Ventas |
| `c024359` | desacoplar las páginas de venta del sistema de autenticación |
| `c664323` | restaurar protección con `[Authorize]` y rol Administrador (punto de enganche) |
| `e912b9e` | dejar de rastrear `appsettings.json` con credenciales |
| `f75d8fc` | **F3** — asincronía real de extremo a extremo en el flujo de venta |
| `2567816` | **F4** — verificar los cuatro escenarios de concurrencia contra MySQL |
| `34f853e` | **F6** — documentar `IContextoUsuario` como punto de integración con la autenticación |

---

## 12. Estado final

- Rama `feature/ventas-scope-cleanup` aislada; **no se hizo merge a `master`**.
- La base principal `bdferreteria` **no fue modificada**; todo el trabajo y las pruebas usaron `bdferreteria_dev`.
- `appsettings.json` permanece **sin versionar** (contiene la cadena de conexión).
- Build limpio y suites en verde (119 pruebas).
- Login, roles, sesiones y los CRUD de catálogo **no** se reclaman como trabajo de este módulo.

---

## 13. Preguntas frecuentes de defensa (módulo de Ventas)

1. **¿Por qué una fachada y no llamar directo a los repositorios?** `FachadaVenta` coordina cliente, el descuento de stock y la transacción; la Web queda simple y el caso de uso es reutilizable.
2. **¿Dónde está la transacción y qué cubre?** En `RegistrarVentaAsync`/`AnularVentaAsync` vía `IUnidadDeTrabajo`: descontar/reponer stock + venta + detalles como una sola operación atómica.
3. **¿Cómo evitan el sobre-stock bajo concurrencia?** Descuento atómico `UPDATE ... WHERE CantidadStock >= @c`; si no afecta filas, se revierte y se informa el fallo.
4. **¿Cómo garantizan reponer el stock una sola vez?** `UPDATE venta ... WHERE EstadoVenta='Registrada'`: la segunda anulación no afecta filas.
5. **¿La asincronía es real?** Sí: `async`/`await` hasta MySQL (`OpenAsync`, `BeginTransactionAsync`, `ExecuteReaderAsync`, `CommitAsync`/`RollbackAsync`); sin `Task.Run`.
6. **¿Cómo se integra con la autenticación del compañero?** A través de `IContextoUsuario`; el adaptador `ContextoUsuarioHttp` es temporal y reemplazable sin tocar Ventas.
