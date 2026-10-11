using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Helpers;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Seguridad;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Carga las variables definidas en el archivo .env (no versionado) y expone
// la clave de alta de empleados a través de la configuración.
CargarVariablesEntorno(builder.Environment.ContentRootPath);

string? claveEmpleado =
    Environment.GetEnvironmentVariable("REGISTRO_CLAVE_EMPLEADO");

if (!string.IsNullOrWhiteSpace(claveEmpleado))
{
    builder.Configuration["Registro:ClaveEmpleado"] = claveEmpleado;
}

builder.Services.AddRazorPages(options =>
{
    // Páginas de gestión: requieren sesión iniciada. Un visitante que intente
    // abrir cualquiera de estas rutas por URL es redirigido a /Login.
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

    // Páginas de consulta (rol Usuario): requieren sesión iniciada.
    options.Conventions.AuthorizePage("/ProductosUsuario");
    options.Conventions.AuthorizePage("/CategoriasUsuario");
    options.Conventions.AuthorizePage("/MarcasUsuario");
    options.Conventions.AuthorizePage("/ProveedoresUsuario");
});

// Autenticación por cookie: el login (clientes) emite la cookie de sesión
// y [Authorize] protege las páginas de gestión.
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

UrlProtector.Inicializar(
    builder.Configuration["UrlProteccion:Clave"]
        ?? throw new InvalidOperationException(
            "Falta la clave 'UrlProteccion:Clave' en la configuración."));


builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();


builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Producto>>, ProductoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Categoria>>, CategoriaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Proveedor>>, ProveedorRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Marca>>, MarcaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IEmpleadoRepository>, EmpleadoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IHistoricoPrecioRepository>, HistoricoPrecioRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IClienteRepository>, ClienteRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IBitacoraRepository>, BitacoraRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Venta>>, VentaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<DetalleVenta>>, DetalleVentaRepositoryCreator>();


builder.Services.AddScoped<ServicioCategoria>();
builder.Services.AddScoped<ServicioProducto>();
builder.Services.AddScoped<ServicioProveedor>();
builder.Services.AddScoped<ServicioEmpleado>();
builder.Services.AddScoped<ServicioHistoricoPrecio>();
builder.Services.AddScoped<ServicioMarca>();

builder.Services.AddSingleton<IHasheadorContrasena, Argon2HasheadorContrasena>();
builder.Services.AddScoped<ServicioRegistroEmpleado>();
builder.Services.AddScoped<ServicioRegistroUsuario>();
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
