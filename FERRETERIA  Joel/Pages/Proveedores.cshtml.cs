using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = FERRETERIA__Joel.Dominio.Constantes.RolesSistema.Administrador)]
    public class ProveedoresModel : PageModel
    {
        private readonly ServicioProveedor _servicio;
        private readonly ILogger<ProveedoresModel> _logger;

        public List<Proveedor> ListProveedores { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public ProveedoresModel(
            ServicioProveedor servicio,
            ILogger<ProveedoresModel> logger)
        {
            _servicio = servicio;
            _logger = logger;
        }

        public string ObtenerToken(int id)
        {
            return UrlProtector.Cifrar(id.ToString());
        }

        public async Task OnGetAsync(bool? soloActivos)
        {
            SoloActivos = soloActivos ?? false;

            try
            {
                ListProveedores = await _servicio.ListarAsync(SoloActivos);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cargar el listado de proveedores.");

                TempData["MensajeError"] =
                    "No se pudo cargar el listado de proveedores.";
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int idProveedor)
        {
            try
            {
                await _servicio.CambiarEstadoAsync(
                    idProveedor,
                    EmpleadoSesion.ObtenerIdEmpleado(User));

                TempData["Mensaje"] =
                    "Proveedor eliminado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al eliminar el proveedor {IdProveedor}.",
                    idProveedor);

                TempData["MensajeError"] =
                    "No se pudo eliminar el proveedor. Inténtalo nuevamente.";
            }

            return RedirectToPage(new { soloActivos = SoloActivos });
        }
    }
}
