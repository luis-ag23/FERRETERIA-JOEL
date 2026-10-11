using System.Security.Claims;
using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    // Inicio de sesión de empleados: valida credenciales vía el servicio y,
    // si son correctas, emite la cookie de autenticación del panel interno.
    public class LoginModel : PageModel
    {
        private readonly ServicioInicioSesionEmpleado _servicio;

        [BindProperty]
        public InicioSesionSolicitud Solicitud { get; set; } = new();

        public string? Mensaje { get; set; }

        public LoginModel(ServicioInicioSesionEmpleado servicio)
        {
            _servicio = servicio;
        }

        public IActionResult OnGet()
        {
            ViewData["ActiveMenu"] = "Login";

            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ViewData["ActiveMenu"] = "Login";

            ResultadoInicioSesion resultado = _servicio.Iniciar(Solicitud);

            if (!resultado.Exitoso || resultado.Empleado is null)
            {
                Mensaje = resultado.Mensaje;
                return Page();
            }

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    resultado.Empleado.IdEmpleado.ToString()),
                new(
                    ClaimTypes.Name,
                    $"{resultado.Empleado.Nombre} {resultado.Empleado.Apellido}".Trim()),
                new(
                    ClaimTypes.Email,
                    resultado.Empleado.CorreoElectronico ?? string.Empty),
                new(
                    ClaimTypes.Role,
                    string.IsNullOrWhiteSpace(resultado.Empleado.Rol)
                        ? "Empleado"
                        : resultado.Empleado.Rol)
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            TempData["Mensaje"] =
                $"Bienvenido, {resultado.Empleado.Nombre}.";

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToPage("/Index");
        }
    }
}
