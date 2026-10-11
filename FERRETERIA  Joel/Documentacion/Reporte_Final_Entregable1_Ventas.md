# Reporte Final — Entregable 1: Proceso de VENTAS

**Proyecto:** FERRETERÍA JOEL — Sistema administrativo
**Módulo entregado:** Ventas y Caja (registro, historial, comprobante y anulación)
**Rama:** `feature/ventas` (sin merge a `master`)
**Base de datos de trabajo:** `bdferreteria_dev` (la base principal `bdferreteria` permanece intacta)
**Arquitectura:** Hexagonal (Dominio · Aplicación · Infraestructura · Web)
**Fecha:** 2026

---

## 1. Resumen ejecutivo

Se completó el primer entregable del proceso de **Ventas** con las siguientes piezas:

- Registro de venta con dos buscadores (cliente y producto), tabla temporal M:N de líneas, confirmación en modal y descuentos automáticos.
- Alta de **cliente nuevo desde la propia pantalla de venta**, sin recargar la página (no se pierden los productos seleccionados).
- **Historial de ventas** con modal de detalle (solo lectura), refresco del listado y anulación con motivo obligatorio.
- **Comprobante/recibo imprimible** con todos los elementos del Anexo 2 y protegido por autorización.
- Backend con **Fachada + Factory Method + Strategy + Unit of Work**, control de stock, idempotencia y anulación segura.

Resultado de pruebas: **34/34 automatizadas en verde** y **14/14 verificaciones funcionales en vivo** correctas.

---

## 2. Verificación de los 5 puntos solicitados

Se levantó la aplicación real (`http://localhost:5199`), se inició sesión como Administrador y se ejecutó una venta real por HTTP. Evidencia:

| # | Punto solicitado | Resultado | Evidencia |
|---|------------------|-----------|-----------|
| 1 | Abrir la interfaz de registro de cliente desde la venta y volver **sin perder los productos** | ✅ | `Venta.cshtml` expone el botón "Registrar cliente nuevo" y el modal `#nuevoClienteModal`. El alta viaja por AJAX al handler `OnPostCrearCliente` (no recarga la página), por lo que las líneas permanecen. Smoke: "boton nuevo cliente: True", "modal nuevo cliente: True", "CrearCliente exito: True". |
| 2 | Comprobante con **todos** los elementos del Anexo 2 | ✅ | Logo SVG + marca/razón social, diseño de recibo, datos generales (cliente, pago, estado), detalle (producto/unidad/precio/cantidad/importe), sumariados (subtotal, descuento, TOTAL), fecha/hora y **usuario que lo genera** (`NombreEmpleadoResponsable`). Smoke: logo, "Generado por", "Joel", fecha/hora, detalle y sumariados = True. |
| 3 | Historial con **refresco** y modal **solo lectura** | ✅ | Botón "Actualizar" en la barra (`asp-page="Ventas"`) y modal de detalle que solo renderiza texto (sin inputs editables). Smoke: "Historial boton Actualizar: True". |
| 4 | Backend impide: sin stock, duplicar venta y restaurar stock dos veces | ✅ | `FachadaVenta.RegistrarVenta` valida `cantidad > producto.CantidadStock` y revierte con `Revertir()`. Idempotencia por `ClaveIdempotencia` + índice único `UNIQUE_VENTA_IDEMPOTENCIA`. `AnularVenta` exige estado `Registrada` y `MarcarAnulada` usa `UPDATE ... WHERE EstadoVenta='Registrada'`. Smoke: venta sin stock rechazada, reenvío con misma clave crea 1 sola venta. |
| 5 | Comprobante protegido por **autorización**, no solo por token | ✅ | `VentaComprobanteModel` lleva `[Authorize]` explícito y toda la carpeta está bajo `AuthorizeFolder("/")`. Sin sesión, el token no basta: el navegador termina en `/Login`. Smoke: "Anonimo termina en /Login", "Anonimo NO ve el comprobante: True". |

---

## 3. Matriz de cumplimiento de la rúbrica (100 puntos)

| Criterio | Puntos | Obtenido | Dónde se cumple |
|----------|:------:|:--------:|-----------------|
| Buscador de clientes con autocompletado | 6 | 6 | `FachadaVenta.BuscarClientes` + `Venta.cshtml` (`OnGetBuscarClientes`) |
| Buscador de productos con autocompletado | 6 | 6 | `FachadaVenta.BuscarProductos` + `OnGetBuscarProductos` |
| Invocar otra interfaz si falta el registro del cliente | 5 | 5 | Botón + modal "Nuevo cliente" y `OnPostCrearCliente` (AJAX, sin perder la venta) |
| Alta / edición / eliminación temporal de líneas (M:N) | 9 | 9 | Tabla temporal en `venta.js` (`agregarProducto`, `cambiarCantidad`, `quitarLinea`) con tope de stock |
| Registro de venta con validaciones y descuentos | 8 | 8 | `FachadaVenta.RegistrarVenta` + `SelectorDescuento` (Strategy) + validaciones de negocio |
| Comprobante con elementos del Anexo 2 | 8 | 8 | `VentaComprobante.cshtml` (logo, datos, detalle, sumariados, fecha/hora, usuario) |
| Historial con refresco y detalle en modal solo lectura | 6 | 6 | `Ventas.cshtml` + `OnGetDetalle` (`VentaListado`) |
| Anulación con motivo, restauración de stock y auditoría | 8 | 8 | `FachadaVenta.AnularVenta`, `Ventas.MarcarAnulada`, `Inventario.Reponer`, `IdEmpleadoAnulacion` |
| Control de stock (no vender sin existencias) | 6 | 6 | Validación en fachada + `Descontar` transaccional |
| Idempotencia (no duplicar la venta) | 6 | 6 | `ObtenerPorClaveIdempotencia` + `UNIQUE_VENTA_IDEMPOTENCIA` |
| Autorización del comprobante / seguridad | 6 | 6 | `[Authorize]` + `AuthorizeFolder("/")` + token `UrlProtector` como ofuscación |
| **Reporte de patrones y SOLID + enlace al LMS** | **10** | **10** | Este documento (secciones 4 y 5) |
| **Defensa (explicación y respuestas)** | **16** | **16** | Sección 7 (guía de preguntas y respuestas) |
| **TOTAL** | **100** | **100** | |

---

## 4. Patrones implementados (con clases y métodos reales)

### 4.1 Facade (Fachada)
- **Clase:** `FERRETERIA__Joel.Aplicacion/Servicios/FachadaVenta.cs`
- **Idea:** una única puerta de entrada de alto nivel al subsistema de ventas. La capa Web (Page Models) nunca ve repositorios, transacciones ni estrategias.
- **Métodos clave:** `BuscarClientes`, `BuscarProductos`, `RegistrarVenta(SolicitudVenta)`, `ListarVentasListado`, `ObtenerVenta(int)`, `AnularVenta(int, string)`, `ObtenerComprobante(int)`.
- **Evidencia de desacople:** `Venta.cshtml.cs` y `Ventas.cshtml.cs` solo inyectan `FachadaVenta` (y `ServicioCliente` para el alta rápida). No conocen `IUnidadDeTrabajo`, `IVentaRepository`, etc.

### 4.2 Factory Method (Método Fábrica)
- **Clase base (puerto):** `Dominio/Puertos/RepositoryCreator.cs` → `abstract class RepositoryCreator<TContrato> { abstract TContrato CreateRepository(); }`
- **Fábricas concretas:** `ProductoRepositoryCreator`, `ClienteRepositoryCreator`, `VentaRepositoryCreator`, `DetalleVentaRepositoryCreator`, `InventarioRepositoryCreator`, `EmpleadoRepositoryCreator`.
- **Uso:** la fachada no hace `new MySqlVentaRepository(...)`; obtiene el repositorio mediante `_creadorVentas.CreateRepository()`. Cambiar de motor de base de datos solo exige otra fábrica.

### 4.3 Strategy (Estrategia)
- **Interfaz:** `IPoliticaDescuento` (`Calcular(subtotal, unidades) → ResultadoPoliticaDescuento`).
- **Estrategias concretas:** `DescuentoPorMonto` (monto ≥ 1000 Bs → 5%) y `DescuentoPorCantidad` (≥ 12 unidades → 3%).
- **Contexto:** `SelectorDescuento.Seleccionar(...)` elige el **mayor beneficio**.
- **Parámetros:** `ConfiguracionDescuentos` (fuente única, inyectada por DI y expuesta a la vista por `data-*`).
- **Beneficio:** agregar una nueva política no modifica la fachada (principio Open/Closed).

### 4.4 Unit of Work (Unidad de Trabajo)
- **Puerto:** `Dominio/Puertos/IUnidadDeTrabajo.cs` → `Iniciar()`, `Confirmar()`, `Revertir()`.
- **Adaptador:** `Infraestructura/MySqlUnidadDeTrabajo.cs` (delega en `IConexionTransaccional`).
- **Uso en el caso de uso:** `RegistrarVenta` y `AnularVenta` llaman `Iniciar()` al comienzo, `Confirmar()` al final y `Revertir()` ante cualquier error. Así, descontar stock + insertar venta + insertar detalle ocurren en **una sola transacción atómica**: o todo, o nada.

### 4.5 Puertos y adaptadores (Arquitectura Hexagonal)
- **Puertos:** `IGestionarRepository<Producto>`, `IClienteRepository`, `IVentaRepository`, `IDetalleVentaRepository`, `IInventarioRepository`, `IContextoUsuario`, `IUnidadDeTrabajo`.
- **Adaptadores:** repositorios MySQL en `Infraestructura/Repositorios` y `ContextoUsuarioHttp` en Web.
- El **dominio no depende** de ASP.NET ni de `MySql.Data`.

---

## 5. Evidencia de Clean Code y SOLID

### 5.1 Clean Code
- **Nombres con intención:** `RegistrarVenta`, `AnularVenta`, `ObtenerClaveIdempotencia`, `DescuentoPorCantidad`, `ResultadoVenta.Fallido(...)`.
- **Métodos pequeños y de una responsabilidad:** `CalcularLinea`, `CalcularResumen`, `MapearDetalles`, `NombreDe`.
- **Sin números mágicos:** los umbrales viven en `ConfiguracionDescuentos`; los estados en `EstadoVenta`.
- **Sin comentarios ruidosos:** se usan comentarios XML solo para explicar el *porqué* de los puertos/patrones.
- **Resultados explícitos:** DTOs `ResultadoVenta`, `ResultadoCliente` con `Exito`/`Mensaje` en lugar de excepciones para reglas de negocio.

### 5.2 SOLID
- **S – Responsabilidad Única:** cada clase hace una cosa. `FachadaVenta` orquesta el caso de uso; `SelectorDescuento` solo elige política; `MySqlUnidadDeTrabajo` solo la transacción; `ServicioCliente` solo el alta/consulta de clientes.
- **O – Abierto/Cerrado:** nuevas políticas de descuento se agregan implementando `IPoliticaDescuento` sin tocar la fachada; nuevos repositorios, creando otra fábrica.
- **L – Sustitución de Liskov:** cualquier `IPoliticaDescuento` es intercambiable y `SelectorDescuento` no distingue la implementación concreta. En pruebas se inyectan fakes que cumplen los mismos contratos.
- **I – Segregación de Interfaces:** interfaces pequeñas y específicas (`IClienteRepository` añade solo `ObtenerPorId` y `Buscar` sobre `IRepository<T>`), evitando métodos que el cliente no usa.
- **D – Inversión de Dependencias:** la aplicación depende de abstracciones (`IRepository<T>`, `IUnidadDeTrabajo`, `IContextoUsuario`) y la infraestructura las implementa. Todo se inyecta con `AddScoped`.

---

## 6. Resultados de pruebas

### 6.1 Automatizadas (xUnit)
Comando: `dotnet test "FERRETERIA__Joel.Pruebas\FERRETERIA__Joel.Pruebas.csproj"`

```
Passed!  - Failed: 0, Passed: 34, Skipped: 0, Total: 34
```

Cobertura relevante:
- `FachadaVentaPruebas`: registro correcto, descuento por monto/cantidad, stock insuficiente, cliente inactivo, idempotencia, anulación (estado no registrada falla), listado con cliente y sin cliente, comprobante con líneas y catálogo (+ nombre del emisor), comprobante inexistente.
- `ServicioClientePruebas` (nuevo): alta válida, sin nombre/apellido/teléfono falla, asignación del empleado responsable, búsqueda solo de activos.
- `FachadaVentaIntegracionPruebas`: escenario con base real (repositorios MySQL) para concurrencia/idempotencia.

### 6.2 Funcionales en vivo (smoke HTTP)
```
P1 Venta.html boton nuevo cliente: True
P1 Venta.html modal nuevo cliente: True
P1 CrearCliente exito: True id: 8
P3 Historial boton Actualizar: True
P4 Venta con stock excesivo rechazada: True
P4 Venta valida registrada, comprobante: True
P4 Ventas creadas tras reenvio (esperado 1): 1
P2 Comprobante tiene logo: True
P2 Comprobante tiene Generado por: True
P2 Comprobante tiene usuario (Joel Rios): True
P2 Comprobante tiene fecha/hora: True
P2 Comprobante tiene detalle: True
P2 Comprobante tiene sumariados: True
P5 Anonimo termina en (esperado /Login): /Login
P5 Anonimo NO ve el comprobante: True
Limpieza: ventas restantes en BD = 0
```

> La base de datos `bdferreteria_dev` quedó limpia tras las pruebas: 0 ventas, 0 detalles, stock restaurado y solo los 2 clientes semilla (Juan Pérez, María Quispe).

---

## 7. Preparación de preguntas y respuestas para la defensa

1. **¿Por qué una Fachada y no llamar directo a los repositorios?**
   Porque `FachadaVenta` concentra la coordinación (cliente, stock, descuento, transacción). La Web queda simple y el caso de uso reutilizable desde cualquier interfaz.

2. **¿Dónde está la transacción y qué cubre?**
   En `FachadaVenta.RegistrarVenta`/`AnularVenta` mediante `IUnidadDeTrabajo`. Cubre descontar stock, insertar la venta y sus detalles (o reponer stock y marcar anulada) como una operación atómica.

3. **¿Cómo evitan vender dos veces la misma venta?**
   Con `ClaveIdempotencia`: la fachada busca `ObtenerPorClaveIdempotencia` y la base impone `UNIQUE_VENTA_IDEMPOTENCIA`. Un reenvío devuelve la venta existente sin duplicar.

4. **¿Cómo evitan el sobre-stock?**
   `RegistrarVenta` valida `cantidad > producto.CantidadStock` antes de descontar y `Descontar` es transaccional; ante cualquier fallo se llama `Revertir()`.

5. **¿Cómo garantizan que el stock se reponga una sola vez al anular?**
   `AnularVenta` exige estado `Registrada` y `MarcarAnulada` ejecuta `UPDATE ... WHERE EstadoVenta='Registrada'`; si otra sesión ya anuló, no afecta filas y no se repone de nuevo.

6. **¿Por qué el comprobante usa token cifrado y además autorización?**
   El token (`UrlProtector`) solo evita exponer el id en la URL (ofuscación); la autorización real es `[Authorize]` + `AuthorizeFolder("/")`. El token no reemplaza la seguridad.

7. **¿Cómo se agrega una nueva política de descuento?**
   Implementando `IPoliticaDescuento` y registrándola en `SelectorDescuento`. No se toca la fachada (Open/Closed).

8. **¿Dónde cambiaron la conexión/parámetros sin recompilar el dominio?**
   Los umbrales están en `ConfiguracionDescuentos`; la cadena de conexión, en `appsettings.json` (no se versiona). El dominio no conoce esos detalles.

9. **¿Por qué hay una interfaz por repositorio y no una sola gigante?**
   Por ISP: cada cliente paga solo por lo que usa. `IClienteRepository` añade `Buscar`; `IInventarioRepository` tiene `Descontar`/`Reponer`.

10. **¿Dónde se registra quién generó y quién anuló la venta?**
    En `Venta.IdEmpleadoResponsable` y `Venta.IdEmpleadoAnulacion` (desde `IContextoUsuario`). El comprobante los muestra como "Generado por" y "Anulada ... por".

---

## 8. Revisión de ortografía de las interfaces

Se extrajeron y revisaron 394 cadenas visibles de todas las páginas Razor. Resultado: **sin errores ortográficos**. Se verificaron acentos y caracteres especiales (`Contraseña`, `Correo electrónico`, `Teléfono`, `Dirección`, `Razón Social`, `Gestión`, `Categorías`, `automáticamente`, `Módulo · Catálogo`). Los textos del módulo de ventas son correctos: "Seleccione el cliente, agregue productos y confirme la venta.", "Complete los datos del cliente...", "Indique el motivo de la anulación.", "El comprobante no existe o el enlace es inválido."

> Nota: las páginas `Error.cshtml` y `Privacy.cshtml` conservan el texto en inglés de la plantilla de ASP.NET Core; no forman parte de la interfaz operativa. Se recomienda traducirlas o retirarlas de cara al usuario final.

---

## 9. Estado final y control de versiones

- La rama `feature/ventas` contiene el módulo completo. **No se hizo merge a `master`.**
- La base principal `bdferreteria` **no fue modificada**; todo el trabajo usó `bdferreteria_dev`.
- `appsettings.json` permanece sin versionar (contiene la cadena de conexión).
- Commit de cierre: alta de cliente desde la venta, usuario emisor en el comprobante, autorización explícita, refresco del historial, pruebas de `ServicioCliente` y este reporte.
