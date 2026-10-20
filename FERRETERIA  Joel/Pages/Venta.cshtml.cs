using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Pantalla de registro de venta. La presentación solo consume la
    /// fachada de venta: nunca accede a repositorios ni transacciones.
    /// El precio y los totales siempre se resuelven en el dominio.
    /// Todos los manejadores son asíncronos.
    /// </summary>
    [Authorize(Roles = RolesSistema.EmpleadoVenta)]
    public class VentaModel : PageModel
    {
        private readonly FachadaVenta _fachada;
        private readonly ILogger<VentaModel> _logger;

        public VentaModel(
            FachadaVenta fachada,
            ILogger<VentaModel> logger)
        {
            _fachada = fachada;
            _logger = logger;
        }

        public IReadOnlyList<string> MetodosPago { get; } = new[]
        {
            "Efectivo", "Tarjeta", "Transferencia", "QR"
        };

        [BindProperty]
        public int IdCliente { get; set; }

        [BindProperty]
        public string? NombreCliente { get; set; }

        [BindProperty]
        public string MetodoPago { get; set; } = "Efectivo";

        [BindProperty]
        public List<LineaVentaSolicitada> Lineas { get; set; } = new();

        public string? MensajeError { get; set; }

        public ResultadoVenta? VentaRegistrada { get; set; }

        public async Task<IActionResult> OnGetBuscarClientesAsync(string? termino)
        {
            try
            {
                var clientes = (await _fachada.BuscarClientesAsync(termino ?? string.Empty))
                    .Select(c => new
                    {
                        id = c.IdCliente,
                        ci = c.Ci,
                        nombre = $"{c.Nombre} {c.Apellido}".Trim(),
                        telefono = c.Telefono
                    });
                return new JsonResult(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar clientes desde Ventas");
                return StatusCode(503, new { mensaje = "No se pudieron buscar clientes. Revise la conexión con MySQL." });
            }
        }

        public async Task<IActionResult> OnGetBuscarProductosAsync(string? termino)
        {
            try
            {
                var productos = (await _fachada.BuscarProductosAsync(termino ?? string.Empty))
                    .Select(p => new
                    {
                        id = p.IdProducto,
                        codigo = p.Codigo,
                        nombre = p.Nombre,
                        unidad = p.UnidadMedida,
                        precio = p.PrecioVenta,
                        stock = p.CantidadStock
                    });
                return new JsonResult(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos desde Ventas");
                return StatusCode(503, new { mensaje = "No se pudieron buscar productos. Revise la conexión con MySQL." });
            }
        }

        /// <summary>
        /// Registra un cliente desde el modal "Nuevo cliente" de la
        /// propia pantalla de venta. Al ser una llamada AJAX la página
        /// nunca se recarga y las líneas seleccionadas se conservan.
        /// </summary>
        public async Task<IActionResult> OnPostCrearClienteAsync(
            string? nuevoClienteCi,
            string? nuevoClienteNombre,
            string? nuevoClienteApellido,
            string? nuevoClienteTelefono)
        {
            try
            {
                ResultadoCliente resultado = await _fachada.CrearClienteAsync(
                    nuevoClienteCi ?? string.Empty,
                    nuevoClienteNombre ?? string.Empty,
                    nuevoClienteApellido ?? string.Empty,
                    nuevoClienteTelefono ?? string.Empty);

                if (!resultado.Exito || resultado.Cliente is null)
                    return BadRequest(new
                    {
                        exito = false,
                        mensaje = resultado.Mensaje,
                        errores = resultado.Errores
                    });

                return new JsonResult(new
                {
                    exito = true,
                    id = resultado.Cliente.IdCliente,
                    ci = resultado.Cliente.Ci,
                    nombre = $"{resultado.Cliente.Nombre} {resultado.Cliente.Apellido}".Trim(),
                    telefono = resultado.Cliente.Telefono
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un cliente desde el modal de Ventas");
                return StatusCode(503, new
                {
                    exito = false,
                    mensaje = "No se pudo guardar el cliente en MySQL. Verifique la conexión y la migración de Ventas."
                });
            }
        }

        public async Task<IActionResult> OnPostRegistrarAsync()
        {
            // El estado del pago no se toma del cliente: lo fija el
            // servidor. Por ahora toda venta se registra como pagada.
            SolicitudVenta solicitud = new()
            {
                IdCliente = IdCliente,
                MetodoPago = MetodoPago,
                EstadoPago = EstadoPago.Pagado,
                Lineas = Lineas
            };

            ResultadoVenta resultado;
            try
            {
                resultado = await _fachada.RegistrarVentaAsync(solicitud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no controlado al registrar la venta");
                MensajeError = "No se pudo registrar la venta. Revise la conexión a MySQL o consulte al administrador.";
                return Page();
            }

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
                return Page();
            }

            VentaRegistrada = resultado;
            Lineas = new List<LineaVentaSolicitada>();
            IdCliente = 0;
            NombreCliente = null;
            return Page();
        }

    }
}
