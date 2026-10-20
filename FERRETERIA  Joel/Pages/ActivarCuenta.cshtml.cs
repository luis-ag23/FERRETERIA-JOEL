using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Pantalla pública de activación de cuenta. El empleado llega aquí desde
    /// el enlace enviado a su correo, define su contraseña y activa la cuenta.
    /// El token viaja cifrado en el query string y solo es válido 10 minutos.
    /// </summary>
    [AllowAnonymous]
    public class ActivarCuentaModel : PageModel
    {
        private readonly ServicioActivacionCuenta _servicio;

        [BindProperty]
        public ActivacionSolicitud Solicitud { get; set; } = new();

        public string? Mensaje { get; set; }

        public string? NombreEmpleado { get; set; }

        public bool TokenValido { get; set; }

        public bool Activada { get; set; }

        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public ActivarCuentaModel(ServicioActivacionCuenta servicio)
        {
            _servicio = servicio;
        }

        public async Task<IActionResult> OnGetAsync(string? token)
        {
            ViewData["ActiveMenu"] = "";

            string? tokenDescifrado = Descifrar(token);

            ResultadoActivacion resultado =
                await _servicio.ValidarTokenAsync(tokenDescifrado);

            if (!resultado.Exitoso)
            {
                Mensaje = resultado.Mensaje;
                TokenValido = false;
                return Page();
            }

            TokenValido = true;
            NombreEmpleado = resultado.NombreEmpleado;
            Solicitud.Token = tokenDescifrado!;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActiveMenu"] = "";

            string? tokenDescifrado = Descifrar(Solicitud.Token);

            if (string.IsNullOrWhiteSpace(tokenDescifrado))
            {
                Mensaje = "El enlace de activación no es válido.";
                TokenValido = false;
                return Page();
            }

            Solicitud.Token = tokenDescifrado;

            ResultadoActivacion resultado =
                await _servicio.ActivarAsync(Solicitud);

            if (!resultado.Exitoso)
            {
                Mensaje = resultado.Mensaje;
                TokenValido = false;
                return Page();
            }

            if (resultado.ErroresCampo.Any())
            {
                TokenValido = true;
                NombreEmpleado = resultado.NombreEmpleado;
                ErroresCampo = resultado.ErroresCampo;
                Solicitud.Contrasena = string.Empty;
                Solicitud.ConfirmarContrasena = string.Empty;
                return Page();
            }

            TokenValido = false;
            Activada = true;
            NombreEmpleado = resultado.NombreEmpleado;

            return Page();
        }

        private static string? Descifrar(string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            // El token en el enlace está cifrado; si llega en claro (p. ej. en
            // pruebas) se admite tal cual.
            return UrlProtector.Descifrar(token) ?? token;
        }
    }
}
