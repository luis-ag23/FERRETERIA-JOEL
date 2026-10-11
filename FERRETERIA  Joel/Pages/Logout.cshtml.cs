using System.Security.Claims;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        private readonly ServicioSesion _servicioSesion;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(
            ServicioSesion servicioSesion,
            ILogger<LogoutModel> logger)
        {
            _servicioSesion = servicioSesion;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            CerrarSesion();

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToPage("/Login");
        }

        private void CerrarSesion()
        {
            string? valor = User.FindFirstValue(
                ContextoUsuarioHttp.ClaimIdSesion);

            if (!int.TryParse(valor, out int idSesion) || idSesion <= 0)
            {
                return;
            }

            try
            {
                _servicioSesion.Cerrar(idSesion);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo registrar el cierre de sesión.");
            }
        }
    }
}
