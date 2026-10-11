using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Listado de ventas con modal de detalle (solo lectura) y
    /// anulación lógica con motivo. La anulación restaura el stock y
    /// registra el responsable; solo la autoriza el rol Administrador.
    /// </summary>
    public class VentasModel : PageModel
    {
        private readonly FachadaVenta _fachada;

        public VentasModel(FachadaVenta fachada)
        {
            _fachada = fachada;
        }

        public List<VentaListado> Ventas { get; private set; } = new();

        [BindProperty]
        public int IdVenta { get; set; }

        [BindProperty]
        public string? MotivoAnulacion { get; set; }

        [TempData]
        public string? Mensaje { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public void OnGet()
        {
            Ventas = _fachada.ListarVentasListado();
        }

        public JsonResult OnGetDetalle(int idVenta)
        {
            ResultadoVenta? resultado = _fachada.ObtenerVenta(idVenta);

            if (resultado is null)
            {
                return new JsonResult(null);
            }

            return new JsonResult(new
            {
                codigoVenta = resultado.CodigoVenta,
                estadoVenta = resultado.EstadoVenta,
                metodoPago = resultado.MetodoPago,
                estadoPago = resultado.EstadoPago,
                fecha = resultado.FechaRegistro,
                subtotal = resultado.Subtotal,
                descuento = resultado.MontoDescuento,
                total = resultado.MontoTotal,
                motivoAnulacion = resultado.MotivoAnulacion,
                lineas = resultado.Detalles.Select(d => new
                {
                    nombre = d.Nombre,
                    codigo = d.Codigo,
                    unidad = d.UnidadMedida,
                    cantidad = d.Cantidad,
                    precio = d.PrecioUnitario,
                    total = d.TotalProducto
                })
            });
        }

        public IActionResult OnPostAnular()
        {
            if (!User.IsInRole(RolesSistema.Administrador))
            {
                return Forbid();
            }

            ResultadoVenta resultado =
                _fachada.AnularVenta(IdVenta, MotivoAnulacion ?? string.Empty);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
            }
            else
            {
                Mensaje = $@"Venta {resultado.CodigoVenta} anulada. " +
                          "El stock fue restaurado.";
            }

            return RedirectToPage();
        }
    }
}