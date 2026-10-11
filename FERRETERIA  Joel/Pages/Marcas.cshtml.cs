using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
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

        public void OnGet(bool? soloActivos)
        {
            SoloActivos = soloActivos ?? false;

            try
            {
                ListMarcas = SoloActivos
                    ? _servicio.ObtenerActivas()
                    : _servicio.ObtenerTodas();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de marcas.");
                Mensaje = "No se pudo cargar el listado de marcas.";
            }
        }

        public IActionResult OnPostEliminar(int id)
        {
            if (!User.IsInRole(RolesSistema.Administrador))
            {
                return Forbid();
            }

            try
            {
                _servicio.CambiarEstado(id);
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