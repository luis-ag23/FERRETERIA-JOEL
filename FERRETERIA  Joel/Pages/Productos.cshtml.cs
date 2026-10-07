using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    public class ProductosModel : PageModel
    {
        private readonly ServicioProducto _servicio;
        private readonly ServicioCategoria _servicioCategoria;
        private readonly ILogger<ProductosModel> _logger;

        public List<Producto> ListProductos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public ProductosModel(
            ServicioProducto servicio,
            ServicioCategoria servicioCategoria,
            ILogger<ProductosModel> logger)
        {
            _servicio = servicio;
            _servicioCategoria = servicioCategoria;
            _logger = logger;
        }

        public string ObtenerToken(int id)
        {
            return UrlProtector.Cifrar(id.ToString());
        }

        public void OnGet(bool? soloActivos)
        {
            SoloActivos = soloActivos ?? false;

            ListProductos = _servicio.Listar(SoloActivos);

            Categorias =
                _servicioCategoria.ObtenerTodos();
        }

        public IActionResult OnPostEliminar(int idProducto)
        {
            try
            {
                _servicio.CambiarEstado(idProducto);

                TempData["Mensaje"] =
                    "Producto eliminado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al eliminar el producto {IdProducto}.",
                    idProducto);

                TempData["MensajeError"] =
                    "No se pudo eliminar el producto. Inténtalo nuevamente.";
            }

            return RedirectToPage(new { soloActivos = SoloActivos });
        }
    }
}
