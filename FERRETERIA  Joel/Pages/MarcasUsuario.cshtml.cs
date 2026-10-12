using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    // Consulta de marcas en modo solo lectura para el rol Usuario.
    [Authorize]
    public class MarcasUsuarioModel : PageModel
    {
        private readonly ServicioMarca _servicio;
        private readonly ILogger<MarcasUsuarioModel> _logger;

        public string Mensaje { get; set; } = "";
        public List<Marca> ListMarcas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public MarcasUsuarioModel(
            ServicioMarca servicio,
            ILogger<MarcasUsuarioModel> logger)
        {
            _servicio = servicio;
            _logger = logger;
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
    }
}
