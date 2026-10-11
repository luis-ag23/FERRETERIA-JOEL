using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Helpers;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Repositorios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

UrlProtector.Inicializar(
    builder.Configuration["UrlProteccion:Clave"]
        ?? throw new InvalidOperationException(
            "Falta la clave 'UrlProteccion:Clave' en la configuración."));


builder.Services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();


builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Producto>>, ProductoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Categoria>>, CategoriaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Proveedor>>, ProveedorRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IGestionarRepository<Marca>>, MarcaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Empleado>>, EmpleadoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<HistoricoPrecio>>, HistoricoPrecioRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Cliente>>, ClienteRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Venta>>, VentaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<DetalleVenta>>, DetalleVentaRepositoryCreator>();

builder.Services.AddScoped<IGestionarRepository<Producto>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IGestionarRepository<Producto>>>().CreateRepository());
builder.Services.AddScoped<IGestionarRepository<Categoria>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IGestionarRepository<Categoria>>>().CreateRepository());
builder.Services.AddScoped<IGestionarRepository<Proveedor>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IGestionarRepository<Proveedor>>>().CreateRepository());
builder.Services.AddScoped<IGestionarRepository<Marca>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IGestionarRepository<Marca>>>().CreateRepository());
builder.Services.AddScoped<IRepository<Empleado>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<Empleado>>>().CreateRepository());
builder.Services.AddScoped<IRepository<HistoricoPrecio>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<HistoricoPrecio>>>().CreateRepository());
builder.Services.AddScoped<IRepository<Cliente>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<Cliente>>>().CreateRepository());
builder.Services.AddScoped<IRepository<Venta>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<Venta>>>().CreateRepository());
builder.Services.AddScoped<IRepository<DetalleVenta>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<DetalleVenta>>>().CreateRepository());

builder.Services.AddScoped<MySqlHistoricoPrecioRepository>();
builder.Services.AddScoped<IHistoricoPrecioRepository>(provider =>
    provider.GetRequiredService<MySqlHistoricoPrecioRepository>());

builder.Services.AddScoped<ServicioCategoria>();
builder.Services.AddScoped<ServicioProducto>();
builder.Services.AddScoped<ServicioProveedor>();
builder.Services.AddScoped<ServicioEmpleado>();
builder.Services.AddScoped<ServicioHistoricoPrecio>();
builder.Services.AddScoped<ServicioMarca>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
