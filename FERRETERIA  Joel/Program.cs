using FERRETERIA__Joel.Aplicacion.Politicas;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Helpers;
using FERRETERIA__Joel.Infraestructura;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// ------------------------------------------------------------------
// Punto de integracion con el modulo de autenticacion del equipo.
// Ventas NO implementa login: solo declara la proteccion ([Authorize] /
// rol Administrador). Hasta que exista el modulo externo, la cookie no
// se emite y las operaciones protegidas rechazan el acceso (401/403);
// nunca se ejecutan acciones anonimas.
// ------------------------------------------------------------------
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

UrlProtector.Inicializar(
    builder.Configuration["UrlProteccion:Clave"]
        ?? throw new InvalidOperationException(
            "Falta la clave 'UrlProteccion:Clave' en la configuración."));


builder.Services.AddHttpContextAccessor();

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


builder.Services.AddScoped<ServicioCategoria>();
builder.Services.AddScoped<ServicioProducto>();
builder.Services.AddScoped<ServicioProveedor>();
builder.Services.AddScoped<ServicioEmpleado>();
builder.Services.AddScoped<ServicioHistoricoPrecio>();
builder.Services.AddScoped<ServicioMarca>();
builder.Services.AddScoped<ServicioCliente>();


builder.Services.AddScoped<IContextoUsuario, ContextoUsuarioHttp>();


var descuentos = new ConfiguracionDescuentos();

builder.Services.AddSingleton(descuentos);
builder.Services.AddSingleton<IPoliticaDescuento>(
    new DescuentoPorMonto(
        montoMinimo: descuentos.MontoMinimo,
        porcentaje: descuentos.PorcentajePorMonto));
builder.Services.AddSingleton<IPoliticaDescuento>(
    new DescuentoPorCantidad(
        cantidadMinima: descuentos.CantidadMinima,
        porcentaje: descuentos.PorcentajePorCantidad));
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

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
