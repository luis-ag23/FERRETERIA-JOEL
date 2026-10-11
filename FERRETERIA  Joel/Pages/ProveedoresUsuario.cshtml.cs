using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    // Consulta de proveedores en modo solo lectura para el rol Usuario.
    [Authorize]
    public class ProveedoresUsuarioModel : PageModel
    {
        private readonly ServicioProveedor _servicio;
        private readonly ILogger<ProveedoresUsuarioModel> _logger;

        public List<Proveedor> ListProveedores { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public ProveedoresUsuarioModel(
            ServicioProveedor servicio,
            ILogger<ProveedoresUsuarioModel> logger)
        {
            _servicio = servicio;
            _logger = logger;
        }

        public void OnGet(bool? soloActivos)
        {
            SoloActivos = soloActivos ?? false;

            try
            {
                ListProveedores = _servicio.Listar(SoloActivos);
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
    }
}
