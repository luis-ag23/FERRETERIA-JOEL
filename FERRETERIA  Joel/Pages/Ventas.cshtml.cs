using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Listado de ventas con modal de detalle (solo lectura) y
    /// anulación lógica con motivo. La anulación restaura el stock y
    /// registra el responsable. Consume únicamente la fachada de
    /// anulación (que a su vez delega el stock en su propia fachada).
    /// Todos los manejadores son asíncronos.
    /// </summary>
    [Authorize(Roles = RolesSistema.Personal)]
    public class VentasModel : PageModel
    {
        private readonly FachadaAnulacion _fachada;
        private readonly ILogger<VentasModel> _logger;

        public VentasModel(FachadaAnulacion fachada, ILogger<VentasModel> logger)
        {
            _fachada = fachada;
            _logger = logger;
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

        public async Task OnGetAsync()
        {
            try
            {
                Ventas = await _fachada.ListarVentasListadoAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo cargar el historial de ventas");
                TempData["MensajeError"] = "No se pudo cargar el historial. Revise la conexión a MySQL y la migración de Ventas.";
                Ventas = new List<VentaListado>();
            }
        }

        public async Task<IActionResult> OnGetDetalleAsync(int idVenta)
        {
            ResultadoVenta? resultado;
            try
            {
                resultado = await _fachada.ObtenerVentaAsync(idVenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo cargar el detalle de venta {IdVenta}", idVenta);
                return StatusCode(503, new { mensaje = "No se pudo consultar el detalle en MySQL." });
            }

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

        public async Task<IActionResult> OnPostAnularAsync()
        {
            if (!User.IsInRole(RolesSistema.EmpleadoVenta))
            {
                return Forbid();
            }

            ResultadoVenta resultado;
            try
            {
                resultado = await _fachada.AnularVentaAsync(
                    IdVenta,
                    MotivoAnulacion ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo anular la venta {IdVenta}", IdVenta);
                MensajeError = "No se pudo anular la venta. Revise la conexión a MySQL.";
                return RedirectToPage();
            }

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
