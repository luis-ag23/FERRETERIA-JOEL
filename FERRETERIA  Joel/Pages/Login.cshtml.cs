using System.Security.Claims;
using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ServicioAutenticacion _servicioAutenticacion;
        private readonly ServicioSesion _servicioSesion;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public string Correo { get; set; } = string.Empty;

        [BindProperty]
        public string Contrasena { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }

        public string? MensajeError { get; set; }

        public LoginModel(
            ServicioAutenticacion servicioAutenticacion,
            ServicioSesion servicioSesion,
            ILogger<LoginModel> logger)
        {
            _servicioAutenticacion = servicioAutenticacion;
            _servicioSesion = servicioSesion;
            _logger = logger;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(Correo) ||
                string.IsNullOrWhiteSpace(Contrasena))
            {
                MensajeError = "Debe ingresar el correo y la contraseña.";

                return Page();
            }

            ResultadoAutenticacion resultado =
                _servicioAutenticacion.Autenticar(Correo, Contrasena);

            if (!resultado.Exito || resultado.Empleado is null)
            {
                MensajeError = resultado.Mensaje;

                return Page();
            }

            Empleado empleado = resultado.Empleado;

            int idSesion = RegistrarSesion(empleado.IdEmpleado);

            await IniciarSesionAsync(empleado, idSesion);

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToPage("/Index");
        }

        private int RegistrarSesion(int idEmpleado)
        {
            try
            {
                return _servicioSesion.Registrar(
                    idEmpleado,
                    "Inicio de sesión");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo registrar la bitácora de sesión.");

                return 0;
            }
        }

        private async Task IniciarSesionAsync(Empleado empleado, int idSesion)
        {
            string rol = RolesSistema.Normalizar(empleado.Rol);

            string nombreCompleto =
                $"{empleado.Nombre} {empleado.Apellido}".Trim();

            List<Claim> claims = new()
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    empleado.IdEmpleado.ToString()),
                new Claim(ClaimTypes.Name, nombreCompleto),
                new Claim(ClaimTypes.Role, rol),
                new Claim(
                    ClaimTypes.Email,
                    empleado.CorreoElectronico ?? string.Empty),
                new Claim(
                    ContextoUsuarioHttp.ClaimNombreCompleto,
                    nombreCompleto),
                new Claim(
                    ContextoUsuarioHttp.ClaimIdSesion,
                    idSesion.ToString())
            };

            ClaimsIdentity identidad = new(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));
        }
    }
}
