using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    // Consulta de productos en modo solo lectura para el rol Usuario.
    [Authorize]
    public class ProductosUsuarioModel : PageModel
    {
        private readonly ServicioProducto _servicio;
        private readonly ServicioCategoria _servicioCategoria;

        public List<Producto> ListProductos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloActivos { get; set; }

        public ProductosUsuarioModel(
            ServicioProducto servicio,
            ServicioCategoria servicioCategoria)
        {
            _servicio = servicio;
            _servicioCategoria = servicioCategoria;
        }

        public void OnGet(bool? soloActivos)
        {
            SoloActivos = soloActivos ?? false;

            ListProductos = _servicio.Listar(SoloActivos);

            Categorias = _servicioCategoria.ObtenerTodos();
        }
    }
}
