# FERRETERIA Joel

Sistema administrativo web para una ferretería, construido con **ASP.NET Core
Razor Pages (.NET 10)** y **arquitectura hexagonal (puertos y adaptadores)**,
dividida en proyectos por capa. Incluye catálogo con CRUD, histórico de precios,
seguridad con roles y un módulo de ventas transaccional.

Este entregable además despliegue el sistema sobre **cuatro réplicas** del
monolito detrás de un balanceador **Nginx**, con **Redis** como caché
distribuida compartida entre todas las réplicas.

Repositorio: https://github.com/Macacofig/FERRETERIA-Joel

## Características

- **Catálogo** con CRUD completo: Categorías, Marcas, Productos y Proveedores,
  con validaciones de negocio y eliminación lógica (soft delete).
- **Histórico de precios** por vigencia, con vista de cambios (`ProductoHistoricoPrecio`).
- **Seguridad**: inicio/cierre de sesión por cookie **solo para empleados**, roles
  diferenciados y bitácora de sesiones. Contraseñas con **Argon2**.
- **Personal**: alta de cuentas con activación por correo, **baja lógica** y
  reactivación desde la vista `/Personal`. El listado se consulta con una
  **proyección sin credenciales**: la contraseña nunca sale de la base de datos
  hacia la interfaz.
- **Ventas**: registro con carrito, historial, comprobante y **anulación
  lógica**, todo sobre **transacciones atómicas**.
- **Escalamiento horizontal**: cuatro réplicas detrás de Nginx, con
  **failover** explícito, caché **cache-aside** en Redis compartida y
  **Data Protection** con claves cifradas y replicadas.
- **Asincronía de extremo a extremo** (`async`/`await` hasta MySQL).

## Arquitectura

Monolito con arquitectura **hexagonal** (puertos y adaptadores). La regla de
dependencia siempre apunta hacia el Dominio:

```
Web ─▶ Aplicacion ─▶ Dominio ◀─ Infraestructura
```

| Proyecto | Responsabilidad |
|----------|-----------------|
| `FERRETERIA__Joel.Dominio` | Núcleo: `Entidades/`, `Puertos/`, `Modelos/`, `Validaciones/`, `ConfiguracionValidacion/`, `Constantes/`, `Utilidades/` |
| `FERRETERIA__Joel.Aplicacion` | Casos de uso: `Servicios/` (servicios y fachadas), `Modelos/` (DTOs) |
| `FERRETERIA__Joel.Infraestructura` | Adaptadores de salida: `Repositorios/` (MySQL), `Conexion/`, `Factories/`, `Seguridad/`, `Servicios/` (Redis) |
| `FERRETERIA  Joel` (Web) | Adaptador de entrada: `Pages/` (Razor Pages), `Helpers/`, `Seguridad/`, `Program.cs` |
| `FERRETERIA__Joel.Pruebas` | Pruebas unitarias e integración (xUnit) |

### Patrones de diseño

**Facade** (`FachadaVenta`, `FachadaAnulacion`, `FachadaActualizacionStock`),
**Factory Method** (`RepositoryCreator<TContrato>` + `Infraestructura/Factories/`),
**Repository** (`MySql*Repository`) y **Unit of Work** (`IUnidadDeTrabajo`).

### Roles

| Rol | Alcance |
|-----|---------|
| `Administrador` | Gestión total del catálogo y del personal; en ventas solo visualiza (no anula) |
| `EmpleadoVenta` | Registra ventas, ve el historial y **puede anularlas** |

## Requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download)
- MySQL 8+ (base de trabajo de desarrollo: `bdferreteria`)
- Docker + Docker Compose (para el despliegue con Nginx y Redis)

## Configuración

La aplicación **no guarda ningún secreto en los archivos versionados**: los
`appsettings.json` se entregan con la cadena de conexión y la clave del
protector de URLs vacías. Todo lo sensible llega por variables de entorno.

La forma recomendada es el archivo `.env` de la **raíz del repositorio** (no
versionado). La aplicación lo carga al arrancar: lo busca en su carpeta y en
las carpetas padre, y **no sobrescribe** las variables que ya existan en el
sistema. Docker Compose lee ese mismo archivo para resolver los `${VARIABLE}`
de `docker-compose.yml`.

Las variables de la aplicación usan doble guion bajo (`__`), porque así es como
ASP.NET Core traduce una variable de entorno a una clave de configuración con
secciones:

```
ConnectionStrings__MySqlConnection=Server=localhost;Database=bdferreteria;Uid=root;Pwd=tu_clave;
UrlProteccion__Clave=una_clave_larga_para_proteger_los_tokens_de_url
```

Si falta la cadena de conexión o la clave del protector, la aplicación **no
arranca** y explica qué variable falta: es preferible a firmar los tokens con
una clave vacía.

### Secretos

**Ningún secreto está versionado.** Toda la configuración sensible vive en el
archivo `.env` de la raíz (ignorado por Git) y en las variables de entorno que
Docker Compose toma de él:

| Variable | Para qué sirve |
|----------|----------------|
| `MYSQL_ROOT_PASSWORD` | Contraseña del usuario `root` de MySQL |
| `MYSQL_DATABASE` | Nombre de la base de datos |
| `URL_PROTECCION_CLAVE` | Clave del protector de URLs firmadas (enlaces de activación) |
| `SMTP_USUARIO`, `SMTP_CLAVE` | Credenciales del servidor de correo |
| `HTTP_PUERTO_HOST` | Puerto del host donde escucha Nginx |

Para empezar, copie la plantilla y complete los valores:

```bash
cp .env.example .env
```

> **Rotación de la clave SMTP.** La clave de aplicación que se usó en
> anteriores versiones del repositorio debe considerarse filtrada. Rote la
> clave en el proveedor de correo y deje la nueva **solo** en `.env`.

## Base de datos

> **`Scripts/esquema.sql` es el esquema canónico.** Es el que coincide con el
> código C# (los repositorios leen y escriben exactamente esas columnas).
> `Scripts/init-completo.sql` es el script de arranque que monta Docker
> (`/docker-entrypoint-initdb.d/01-init.sql`): contiene el mismo esquema más la
> tabla `bitacora_sesion` y los datos semilla. **Ambos deben mantenerse
> alineados**: si divergen, la aplicación falla contra la base recién creada
> (por ejemplo, con `Unknown column 'TotalProducto' in 'field list'` al
> registrar una venta).

Ejecutar los scripts de `Scripts/` en orden sobre MySQL:

1. `Scripts/esquema.sql` — esquema base (catálogo, clientes, empleados, ventas).
2. `Scripts/registro_seguridad.sql` — índice único de correo del empleado (login).
3. `Scripts/bitacora_seguridad.sql` — tabla de bitácora de sesiones.
4. `Scripts/migracion_ventas.sql` — columnas y restricciones del módulo de ventas
   (anulación) y limpieza de credenciales del cliente.
5. `Scripts/migracion_cliente_ci.sql`

## Escalamiento horizontal (EC2)

### Topología

```
                    ┌──────────────┐
   Cliente ────────▶│    Nginx     │  balanceo round-robin + failover
                    │  :8080       │
                    └──────┬───────┘
             ┌─────────────┼─────────────┬─────────────┐
             ▼             ▼             ▼             ▼
          ┌──────┐     ┌──────┐     ┌──────┐     ┌──────┐
          │ app1 │     │ app2 │     │ app3 │     │ app4 │   réplicas ASP.NET Core
          └──┬───┘     └──┬───┘     └──┬───┘     └──┬───┘
             └─────────────┼─────────────┼─────────────┘
                           ▼             ▼
                     ┌──────────┐  ┌──────────┐
                     │  MySQL   │  │  Redis   │  caché distribuida
                     └──────────┘  └──────────┘
```

Los cuatro contenedores de aplicación son **idénticos** (misma imagen) y
difieren solo en el identificador de réplica (`HOSTNAME=app1…app4`). Todo el
estado compartido (sesiones, claves de Data Protection, caché) vive en Redis,
de modo que cualquier réplica puede atender cualquier petición.

### Levantar el sistema

```bash
# 1. Preparar los secretos
cp .env.example .env
#    edite .env y complete MYSQL_ROOT_PASSWORD y URL_PROTECCION_CLAVE

# 2. Construir la imagen y levantar el conjunto
docker compose up -d --build

# 3. Ver el estado de cada réplica
docker compose ps

# 4. Seguir el log de una réplica concreta
docker compose logs -f app3
```

La aplicación queda disponible en `http://localhost` (o en el puerto definido en
`HTTP_PUERTO_HOST`).

### Balanceo y failover

`nginx/nginx.conf` define el *upstream* con las cuatro réplicas:

```nginx
server app1:8080 max_fails=3 fail_timeout=10s;
server app2:8080 max_fails=3 fail_timeout=10s;
server app3:8080 max_fails=3 fail_timeout=10s;
server app4:8080 max_fails=3 fail_timeout=10s;

proxy_next_upstream error timeout http_502 http_503 http_504;
proxy_next_upstream_tries 4;
```

* Tras `max_fails` errores en `fail_timeout` segundos, la réplica se marca como
  no disponible y Nginx deja de enviarle peticiones.
* Si una réplica no responde, la **misma** petición se reenvía a otra sin que
  el cliente lo note (`proxy_next_upstream`).
* Al vencer `fail_timeout`, la réplica vuelve a entrar en la rotación.

### Evidencias de escalamiento

* **Cabecera `X-Replica-Id`**: la escribe la réplica que atendió la petición y
  Nginx la reenvía al cliente. `curl -I http://localhost/` muestra siempre la
  réplica que respondió.
* **Cabecera `X-Replica-Served`**: la escribe Nginx con la dirección del
  *upstream* que atendió la petición.
* **Vista `/DiagnosticoCache`** (solo administrador): lista las claves que hay
  en Redis con su tiempo de vida restante y el registro compartido de los
  últimos 50 accesos (HIT/MISS, clave, réplica y origen del dato).
* **Sonda `/health`**: devuelve el estado de la réplica y si Redis responde.
  La usan el *healthcheck* de Docker y el balanceador.

```bash
curl -s http://localhost/health
# {"estado":"saludable","replica":"app2","redis":true,"fecha":"..."}
```

### Caché distribuida (Redis)

Los repositorios de catálogo (productos, categorías, marcas y proveedores) usan
el patrón **cache-aside** a través del puerto `ICacheSeleccion`:

* **Lectura**: se consulta Redis; si no está la clave, se lee MySQL y el
  resultado se guarda en Redis con el TTL configurado
  (`Redis:CacheTtlMinutes`, 5 minutos por defecto).
* **Invalidación**: al insertar, actualizar o cambiar el estado de un registro
  se borra el prefijo de claves de esa tabla, de modo que la siguiente lectura
  vuelva a la base de datos.
* **Registro**: cada acceso se anota como `HIT` o `MISS` con la clave, la
  réplica que lo produjo y el origen real del dato.

Como la caché es la **misma** para las cuatro réplicas, un dato cargado por
`app1` es servido desde Redis por `app3` sin volver a consultar MySQL.

### Data Protection compartido y cifrado

Las cookies de sesión y de antiforgery se cifran con las claves de Data
Protection. Para que la sesión sobreviva al cambio de réplica:

1. Las claves se guardan en **Redis** (`PersistKeysToStackExchangeRedis`), de
   modo que las cuatro réplicas las comparten.
2. Las claves se **cifran con un certificado X.509** (`ProtectKeysWithCertificate`).
   Sin este paso, las claves quedarían almacenadas en Redis en claro.

El certificado lo genera el *entrypoint* del contenedor (`docker/entrypoint.sh`)
en el volumen Docker compartido `proteccion_datos`, de forma atómica. **Tiene
que ser el mismo en las cuatro réplicas**: si cada una usara uno distinto, una
cookie emitida por `app1` no se podría descifrar en `app2` y el inicio de
sesión se rompería al cambiar de réplica.

### Comprobación del failover

```bash
# Detener una réplica y comprobar que el sistema sigue respondiendo
docker compose stop app2

for i in $(seq 1 6); do
  curl -s -o /dev/null -D - http://localhost/health | grep -i x-replica-id
done
```

Las peticiones se siguen atendiendo y la cabecera `X-Replica-Id` deja de
mostrar `app2`. Para volver a incorporarla:

```bash
docker compose start app2
```

## Compilar, probar y ejecutar

```bash
# Compilar la solución completa
dotnet build "FERRETERIA  Joel.slnx"

# Ejecutar las pruebas
dotnet test "FERRETERIA  Joel.slnx"

# Ejecutar la aplicación web
dotnet run --project "FERRETERIA  Joel"
```

### Pruebas de integración

La suite se divide en pruebas unitarias (siempre se ejecutan) y pruebas de
integración **reales contra MySQL** (ventas, stock, anulación, personal y baja
lógica). Estas últimas solo se activan si la variable de entorno
`FERRETERIA_TEST_CONNECTION` apunta a una base de desarrollo; sin ella se
omiten sin fallar:

```bash
# PowerShell
# La clave es la misma que MYSQL_ROOT_PASSWORD del archivo .env
$env:FERRETERIA_TEST_CONNECTION="Server=localhost;Port=3307;Database=bdferreteria;Uid=root;Pwd=<tu_clave>;"
dotnet test "FERRETERIA  Joel.slnx"
```

Con la variable definida se ejecutan las **119** pruebas (109 unitarias + 10 de
integración real: 5 del módulo de ventas y 5 del de personal). Con el
despliegue de Docker levantando, el puerto `3307` es el que MySQL publica en el
host.

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
  prueba. Ya incluye las columnas del módulo de ventas (`EstadoVenta`) y el
  índice único de correo del empleado.
- **SQL 2 — `Scripts/bitacora_seguridad.sql`**: crea la tabla `bitacora_sesion`
  (login/logout).

> Scripts adicionales: `Scripts/registro_seguridad.sql` es idempotente y ya no es
> necesario en bases nuevas; `Scripts/migracion_ventas.sql` sirve **solo** para
> actualizar una base creada con un esquema anterior.

### 2. Configurar la conexión (archivo `.env`)

La configuración **no va en `appsettings.json`**: ese archivo se versiona y
queda con la cadena de conexión vacía a propósito. Se ajusta todo en el
archivo `.env` de la raíz (no versionado), que es el mismo que usa Docker
Compose.

Copia la plantilla y pon tus credenciales:

```bash
cp .env.example .env
```

```
MYSQL_ROOT_PASSWORD=tu_clave_de_mysql
MYSQL_DATABASE=bdferreteria
URL_PROTECCION_CLAVE=una_clave_larga_para_proteger_los_tokens_de_url

ConnectionStrings__MySqlConnection=Server=localhost;Database=bdferreteria;Uid=root;Pwd=tu_clave_de_mysql;
UrlProteccion__Clave=una_clave_larga_para_proteger_los_tokens_de_url
```

> `ConnectionStrings__MySqlConnection` es la que usa la aplicación cuando se
> ejecuta fuera de Docker (host `localhost`). Dentro de los contenedores la
> define el `docker-compose.yml` con el host `mysql`.

### 3. `.env` (obligatorio para el envío de correos de activación)

El mismo archivo `.env` del punto anterior sirve para el correo. El alta de
empleados desde `/Registro` (exclusiva del administrador) envía un enlace de
activación por correo, por lo que se configura el SMTP:

```
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

El archivo `.env` se carga automáticamente al arrancar (`Program.cs`), se busca
en la carpeta de la aplicación **y en sus carpetas padre**, y **no sobrescribe**
variables de entorno ya definidas en el sistema.

### 4. Compilar y ejecutar

```bash
dotnet run --project "FERRETERIA  Joel"
```

Luego abre el navegador en la URL que indique la consola y inicia sesión con un
usuario de prueba del seed
ADMINISTRADOR: joel.rios@ferreteria.com / admin123
EMPLEADO VENTA: `maria.lopez@ferreteria.com` / clave123

**Nota:** el registro (`/Registro`) es exclusivo del **Administrador**
(menú Administración → *Empleados y roles*) y permite crear cuentas con rol
**Administrador** o **EmpleadoVenta**. No se define contraseña en el alta: la
cuenta nace en estado *pendiente de activación* (`-1`) y el empleado establece
su contraseña desde el enlace enviado a su correo (válido 10 minutos). Estados
de la cuenta: `-1` pendiente de activación, `0` dada de baja, `1` activa. La
contraseña se almacena con **Argon2**.

La **baja lógica** se hace desde `/Personal` (Administración → *Personal y
bajas*): la cuenta no se borra, pasa a estado `0` y deja de poder iniciar
sesión, pero el histórico de ventas y la auditoría se conservan. La cuenta se
puede reactivar, qued pendiente de activación.
