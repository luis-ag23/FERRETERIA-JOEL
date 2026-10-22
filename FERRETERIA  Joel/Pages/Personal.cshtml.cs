using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Gestión del personal: listado de cuentas y baja lógica.
    ///
    /// <para>
    /// La baja es lógica: la cuenta no se borra, pasa a estado
    /// <c>Inactivo</c> y deja de poder iniciar sesión, pero el histórico de
    /// ventas y la auditoría siguen siendo trazables.
    /// </para>
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    public class PersonalModel : PageModel
    {
        private readonly ServicioPersonal _servicio;
        private readonly ILogger<PersonalModel> _logger;

        public PersonalModel(
            ServicioPersonal servicio,
            ILogger<PersonalModel> logger)
        {
            _servicio = servicio;
            _logger = logger;
        }

        /// <summary>
        /// Personal registrado. La consulta usa la proyección sin
        /// credenciales: ni la contraseña ni el token de activación llegan a
        /// la vista.
        /// </summary>
        public List<Empleado> Personal { get; set; } = new();

        public async Task OnGetAsync()
        {
            ViewData["ActiveMenu"] = "Registro";
            await CargarAsync();
        }

        /// <summary>
        /// Da de baja a un empleado: la cuenta queda inactiva y sin acceso.
        /// </summary>
        public async Task<IActionResult> OnPostBajaAsync(int idEmpleado)
        {
            try
            {
                ResultadoPersonal resultado =
                    await _servicio.DarDeBajaAsync(idEmpleado);

                if (resultado.Exitoso)
                {
                    TempData["Mensaje"] = resultado.Mensaje;
                }
                else
                {
                    TempData["MensajeError"] = resultado.Mensaje;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al dar de baja al empleado.");
                TempData["MensajeError"] =
                    "No se pudo completar la baja. Inténtalo nuevamente.";
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Reactiva una cuenta dada de baja. Queda pendiente de activación:
        /// el empleado debe definir de nuevo su contraseña.
        /// </summary>
        public async Task<IActionResult> OnPostReactivarAsync(int idEmpleado)
        {
            try
            {
                ResultadoPersonal resultado =
                    await _servicio.ReactivarAsync(idEmpleado);

                if (resultado.Exitoso)
                {
                    TempData["Mensaje"] = resultado.Mensaje;
                }
                else
                {
                    TempData["MensajeError"] = resultado.Mensaje;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar al empleado.");
                TempData["MensajeError"] =
                    "No se pudo completar la reactivación. Inténtalo nuevamente.";
            }

            return RedirectToPage();
        }

        private async Task CargarAsync()
        {
            try
            {
                Personal = await _servicio.ListarAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar el personal.");
                TempData["MensajeError"] =
                    "No se pudo cargar el listado del personal.";
            }
        }
    }
}
