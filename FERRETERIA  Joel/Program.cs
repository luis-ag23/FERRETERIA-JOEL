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
using FERRETERIA__Joel.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
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
    var pagesToIgnore = new[] { "/Login", "/ProductoNuevo", "/ProductoEditar", "/CategoriaNueva", "/CategoriaEditar", "/MarcaNueva", "/MarcaEditar", "/ProveedorNuevo", "/ProveedorEditar", "/Venta", "/Logout" };
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

UrlProtector.Inicializar(
    builder.Configuration["UrlProteccion:Clave"]
        ?? throw new InvalidOperationException(
            "Falta la clave 'UrlProteccion:Clave' en la configuración."));

builder.Services.AddHttpContextAccessor();

// Redis
var redisConnectionString = builder.Configuration["Redis:Connection"]
    ?? builder.Configuration["Redis__Connection"]
    ?? Environment.GetEnvironmentVariable("Redis__Connection")
    ?? "redis:6379";

var redisMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

builder.Services.AddSingleton<RedisConnection>(sp => new RedisConnection(redisConnectionString));
builder.Services.AddSingleton<RedisCacheHelper>(sp =>
{
    var conn = sp.GetRequiredService<RedisConnection>();
    return new RedisCacheHelper(conn, "ferreteria:");
});
builder.Services.AddScoped<SesionRedisServicio>();
builder.Services.AddScoped<CacheSelectRedisServicio>();
builder.Services.AddSingleton<InfoContenedor>();

// DataProtection with Redis - shared keys across containers
builder.Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(redisMultiplexer, "FerreteriaJoel-DataProtection-Keys")
    .SetApplicationName("FerreteriaJoel");

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
builder.Services.AddScoped<ServicioBitacora>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

// Lee un archivo .env (clave=valor por línea) desde el directorio indicado y
// define cada variable de entorno si aún no existe. Las líneas vacías o que
// comienzan con '#' se ignoran.
static void CargarVariablesEntorno(string directorio)
{
    string ruta = Path.Combine(directorio, ".env");

    if (!File.Exists(ruta))
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