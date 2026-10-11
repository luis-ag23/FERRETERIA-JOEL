using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Politicas;
using FERRETERIA__Joel.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Pantalla de registro de venta. La presentación solo consume la
    /// fachada de ventas: nunca accede a repositorios ni transacciones.
    /// El precio y los totales siempre se resuelven en el dominio.
    /// </summary>
    public class VentaModel : PageModel
    {
        private readonly FachadaVenta _fachada;

        public VentaModel(FachadaVenta fachada, ConfiguracionDescuentos descuentos)
        {
            _fachada = fachada;
            Descuentos = descuentos;
        }

        public ConfiguracionDescuentos Descuentos { get; }

        public IReadOnlyList<string> MetodosPago { get; } = new[]
        {
            "Efectivo", "Tarjeta", "Transferencia", "QR"
        };

        public IReadOnlyList<string> EstadosPago { get; } = new[]
        {
            "Pagado", "Pendiente", "Parcial"
        };

        [BindProperty]
        public int IdCliente { get; set; }

        [BindProperty]
        public string? NombreCliente { get; set; }

        [BindProperty]
        public string MetodoPago { get; set; } = "Efectivo";

        [BindProperty]
        public string EstadoPago { get; set; } = "Pagado";

        [BindProperty]
        public string? ClaveIdempotencia { get; set; }

        [BindProperty]
        public List<LineaVentaSolicitada> Lineas { get; set; } = new();

        public string? MensajeError { get; set; }

        public ResultadoVenta? VentaRegistrada { get; set; }

        public void OnGet()
        {
            ClaveIdempotencia = NuevaClave();
        }

        public JsonResult OnGetBuscarClientes(string? termino)
        {
            var clientes = _fachada.BuscarClientes(termino ?? string.Empty)
                .Select(c => new
                {
                    id = c.IdCliente,
                    nombre = $"{c.Nombre} {c.Apellido}".Trim(),
                    telefono = c.Telefono
                });

            return new JsonResult(clientes);
        }

        public JsonResult OnGetBuscarProductos(string? termino)
        {
            var productos = _fachada.BuscarProductos(termino ?? string.Empty)
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

        public IActionResult OnPostRegistrar()
        {
            if (IdCliente <= 0 && !string.IsNullOrWhiteSpace(NombreCliente))
            {
                IdCliente = _fachada
                    .BuscarClientes(NombreCliente!)
                    .FirstOrDefault()?.IdCliente ?? 0;
            }

            SolicitudVenta solicitud = new()
            {
                IdCliente = IdCliente,
                MetodoPago = MetodoPago,
                EstadoPago = EstadoPago,
                ClaveIdempotencia = ClaveIdempotencia,
                Lineas = Lineas
            };

            ResultadoVenta resultado = _fachada.RegistrarVenta(solicitud);

            if (!resultado.Exito)
            {
                MensajeError = resultado.Mensaje;
                ClaveIdempotencia = NuevaClave();
                return Page();
            }

            VentaRegistrada = resultado;
            Lineas = new List<LineaVentaSolicitada>();
            IdCliente = 0;
            NombreCliente = null;
            ClaveIdempotencia = NuevaClave();
            return Page();
        }

        private static string NuevaClave() => Guid.NewGuid().ToString("N");
    }
}
