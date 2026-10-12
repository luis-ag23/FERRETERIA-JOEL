using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace FERRETERIA__Joel.Servicios;

/// <summary>
/// Herramienta temporal de integración para probar Ventas SIN implementar
/// el login real del equipo. Solo se registra en Development y si el
/// desarrollador habilita Ventas__HabilitarAccesoLocal=true.
/// Además rechaza solicitudes no originadas en loopback.
/// No puede utilizarse como sistema de autenticación en producción.
/// </summary>
public static class AccesoLocalVentasDesarrollo
{
    public static void MapAccesoLocalVentasDesarrollo(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()
            || !app.Configuration.GetValue<bool>("Ventas:HabilitarAccesoLocal"))
        {
            return;
        }

        app.MapGet("/dev/ventas-acceso", async (
            HttpContext contexto,
            IAntiforgery antiforgery,
            RepositoryCreator<IEmpleadoVentaRepository> creador,
            ILoggerFactory logs) =>
        {
            if (!EsLocal(contexto)) return Results.NotFound();

            try
            {
                var empleados = (await creador.CreateRepository().ListarAsync())
                    .Where(e => e.Estado == 1)
                    .OrderBy(e => e.Nombre)
                    .ToList();

                var token = antiforgery.GetAndStoreTokens(contexto);
                var enc = HtmlEncoder.Default;
                string opciones = string.Join("\n", empleados.Select(e =>
                    $"<option value=\"{e.IdEmpleado}\">" +
                    enc.Encode($"{e.Nombre} {e.Apellido} · {e.Rol}") + "</option>"));

                string html = $$"""
                    <!doctype html>
                    <html lang="es">
                    <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>Acceso local — Ventas</title>
                    <style>
                    body{font:16px system-ui,sans-serif;background:#f4f4f6;margin:0;display:grid;place-items:center;min-height:100vh;color:#222}
                    main{background:#fff;border:1px solid #ddd;border-radius:12px;max-width:430px;padding:28px;box-shadow:0 6px 25px #0001}
                    h1{font-size:24px;margin:0 0 12px}p{line-height:1.6}select,button{box-sizing:border-box;width:100%;padding:12px;margin:8px 0 15px}
                    button{background:#ffc107;border:0;border-radius:8px;font-weight:bold;cursor:pointer}
                    .aviso{font-size:13px;color:#a14c00}
                    </style></head><body><main>
                    <h1>Acceso local de Ventas</h1>
                    <p>Seleccione un empleado <strong>activo y existente en la BD de desarrollo</strong> para probar el módulo.</p>
                    <form method="post" action="/dev/ventas-acceso">
                        <input type="hidden" name="{{enc.Encode(token.FormFieldName)}}" value="{{enc.Encode(token.RequestToken ?? "")}}">
                        <label for="idEmpleado">Empleado de prueba</label>
                        <select id="idEmpleado" name="idEmpleado" required>{{opciones}}</select>
                        <button type="submit">Entrar a Ventas</button>
                    </form>
                    <p class="aviso">Solo localhost + Development + habilitación explícita. No es el login definitivo.</p>
                    </main></body></html>
                    """;
                return Results.Content(html, "text/html; charset=utf-8");
            }
            catch (Exception ex)
            {
                logs.CreateLogger("AccesoLocalVentasDesarrollo")
                    .LogError(ex, "No se pudo consultar la BD de desarrollo para acceso a Ventas");
                return Results.Problem(
                    "No se pudo consultar empleados de prueba. Revise la conexión MySQL y la base de desarrollo.",
                    statusCode: 503);
            }
        });

        app.MapPost("/dev/ventas-acceso", async (
            HttpContext contexto,
            IAntiforgery antiforgery,
            RepositoryCreator<IEmpleadoVentaRepository> creador,
            ILoggerFactory logs) =>
        {
            if (!EsLocal(contexto)) return Results.NotFound();

            try
            {
                await antiforgery.ValidateRequestAsync(contexto);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest("Formulario caducado o token antifalsificación inválido.");
            }

            var form = await contexto.Request.ReadFormAsync();
            if (!int.TryParse(form["idEmpleado"].ToString(), out int idEmpleado)
                || idEmpleado <= 0)
            {
                return Results.BadRequest("Seleccione un empleado válido.");
            }

            try
            {
                var empleado = (await creador.CreateRepository().ListarAsync())
                    .SingleOrDefault(e => e.IdEmpleado == idEmpleado && e.Estado == 1);
                if (empleado is null)
                    return Results.BadRequest("El empleado no existe o no está activo.");

                string nombre = $"{empleado.Nombre} {empleado.Apellido}".Trim();
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, empleado.IdEmpleado.ToString()),
                    new Claim(ClaimTypes.Name, nombre),
                    new Claim(ClaimTypes.Role, RolesSistema.Normalizar(empleado.Rol)),
                    new Claim(ContextoUsuarioHttp.ClaimNombreCompleto, nombre)
                };
                var identidad = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await contexto.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identidad),
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2),
                        AllowRefresh = false
                    });
                return Results.Redirect("/Venta");
            }
            catch (Exception ex)
            {
                logs.CreateLogger("AccesoLocalVentasDesarrollo")
                    .LogError(ex, "Fallo al validar empleado en el entorno de desarrollo");
                return Results.Problem(
                    "No se pudo validar el empleado en MySQL. Revise la BD de desarrollo.",
                    statusCode: 503);
            }
        });
    }

    private static bool EsLocal(HttpContext contexto) =>
        contexto.Connection.RemoteIpAddress is { } ip && IPAddress.IsLoopback(ip);
}
