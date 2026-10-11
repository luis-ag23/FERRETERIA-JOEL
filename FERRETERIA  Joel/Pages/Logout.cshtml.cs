using System.Security.Claims;
using FERRETERIA__Joel.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    // Cierra la sesión del usuario eliminando la cookie de autenticación y
    // registra el evento en la bitácora de sesiones.
    public class LogoutModel : PageModel
    {
        private readonly ServicioBitacora _bitacora;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(
            ServicioBitacora bitacora,
            ILogger<LogoutModel> logger)
        {
            _bitacora = bitacora;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            RegistrarBitacoraLogout();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["Mensaje"] = "Sesión cerrada correctamente.";

            return RedirectToPage("/Index");
        }

        private void RegistrarBitacoraLogout()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return;
            }

            try
            {
                int? id = int.TryParse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out int valor)
                    ? valor
                    : null;

                _bitacora.RegistrarLogout(
                    id,
                    User.Identity.Name,
                    User.FindFirstValue(ClaimTypes.Role) ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo registrar el cierre de sesión en la bitácora.");
            }
        }
    }
}
