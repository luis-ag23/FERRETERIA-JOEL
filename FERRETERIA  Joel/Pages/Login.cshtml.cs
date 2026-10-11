using System.Security.Claims;
using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    // Inicio de sesión unificado: valida las credenciales (empleados o
    // clientes) vía el servicio, emite la cookie de autenticación con el rol
    // correspondiente y registra el evento en la bitácora de sesiones.
    public class LoginModel : PageModel
    {
        private readonly ServicioInicioSesion _servicio;
        private readonly ServicioBitacora _bitacora;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public InicioSesionSolicitud Solicitud { get; set; } = new();

        public string? Mensaje { get; set; }

        public LoginModel(
            ServicioInicioSesion servicio,
            ServicioBitacora bitacora,
            ILogger<LoginModel> logger)
        {
            _servicio = servicio;
            _bitacora = bitacora;
            _logger = logger;
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

            if (!resultado.Exitoso || resultado.Usuario is null)
            {
                Mensaje = resultado.Mensaje;
                return Page();
            }

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    resultado.Usuario.Id.ToString()),
                new(
                    ClaimTypes.Name,
                    resultado.Usuario.NombreCompleto),
                new(
                    ClaimTypes.Email,
                    resultado.Usuario.CorreoElectronico),
                new(
                    ClaimTypes.Role,
                    resultado.Usuario.Rol)
            };

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));

            RegistrarBitacoraLogin(resultado.Usuario);

            TempData["Mensaje"] =
                $"Bienvenido, {resultado.Usuario.NombreCompleto}.";

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToPage("/Index");
        }

        private void RegistrarBitacoraLogin(SesionUsuario usuario)
        {
            try
            {
                _bitacora.RegistrarLogin(
                    usuario.Id,
                    usuario.NombreCompleto,
                    usuario.Rol);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo registrar el inicio de sesión en la bitácora.");
            }
        }
    }
}
