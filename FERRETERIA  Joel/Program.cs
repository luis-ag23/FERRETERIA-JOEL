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


builder.Services.AddScoped<RepositoryCreator<IRepository<Producto>>, ProductoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Categoria>>, CategoriaRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Proveedor>>, ProveedorRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<Empleado>>, EmpleadoRepositoryCreator>();
builder.Services.AddScoped<RepositoryCreator<IRepository<HistoricoPrecio>>, HistoricoPrecioRepositoryCreator>();

builder.Services.AddScoped<IRepository<Producto>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<Producto>>>().CreateRepository());
builder.Services.AddScoped<IRepository<Categoria>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<Categoria>>>().CreateRepository());
builder.Services.AddScoped<IRepository<Proveedor>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<Proveedor>>>().CreateRepository());
builder.Services.AddScoped<IRepository<Empleado>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<Empleado>>>().CreateRepository());
builder.Services.AddScoped<IRepository<HistoricoPrecio>>(provider =>
    provider.GetRequiredService<RepositoryCreator<IRepository<HistoricoPrecio>>>().CreateRepository());

builder.Services.AddScoped<IModificacionRepository<Producto>, MySqlProductoRepository>();
builder.Services.AddScoped<IModificacionRepository<Categoria>, MySqlCategoriaRepository>();
builder.Services.AddScoped<IModificacionRepository<Proveedor>, MySqlProveedorRepository>();
builder.Services.AddScoped<IModificacionRepository<Empleado>, MySqlEmpleadoRepository>();
builder.Services.AddScoped<MySqlHistoricoPrecioRepository>();
builder.Services.AddScoped<IHistoricoPrecioRepository>(provider =>
    provider.GetRequiredService<MySqlHistoricoPrecioRepository>());

builder.Services.AddScoped<IMarcaRepository, MySqlMarcaRepository>();

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
