using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    // Consulta de categorías en modo solo lectura para el rol Usuario.
    [Authorize]
    public class CategoriasUsuarioModel : PageModel
    {
        private readonly ServicioCategoria _servicio;
        private readonly ILogger<CategoriasUsuarioModel> _logger;

        public string Mensaje { get; set; } = "";
        public List<Categoria> ListCategorias { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public CategoriasUsuarioModel(
            ServicioCategoria servicio,
            ILogger<CategoriasUsuarioModel> logger)
        {
            _servicio = servicio;
            _logger = logger;
        }

        public void OnGet(bool? soloActivos)
        {
            SoloActivos = soloActivos ?? false;

            try
            {
                ListCategorias = SoloActivos
                    ? _servicio.ObtenerActivas()
                    : _servicio.ObtenerTodos();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el listado de categorías.");
                Mensaje = "No se pudo cargar el listado de categorías.";
            }
        }
    }
}
