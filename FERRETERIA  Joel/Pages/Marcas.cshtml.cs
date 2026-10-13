using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = FERRETERIA__Joel.Dominio.Constantes.RolesSistema.Administrador)]
    public class MarcasModel : PageModel
    {
        private readonly ServicioMarca _servicio;
        private readonly ILogger<MarcasModel> _logger;

        public string Mensaje { get; set; } = "";
        public List<Marca> ListMarcas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public MarcasModel(
            ServicioMarca servicio,
            ILogger<MarcasModel> logger)
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
                ListMarcas = SoloActivos
                    ? await _servicio.ObtenerActivasAsync()
                    : await _servicio.ObtenerTodasAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de marcas.");
                Mensaje = "No se pudo cargar el listado de marcas.";
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                await _servicio.CambiarEstadoAsync(
                    id,
                    EmpleadoSesion.ObtenerIdEmpleado(User));
                TempData["Mensaje"] = "Marca desactivada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar la marca {Id}.", id);
                TempData["MensajeError"] = "No se pudo desactivar la marca. Inténtalo nuevamente.";
            }
            return RedirectToPage(new { soloActivos = SoloActivos });
        }
    }
}