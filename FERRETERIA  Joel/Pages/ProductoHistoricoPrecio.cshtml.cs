using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    public class ProductoHistoricoPrecioModel : PageModel
    {
        private readonly ServicioProducto _servicio;
        private readonly ServicioHistoricoPrecio _servicioHistoricoPrecio;
        private readonly ILogger<ProductoHistoricoPrecioModel> _logger;

        public List<HistoricoPrecio> Historicos { get; set; } = new();

        public string NombreProducto { get; set; } = string.Empty;

        public ProductoHistoricoPrecioModel(
            ServicioProducto servicio,
            ServicioHistoricoPrecio servicioHistoricoPrecio,
            ILogger<ProductoHistoricoPrecioModel> logger)
        {
            _servicio = servicio;
            _servicioHistoricoPrecio = servicioHistoricoPrecio;
            _logger = logger;
        }

        public IActionResult OnGet(string token)
        {
            string? texto = UrlProtector.Descifrar(token);

            if (!int.TryParse(texto, out int id))
            {
                TempData["MensajeError"] =
                    "El producto solicitado no existe.";

                return RedirectToPage("Productos");
            }

            var producto = _servicio.ObtenerPorId(id);

            if (producto is null)
            {
                TempData["MensajeError"] =
                    "El producto solicitado no existe.";

                return RedirectToPage("Productos");
            }

            NombreProducto = producto.Nombre;

            try
            {
                Historicos =
                    _servicioHistoricoPrecio.ObtenerPorProducto(producto.IdProducto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al obtener el histórico de precios del producto {IdProducto}.",
                    producto.IdProducto);
            }

            return Page();
        }
    }
}
