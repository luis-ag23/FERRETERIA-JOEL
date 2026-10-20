using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    [Authorize(Roles = $"{RolesSistema.EmpleadoVenta},{RolesSistema.Administrador}")]
    public class CatalogoProductosModel : PageModel
    {
        private readonly ServicioProducto _servicioProducto;
        private readonly ServicioCategoria _servicioCategoria;

        public CatalogoProductosModel(
            ServicioProducto servicioProducto,
            ServicioCategoria servicioCategoria)
        {
            _servicioProducto = servicioProducto;
            _servicioCategoria = servicioCategoria;
        }

        public List<Producto> ListProductos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();
        public bool SoloActivos { get; set; }

        public async Task OnGetAsync(bool soloActivos = false)
        {
            SoloActivos = soloActivos;

            Categorias = await _servicioCategoria.ObtenerTodosAsync();

            List<Producto> productos = await _servicioProducto.ObtenerTodosAsync();

            if (soloActivos)
            {
                productos = productos.Where(p => p.Estado == 1).ToList();
            }

            ListProductos = productos;
        }
    }
}