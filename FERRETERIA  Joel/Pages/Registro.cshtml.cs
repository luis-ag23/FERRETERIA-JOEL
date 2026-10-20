using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Alta de personal exclusiva del administrador. Permite crear cuentas con
    /// rol <c>EmpleadoVenta</c> o <c>Administrador</c>. No se define contraseña:
    /// la cuenta nace pendiente de activación y se envía un enlace por correo
    /// para que el empleado la establezca. También permite reenviar el enlace
    /// cuando el anterior venció.
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    public class RegistroModel : PageModel
    {
        private readonly ServicioRegistroEmpleado _servicioEmpleado;
        private readonly ILogger<RegistroModel> _logger;

        [BindProperty]
        public RegistroSolicitud Solicitud { get; set; } = new();

        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        /// <summary>
        /// Empleados pendientes de activación (enlace enviado y no usado).
        /// </summary>
        public List<Empleado> Pendientes { get; set; } = new();

        /// <summary>
        /// Enlace de activación mostrado en pantalla cuando no hay SMTP
        /// configurado (desarrollo), para poder probar el flujo.
        /// </summary>
        public string? EnlaceActivacion { get; set; }

        /// <summary>
        /// Correo que ya está registrado cuando falla el alta. Permite ofrecer
        /// el reenvío del enlace de activación si esa cuenta está pendiente.
        /// </summary>
        public string? CorreoDuplicado { get; set; }

        /// <summary>
        /// Indica si el correo duplicado corresponde a una cuenta pendiente de
        /// activación (y por tanto se puede reenviar el enlace).
        /// </summary>
        public bool CorreoDuplicadoPendiente { get; set; }

        public RegistroModel(
            ServicioRegistroEmpleado servicioEmpleado,
            ILogger<RegistroModel> logger)
        {
            _servicioEmpleado = servicioEmpleado;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            ViewData["ActiveMenu"] = "Registro";
            await CargarPendientesAsync();
        }

        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            ViewData["ActiveMenu"] = "Registro";
            NormalizarDatos();

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

                    await MarcarCorreoDuplicadoAsync();

                    await CargarPendientesAsync();
                    return Page();
                }

                TempData["Mensaje"] =
                    "Cuenta creada con éxito. Se envió el enlace de activación al correo.";

                EnlaceActivacion = resultado.EnlaceActivacion;

                await CargarPendientesAsync();
                return Page();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                Errores.Add("El correo ya está registrado.");
                ErroresCampo[
                    nameof(RegistroSolicitud.CorreoElectronico)] =
                    "El correo ya está registrado.";

                await MarcarCorreoDuplicadoAsync();

                await CargarPendientesAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar la cuenta.");
                Errores.Add(
                    "No se pudo completar el registro. Inténtalo nuevamente.");
                await CargarPendientesAsync();
                return Page();
            }
        }

        /// <summary>
        /// Reenvía el enlace de activación a un empleado pendiente: genera un
        /// token nuevo y vuelve a enviar el correo.
        /// </summary>
        public async Task<IActionResult> OnPostReenviarAsync(int idEmpleado)
        {
            ViewData["ActiveMenu"] = "Registro";

            try
            {
                ResultadoRegistro resultado =
                    await _servicioEmpleado.ReenviarActivacionAsync(idEmpleado);

                if (resultado.Exitoso)
                {
                    TempData["Mensaje"] =
                        "Se reenvió el enlace de activación al correo del empleado.";

                    EnlaceActivacion = resultado.EnlaceActivacion;
                }
                else
                {
                    Errores = resultado.Errores;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar la activación.");
                Errores.Add(
                    "No se pudo reenviar el enlace. Inténtalo nuevamente.");
            }

            await CargarPendientesAsync();
            return Page();
        }

        /// <summary>
        /// Reenvía el enlace de activación a la cuenta pendiente identificada
        /// por el correo que el administrador intentó registrar (correo ya
        /// existente).
        /// </summary>
        public async Task<IActionResult> OnPostReenviarCorreoAsync(string correo)
        {
            ViewData["ActiveMenu"] = "Registro";

            try
            {
                ResultadoRegistro resultado =
                    await _servicioEmpleado.ReenviarPorCorreoAsync(correo);

                if (resultado.Exitoso)
                {
                    TempData["Mensaje"] =
                        "Se reenvió el enlace de activación al correo indicado.";
                    EnlaceActivacion = resultado.EnlaceActivacion;
                }
                else
                {
                    Errores = resultado.Errores;
                    CorreoDuplicado = correo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar la activación por correo.");
                Errores.Add(
                    "No se pudo reenviar el enlace. Inténtalo nuevamente.");
            }

            await CargarPendientesAsync();
            return Page();
        }

        /// <summary>
        /// Si el error del alta fue por correo duplicado, guarda ese correo y
        /// comprueba si pertenece a una cuenta pendiente (para ofrecer el
        /// reenvío del enlace).
        /// </summary>
        private async Task MarcarCorreoDuplicadoAsync()
        {
            if (!ErroresCampo.ContainsKey(
                    nameof(RegistroSolicitud.CorreoElectronico)))
            {
                return;
            }

            CorreoDuplicado = Solicitud.CorreoElectronico;

            try
            {
                CorreoDuplicadoPendiente =
                    await _servicioEmpleado.EsCuentaPendienteAsync(
                        CorreoDuplicado);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al comprobar si el correo duplicado está pendiente.");
            }
        }

        private async Task CargarPendientesAsync()
        {
            try
            {
                Pendientes = await _servicioEmpleado.ObtenerPendientesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar empleados pendientes.");
            }
        }

        private void NormalizarDatos()
        {
            Solicitud.Nombre = NormalizarTexto(Solicitud.Nombre);
            Solicitud.Apellido = NormalizarTexto(Solicitud.Apellido);
            Solicitud.Telefono = NormalizarTexto(Solicitud.Telefono);
            Solicitud.CorreoElectronico =
                NormalizarTexto(Solicitud.CorreoElectronico);
            Solicitud.Rol = RolesSistema.Normalizar(Solicitud.Rol);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }
    }
}
