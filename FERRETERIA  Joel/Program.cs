using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Helpers;
using FERRETERIA__Joel.Infraestructura;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Correo;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Repositorios;
using FERRETERIA__Joel.Infraestructura.Seguridad;
using FERRETERIA__Joel.Infraestructura.Servicios;
using FERRETERIA__Joel.Seguridad;
using FERRETERIA__Joel.Servicios;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Carga las variables definidas en el archivo .env (no versionado): cadena de
// conexión a MySQL, clave del protector de URLs y parámetros SMTP.
CargarVariablesEntorno(builder.Environment.ContentRootPath);

// 'CreateBuilder' ya había materializado el proveedor de variables de entorno,
// por lo que las claves definidas en el .env recién ahora no están visibles en
// builder.Configuration (p. ej. Smtp:Host). Se vuelve a agregar el proveedor
// para que las lea.
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRazorPages(options =>
{
    // Páginas de gestión del catálogo: solo personal autenticado. Un visitante
    // que intente abrir cualquiera de estas rutas por URL es redirigido a /Login.
    options.Conventions.AuthorizePage("/Productos");
    options.Conventions.AuthorizePage("/ProductoNuevo");
    options.Conventions.AuthorizePage("/ProductoEditar");
    options.Conventions.AuthorizePage("/ProductoHistoricoPrecio");
    options.Conventions.AuthorizePage("/Categorias");
    options.Conventions.AuthorizePage("/CategoriaNueva");
    options.Conventions.AuthorizePage("/CategoriaEditar");
    options.Conventions.AuthorizePage("/Marcas");
    options.Conventions.AuthorizePage("/MarcaNueva");
    options.Conventions.AuthorizePage("/MarcaEditar");
    options.Conventions.AuthorizePage("/Proveedores");
    options.Conventions.AuthorizePage("/ProveedorNuevo");
    options.Conventions.AuthorizePage("/ProveedorEditar");

    // Deshabilitar filtro automático de antiforgery para páginas con POST - usamos autenticación por cookie + Redis
    var ignoreAntiforgery = new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute();
    var pagesToIgnore = new[] { "/Login", "/ProductoNuevo", "/ProductoEditar", "/CategoriaNueva", "/CategoriaEditar", "/MarcaNueva", "/MarcaEditar", "/ProveedorNuevo", "/ProveedorEditar", "/Venta", "/Logout", "/Personal" };
    foreach (var page in pagesToIgnore)
    {
        options.Conventions.AddPageApplicationModelConvention(page, model =>
        {
            model.Filters.Add(ignoreAntiforgery);
        });
    }
});

// Autenticación oficial del equipo por cookie. Ventas la reutiliza sin
// duplicarla: no se declara otra configuracion, cookie ni redirecciones.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.Cookie.Name = "FerreteriaJoel.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Deshabilitar validación automática de antiforgery (usamos autenticación por cookie + Redis)
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = true;
});

// La clave del protector de URLs no se versiona: llega desde el archivo .env
// (variable UrlProteccion__Clave) o desde las variables de entorno de Docker
// Compose. Si falta, la aplicación no arranca: es preferible a firmar tokens
// con una clave vacía.
string? claveUrlProteccion = builder.Configuration["UrlProteccion:Clave"];

if (string.IsNullOrWhiteSpace(claveUrlProteccion))
{
    throw new InvalidOperationException(
        "Falta la clave 'UrlProteccion:Clave'. Defina la variable de entorno "
        + "UrlProteccion__Clave (archivo .env de la raíz o docker-compose.yml).");
}

UrlProtector.Inicializar(claveUrlProteccion);

builder.Services.AddHttpContextAccessor();

// Redis
var redisConnectionString = builder.Configuration["Redis:Connection"]
    ?? builder.Configuration["Redis__Connection"]
    ?? Environment.GetEnvironmentVariable("Redis__Connection")
    ?? "redis:6379";

// Tiempo de vida de las entradas de caché, configurable desde el entorno.
int cacheTtlMinutos = 5;
int.TryParse(
    builder.Configuration["Redis:CacheTtlMinutes"]
        ?? builder.Configuration["Redis__CacheTtlMinutes"]
        ?? Environment.GetEnvironmentVariable("Redis__CacheTtlMinutes"),
    out cacheTtlMinutos);

var redisMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

builder.Services.AddSingleton<RedisConnection>(sp => new RedisConnection(redisConnectionString));
builder.Services.AddSingleton<RedisCacheHelper>(sp =>
{
    var conn = sp.GetRequiredService<RedisConnection>();
    return new RedisCacheHelper(
        conn,
        "ferreteria:",
        TimeSpan.FromMinutes(cacheTtlMinutos));
});
builder.Services.AddScoped<SesionRedisServicio>();

// Identificador real de la replica en ejecucion: su nombre (app1..app4) y
// el identificador unico que genera al arrancar.
builder.Services.AddSingleton<IIdentificadorReplica, IdentificadorReplica>();

// Puerto de la cache distribuida: patron cache-aside. Cada acierto o fallo
// queda en el log de la replica (clave y origen del dato); las claves se
// observan directamente en el contenedor de Redis.
builder.Services.AddSingleton<ICacheSeleccion, CacheSeleccionRedis>();

// DataProtection con Redis: las claves se comparten entre todas las réplicas
// para que una cookie emitida por app1 sea válida en app2, app3 y app4.
X509Certificate2? certificadoProteccion =
    ProteccionClaves.CargarCertificado(builder.Configuration);

builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redisMultiplexer, "FerreteriaJoel-DataProtection-Keys")
    .SetApplicationName("FerreteriaJoel");

if (certificadoProteccion is not null)
{
    // Cifra las claves de Data Protection con el certificado: sin él, las
    // claves se guardan en Redis en claro.
    builder.Services.AddDataProtection()
        .ProtectKeysWithCertificate(certificadoProteccion);
}

builder.Services.AddDistributedMemoryCache();
try
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "ferreteria_session:";
    });
}
catch { }

builder.Services.AddSession(options =>
{
    var sessionTtl = builder.Configuration["Redis:SessionTtlMinutes"] ?? Environment.GetEnvironmentVariable("Redis__SessionTtlMinutes");
    int ttlMin = 30;
    int.TryParse(sessionTtl, out ttlMin);
    options.IdleTimeout = TimeSpan.FromMinutes(ttlMin);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "FerreteriaJoel.Session";
});

builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

builder.Services.AddScoped<IConexionTransaccional, MySqlConexionTransaccional>();
builder.Services.AddScoped<IUnidadDeTrabajo, MySqlUnidadDeTrabajo>();

// Repositorios concretos inyectados directamente (operaciones especiales que
// no encajan en los contratos genéricos) y creadores (patrón Factory Method).
builder.Services.AddScoped<MySqlClienteRepository>();
builder.Services.AddScoped<MySqlEmpleadoRepository>();
builder.Services.AddScoped<MySqlHistoricoPrecioRepository>();
builder.Services.AddScoped<MySqlVentaRepository>();
builder.Services.AddScoped<MySqlDetalleVentaRepository>();
builder.Services.AddScoped<MySqlInventarioRepository>();
builder.Services.AddScoped<MySqlBitacoraRepository>();
builder.Services.AddScoped<MySqlProductoRepository>();
builder.Services.AddScoped<MySqlCategoriaRepository>();

// Contrato de personal: la misma implementación concreta, expuesta por su
// abstracción (baja lógica y proyecciones sin credenciales).
builder.Services.AddScoped<IRepositoryEmpleado>(sp =>
    sp.GetRequiredService<MySqlEmpleadoRepository>());

builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Producto>>, ProductoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Categoria>>, CategoriaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Proveedor>>, ProveedorRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Marca>>, MarcaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Empleado>>, EmpleadoRepositoryCreator>();

builder.Services.AddScoped<ServicioCategoria>();
builder.Services.AddScoped<ServicioProducto>();
builder.Services.AddScoped<ServicioProveedor>();
builder.Services.AddScoped<ServicioEmpleado>();
builder.Services.AddScoped<ServicioHistoricoPrecio>();
builder.Services.AddScoped<ServicioMarca>();
builder.Services.AddScoped<ServicioCliente>();

builder.Services.AddScoped<IContextoUsuario, ContextoUsuarioHttp>();

// Fachadas del módulo de Ventas (patrón Facade). Cada pantalla consume
// una sola fachada; las de venta y anulación delegan el inventario en la
// fachada de stock.
builder.Services.AddScoped<FachadaActualizacionStock>();
builder.Services.AddScoped<FachadaVenta>();
builder.Services.AddScoped<FachadaAnulacion>();

builder.Services.AddSingleton<IHasheadorContrasena, Argon2HasheadorContrasena>();
builder.Services.AddScoped<IEnviadorCorreo, SmtpEnviadorCorreo>();
builder.Services.AddScoped<IConstructorEnlaceActivacion, ConstructorEnlaceActivacion>();
builder.Services.AddSingleton<IGeneradorComprobantePdf, QuestPdfGeneradorComprobante>();
builder.Services.AddScoped<RegistroCredencialesValidador>();
builder.Services.AddScoped<ServicioRegistroEmpleado>();
builder.Services.AddScoped<ServicioActivacionCuenta>();
builder.Services.AddScoped<ServicioInicioSesion>();
builder.Services.AddScoped<ServicioPersonal>();
builder.Services.AddScoped<ServicioBitacora>();

// Nginx es el proxy frontal: reenvia la cabecera X-Forwarded-For, el
// protocolo original y el host. Se confia en sus cabeceras porque es el
// unico punto de entrada de la aplicacion dentro de la red de Docker.
//
// La configuracion debe registrarse antes de builder.Build(): despues del
// build, el proveedor de servicios ya esta construido y la opcion no se
// aplicaria.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;

    // Nginx corre dentro de la red de Docker, con una IP dinamica: no se
    // puede fijar la lista de redes o proxies conocidos. Se vacian para
    // confiar en las cabeceras reenviadas por el unico proxy de entrada.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (certificadoProteccion is not null)
{
    app.Logger.LogInformation(
        "Las claves de Data Protection se cifran con el certificado compartido: {Certificado}",
        certificadoProteccion.Thumbprint);
}
else
{
    app.Logger.LogWarning(
        "No se encontro certificado de Data Protection: las claves se guardan en Redis sin cifrar con un certificado adicional.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();

// Identificador unico de la replica que atendio la peticion. Cada replica
// genera el suyo al arrancar (nombre de la replica mas un token aleatorio:
// app1-3F2A9C77), de modo que la cabecera identifica al proceso real que
// respondio y no solo al contenedor. Nginx lo reenvia al cliente
// (proxy_pass_header X-Replica-Id): es la evidencia de que el escalamiento
// horizontal esta distribuyendo las peticiones entre instancias distintas.
string idReplica =
    app.Services.GetRequiredService<IIdentificadorReplica>().IdInstancia;

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Replica-Id"] = idReplica;

    await next();
});

// La redireccion a HTTPS solo se activa si hay un puerto HTTPS configurado.
// En el laboratorio, Nginx termina la conexion en HTTP: activarla sin un
// listener HTTPS solo produciria advertencias en el log.
bool hayPuertoHttps = !string.IsNullOrWhiteSpace(
    builder.Configuration["https_port"]
        ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT")
        ?? Environment.GetEnvironmentVariable("HTTPS_PORT"));

if (hayPuertoHttps)
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Sonda de salud: la usan Docker (healthcheck) y Nginx para comprobar que la
// replica responde. Devuelve tambien el identificador real de la replica
// que atendio la peticion.
app.MapGet("/health", async (
    IIdentificadorReplica replica,
    RedisConnection redis,
    CancellationToken cancelacion) =>
{
    bool redisDisponible = false;

    try
    {
        await redis.Db.PingAsync()
            .WaitAsync(TimeSpan.FromSeconds(2), cancelacion);

        redisDisponible = true;
    }
    catch
    {
        redisDisponible = false;
    }

    return Results.Ok(new
    {
        // La replica responde, pero sin Redis se pierde la sesion compartida
        // y la cache distribuida: se informa como degradada.
        estado = redisDisponible ? "saludable" : "degradado",
        replica = replica.Id,
        idInstancia = replica.IdInstancia,
        redis = redisDisponible,
        fecha = DateTime.UtcNow
    });
});

app.Run();

// Lee un archivo .env (clave=valor por línea) y define cada variable de
// entorno si aún no existe. Las líneas vacías o que comienzan con '#' se
// ignoran.
//
// El archivo se busca primero en el directorio indicado y luego en sus
// directorios padre: así el .env de la raíz del repositorio sirve tanto
// para Docker Compose como para una ejecución local de la aplicación.
static void CargarVariablesEntorno(string directorio)
{
    string? ruta = BuscarArchivoEnv(directorio);

    if (ruta is null)
    {
        return;
    }

    foreach (string linea in File.ReadAllLines(ruta))
    {
        string texto = linea.Trim();

        if (texto.Length == 0 || texto.StartsWith('#'))
        {
            continue;
        }

        int separador = texto.IndexOf('=');

        if (separador <= 0)
        {
            continue;
        }

        string clave = texto[..separador].Trim();
        string valor = texto[(separador + 1)..].Trim().Trim('"');

        if (Environment.GetEnvironmentVariable(clave) is null)
        {
            Environment.SetEnvironmentVariable(clave, valor);
        }
    }
}

// Busca el archivo .env en el directorio recibido y, si no lo encuentra, sube
// hasta cuatro niveles. Devuelve la ruta completa o null.
static string? BuscarArchivoEnv(string directorio)
{
    string? actual = Path.GetFullPath(directorio);

    for (int nivel = 0; nivel < 5 && actual is not null; nivel++)
    {
        string ruta = Path.Combine(actual, ".env");

        if (File.Exists(ruta))
        {
            return ruta;
        }

        DirectoryInfo? padre = Directory.GetParent(actual);

        actual = padre?.FullName;
    }

    return null;
}