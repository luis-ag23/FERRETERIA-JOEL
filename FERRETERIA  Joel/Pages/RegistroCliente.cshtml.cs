using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    // Página de auto-registro público de clientes: valida, hashea la contraseña y persiste sin autenticar al usuario.
    public class RegistroClienteModel : PageModel
    {
        private readonly ServicioRegistroCliente _servicio;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RegistroClienteModel> _logger;

        [BindProperty]
        public RegistroClienteSolicitud Solicitud { get; set; } = new();

        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public RegistroClienteModel(
            ServicioRegistroCliente servicio,
            IConfiguration configuration,
            ILogger<RegistroClienteModel> logger)
        {
            _servicio = servicio;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        private string? PaginaLogin =>
            _configuration["Registro:PaginaLogin"];

        public IActionResult OnPost()
        {
            NormalizarDatos();

            try
            {
                ResultadoRegistro resultado = _servicio.Registrar(Solicitud);

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
                    "Cliente registrado con éxito. Ya puede iniciar sesión.";

                // TODO (login): mientras la página de inicio de sesión no
                // exista se muestra el mensaje; al crearla, la redirección a
                // la ruta configurada se activará automáticamente.
                if (!string.IsNullOrWhiteSpace(PaginaLogin)
                    && Url.Page(PaginaLogin) is not null)
                {
                    return Redirect(PaginaLogin);
                }

                return RedirectToPage("RegistroCliente");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                ErroresCampo[
                    nameof(RegistroClienteSolicitud.CorreoElectronico)] =
                    "El correo ya está registrado.";

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el cliente.");

                Errores.Add(
                    "No se pudo completar el registro. Inténtalo nuevamente.");

                return Page();
            }
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
