# FERRETERIA Joel

Sistema administrativo web para una ferretería, construido con **ASP.NET Core
Razor Pages (.NET 10)** y **arquitectura hexagonal (puertos y adaptadores)**,
dividida en proyectos por capa. Incluye catálogo con CRUD, histórico de precios,
seguridad con roles y un módulo de ventas transaccional.

Repositorio: https://github.com/Macacofig/FERRETERIA-Joel

## Características

- **Catálogo** con CRUD completo: Categorías, Marcas, Productos y Proveedores,
  con validaciones de negocio y eliminación lógica (soft delete).
- **Histórico de precios** por vigencia, con vista de cambios (`ProductoHistoricoPrecio`).
- **Seguridad**: inicio/cierre de sesión por cookie **solo para empleados**, roles
  diferenciados y bitácora de sesiones. Contraseñas con **Argon2**.
- **Ventas**: registro con carrito y descuentos automáticos, historial, comprobante
  y **anulación lógica**, todo sobre **transacciones atómicas** e **idempotencia**.
- **Asincronía de extremo a extremo** (`async`/`await` hasta MySQL).

## Arquitectura

Monolito con arquitectura **hexagonal** (puertos y adaptadores). La regla de
dependencia siempre apunta hacia el Dominio:

```
Web ─▶ Aplicacion ─▶ Dominio ◀─ Infraestructura
```

| Proyecto | Responsabilidad |
|----------|-----------------|
| `FERRETERIA__Joel.Dominio` | Núcleo: `Entidades/`, `Puertos/`, `Validaciones/`, `ConfiguracionValidacion/`, `Constantes/`, `Utilidades/` |
| `FERRETERIA__Joel.Aplicacion` | Casos de uso: `Servicios/` (servicios y fachadas), `Modelos/` (DTOs), `Politicas/` |
| `FERRETERIA__Joel.Infraestructura` | Adaptadores de salida: `Repositorios/` (MySQL), `Conexion/`, `Factories/`, `Seguridad/` |
| `FERRETERIA  Joel` (Web) | Adaptador de entrada: `Pages/` (Razor Pages), `Servicios/`, `Helpers/`, `Program.cs` |
| `FERRETERIA__Joel.Pruebas` | Pruebas unitarias e integración (xUnit) |

### Patrones de diseño

**Facade** (`FachadaVenta`, `FachadaAnulacion`, `FachadaActualizacionStock`),
**Factory Method** (`RepositoryCreator<TContrato>` + `Infraestructura/Factories/`),
**Strategy** (`IPoliticaDescuento` + `SelectorDescuento`), **Repository**
(`MySql*Repository`) y **Unit of Work** (`IUnidadDeTrabajo`).

### Roles

| Rol | Alcance |
|-----|---------|
| `Administrador` | Gestión total del catálogo; en ventas solo visualiza (no anula) |
| `EmpleadoVenta` | Registra ventas, ve el historial y **puede anularlas** |

## Requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- MySQL 8+ (base de trabajo de desarrollo: `bdferreteria_dev`)

## Configuración

La cadena de conexión se lee con la clave `ConnectionStrings:MySqlConnection`.
Puede definirse en `appsettings.json` o mediante variables de entorno, y el
proyecto también soporta un archivo `.env` (no versionado) dentro de
`FERRETERIA  Joel/`. ASP.NET Core lee el archivo `.env` al arrancar y expone cada
clave como variable de entorno (usa `__` para anidar configuración):

```
ConnectionStrings__MySqlConnection=Server=localhost;Database=bdferreteria;Uid=root;Pwd=tu_clave;
```

También se puede ajustar en `appsettings.json`:

```json
"ConnectionStrings": {
  "MySqlConnection": "Server=localhost;Database=bdferreteria;Uid=root;Pwd=tu_clave;"
},
"UrlProteccion": {
  "Clave": "una_clave_larga_para_proteger_los_tokens_de_url"
}
```

## Base de datos

Ejecutar los scripts de `Scripts/` en orden sobre MySQL:

1. `Scripts/esquema.sql` — esquema base (catálogo, clientes, empleados, ventas).
2. `Scripts/registro_seguridad.sql` — índice único de correo del empleado (login).
3. `Scripts/bitacora_seguridad.sql` — tabla de bitácora de sesiones.
4. `Scripts/migracion_ventas.sql` — columnas y restricciones del módulo de ventas
   (idempotencia, anulación) y limpieza de credenciales del cliente.
5. `Scripts/migracion_cliente_ci.sql`

## Compilar, probar y ejecutar

```bash
# Compilar la solución completa
dotnet build "FERRETERIA  Joel.slnx"

# Ejecutar las pruebas
dotnet test "FERRETERIA  Joel.slnx"

# Ejecutar la aplicación web
dotnet run --project "FERRETERIA  Joel"
```

Las pruebas de integración requieren la variable de entorno
`FERRETERIA_TEST_CONNECTION` apuntando a MySQL; sin ella se omiten sin fallar.

## Documentación

- `FERRETERIA  Joel/Documentacion/Informe_SOLID_FerreteriaJoel.md` — informe de
  Clean Code y SOLID.
- `FERRETERIA  Joel/Documentacion/Guia_Defensa_SOLID.md` — guía de defensa
  (arquitectura, patrones, SOLID, async).
- `FERRETERIA  Joel/Documentacion/Reporte_Final_Entregable1_Ventas.md` — reporte
  del módulo de ventas.

## Puesta en marcha (ejecución local)

Secuencia para dejar el proyecto funcionando: **base de datos → configuración →
variables de entorno (opcional) → ejecución**.

### 1. Base de datos — ejecutar los SQL 1 y 2

En MySQL (Workbench, CLI, etc.) ejecuta, en este orden:

- **SQL 1 — `Scripts/esquema.sql`**: crea la base `bdferreteria`, todas las tablas
  (catálogo, clientes, empleados, ventas, histórico de precios) y los datos de
  prueba. Ya incluye las columnas del módulo de ventas (`EstadoVenta`,
  `ClaveIdempotencia`) y el índice único de correo del empleado.
- **SQL 2 — `Scripts/bitacora_seguridad.sql`**: crea la tabla `bitacora_sesion`
  (login/logout).

> Scripts adicionales: `Scripts/registro_seguridad.sql` es idempotente y ya no es
> necesario en bases nuevas; `Scripts/migracion_ventas.sql` sirve **solo** para
> actualizar una base creada con un esquema anterior.

### 2. Apuntar `appsettings` a tu base de datos local

Edita `FERRETERIA  Joel/appsettings.json` y ajusta la cadena de conexión a tu
MySQL local (usuario y contraseña propios):

```json
"ConnectionStrings": {
  "MySqlConnection": "Server=localhost;Database=bdferreteria;Uid=root;Pwd=tu_clave;"
}
```

Si el archivo `appsettings.json` no existe en tu copia (no está versionado),
créalo tomando `appsettings.Development.json` como base y define ahí la clave
`ConnectionStrings:MySqlConnection`.

### 3. `.env` (obligatorio para el envío de correos de activación)

Copia `.env.example` a `FERRETERIA  Joel/.env` (no versionado) y ajusta las
claves. El alta de empleados desde `/Registro` (exclusiva del administrador)
envía un enlace de activación por correo, por lo que se configura el SMTP:

```
ConnectionStrings__MySqlConnection=Server=localhost;Database=bdferreteria;Uid=root;Pwd=tu_clave;
UrlProteccion__Clave=una_clave_larga_para_proteger_los_tokens_de_url
Smtp__Host=smtp.tu-proveedor.com
Smtp__Puerto=587
Smtp__Usuario=cuenta@tu-proveedor.com
Smtp__Clave=tu_clave_smtp
Smtp__UsarSsl=true
Smtp__Remitente=no-reply@ferreteriajoel.com
Smtp__RemitenteNombre=Ferretería Joel
```

Si `Smtp__Host` queda vacío (por ejemplo en desarrollo), el enlace de
activación **no se envía** y se muestra en la propia pantalla de registro y en
el log, para poder probar el flujo sin servidor de correo.

El archivo `.env` se carga automáticamente al arrancar (`Program.cs`) y **no
sobrescribe** variables de entorno ya definidas en el sistema.

### 4. Compilar y ejecutar

```bash
dotnet run --project "FERRETERIA  Joel"
```

Luego abre el navegador en la URL que indique la consola (por defecto
`https://localhost:xxxx`) e inicia sesión con un usuario de prueba del seed
ADMINISTRADOR: joel.rios@ferreteria.com / admin123  
EMPLEADO VENTA: cualquier ` maria.lopez@ferreteria.com ` / clave123

**Nota:** El registro (`/Registro`) ahora es exclusivo del **Administrador**
(menú Administración → *Empleados y roles*) y permite crear cuentas con rol
**Administrador** o **EmpleadoVenta**. No se define contraseña en el alta: la
cuenta nace en estado *pendiente de activación* (`-1`) y el empleado establece
su contraseña desde el enlace enviado a su correo (válido 10 minutos). Estados
de la cuenta: `-1` pendiente de activación, `0` dada de baja, `1` activa. La
contraseña se almacena con **Argon2**.
