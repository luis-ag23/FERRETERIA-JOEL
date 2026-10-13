using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = FERRETERIA__Joel.Dominio.Constantes.RolesSistema.Administrador)]
    public class CategoriasModel : PageModel
    {
        private readonly ServicioCategoria _servicio;
        private readonly ILogger<CategoriasModel> _logger;

        public string Mensaje { get; set; } = "";
        public List<Categoria> ListCategorias { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public CategoriasModel(
            ServicioCategoria servicio,
            ILogger<CategoriasModel> logger)
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
                ListCategorias = SoloActivos
                    ? await _servicio.ObtenerActivasAsync()
                    : await _servicio.ObtenerTodosAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de categorías.");
                Mensaje = "No se pudo cargar el listado de categorías.";
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                await _servicio.CambiarEstadoAsync(
                    id,
                    EmpleadoSesion.ObtenerIdEmpleado(User));
                TempData["Mensaje"] = "Categoría desactivada correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar la categoría {Id}.", id);
                TempData["MensajeError"] = "No se pudo desactivar la categoría. Inténtalo nuevamente.";
            }
            return RedirectToPage(new { soloActivos = SoloActivos });
        }
    }
}
