using FERRETERIA__Joel.Aplicacion.Politicas;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Helpers;
using FERRETERIA__Joel.Infraestructura;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Seguridad;
using FERRETERIA__Joel.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToAccessDenied = context =>
            {
                context.Response.Redirect("/");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

UrlProtector.Inicializar(
    builder.Configuration["UrlProteccion:Clave"]
        ?? throw new InvalidOperationException(
            "Falta la clave 'UrlProteccion:Clave' en la configuración."));


builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();


builder.Services.AddScoped<IConexionTransaccional, MySqlConexionTransaccional>();
builder.Services.AddScoped<IUnidadDeTrabajo, MySqlUnidadDeTrabajo>();


builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Producto>>, ProductoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Categoria>>, CategoriaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Proveedor>>, ProveedorRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Marca>>, MarcaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Empleado>>, EmpleadoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IHistoricoPrecioRepository>, HistoricoPrecioRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IClienteRepository>, ClienteRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IVentaRepository>, VentaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IDetalleVentaRepository>, DetalleVentaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IInventarioRepository>, InventarioRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IAutenticacionRepository>, AutenticacionRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<ISesionRepository>, SesionRepositoryCreator>();


builder.Services.AddScoped<ServicioCategoria>();
builder.Services.AddScoped<ServicioProducto>();
builder.Services.AddScoped<ServicioProveedor>();
builder.Services.AddScoped<ServicioEmpleado>();
builder.Services.AddScoped<ServicioHistoricoPrecio>();
builder.Services.AddScoped<ServicioMarca>();

builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddScoped<IContextoUsuario, ContextoUsuarioHttp>();
builder.Services.AddScoped<ServicioAutenticacion>();
builder.Services.AddScoped<ServicioSesion>();


builder.Services.AddSingleton<IPoliticaDescuento>(
    new DescuentoPorMonto(montoMinimo: 1000m, porcentaje: 5m));
builder.Services.AddSingleton<IPoliticaDescuento>(
    new DescuentoPorCantidad(cantidadMinima: 12, porcentaje: 3m));
builder.Services.AddScoped<SelectorDescuento>();
builder.Services.AddScoped<FachadaVenta>();

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

app.UseSession();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
