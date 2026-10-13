using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Auto-registro exclusivo para empleados. Exige la clave secreta
    /// configurada en <c>.env</c> (REGISTRO_CLAVE_EMPLEADO) y crea la cuenta
    /// con rol <c>EmpleadoVenta</c>. La contraseña se hashea con Argon2.
    /// </summary>
    public class RegistroModel : PageModel
    {
        private const string MensajeClaveInvalida =
            "La clave de empleado no es válida.";

        private readonly ServicioRegistroEmpleado _servicioEmpleado;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistroModel> _logger;

        [BindProperty]
        public RegistroSolicitud Solicitud { get; set; } = new();

        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public RegistroModel(
            ServicioRegistroEmpleado servicioEmpleado,
            IConfiguration configuration,
            ILogger<RegistroModel> logger)
        {
            _servicioEmpleado = servicioEmpleado;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
            ViewData["ActiveMenu"] = "Registro";
        }

        private string? PaginaLogin =>
            _configuration["Registro:PaginaLogin"] ?? "/Login";

        private string? ClaveEmpleadoConfigurada =>
            _configuration["Registro:ClaveEmpleado"];

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActiveMenu"] = "Registro";
            NormalizarDatos();

            if (!ClaveEmpleadoValida())
            {
                ErroresCampo["ClaveEmpleado"] = MensajeClaveInvalida;
                Errores.Add(
                    "Revise los campos señalados e inténtelo nuevamente.");
                return Page();
            }

            try
            {
                ResultadoRegistro resultado =
                    await _servicioEmpleado.RegistrarAsync(Solicitud);

                if (!resultado.Exitoso)
                {
                    Errores = resultado.Errores;
                    ErroresCampo = resultado.ErroresCampo;

                    if (!Errores.Any() && ErroresCampo.Any())
                    {
                        Errores.Add(
                            "Revise los campos señalados e inténtelo nuevamente.");
                    }

                    return Page();
                }

                TempData["Mensaje"] =
                    "Empleado registrado con éxito. Ya puede iniciar sesión.";

                return RedirectToPage("Login");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ErroresCampo[
                    nameof(RegistroSolicitud.CorreoElectronico)] =
                    "El correo ya está registrado.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la cuenta.");
                Errores.Add(
                    "No se pudo completar el registro. Inténtalo nuevamente.");
                return Page();
            }
        }

        private bool ClaveEmpleadoValida()
        {
            string claveConfigurada = ClaveEmpleadoConfigurada ?? string.Empty;

            if (string.IsNullOrWhiteSpace(claveConfigurada))
            {
                return false;
            }

            return string.Equals(
                Solicitud.ClaveEmpleado ?? string.Empty,
                claveConfigurada,
                StringComparison.Ordinal);
        }

        private void NormalizarDatos()
        {
            Solicitud.Nombre = NormalizarTexto(Solicitud.Nombre);
            Solicitud.Apellido = NormalizarTexto(Solicitud.Apellido);
            Solicitud.Telefono = NormalizarTexto(Solicitud.Telefono);
            Solicitud.CorreoElectronico =
                NormalizarTexto(Solicitud.CorreoElectronico);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }
    }
}
