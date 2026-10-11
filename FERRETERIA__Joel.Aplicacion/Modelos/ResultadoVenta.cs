using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Resultado de una operación de venta (registro, consulta o
    /// anulación). Nunca lanza excepciones de negocio: informa el
    /// desenlace mediante <see cref="Exito"/> y <see cref="Mensaje"/>.
    /// </summary>
    public class ResultadoVenta
    {
        public bool Exito { get; private init; }

        /// <summary>
        /// Verdadero cuando el resultado proviene de un reenvío con la
        /// misma clave de idempotencia (no se creó una nueva venta).
        /// </summary>
        public bool Idempotente { get; private init; }

        public string Mensaje { get; private init; } = string.Empty;

        public int IdVenta { get; private init; }

        public string? CodigoVenta { get; private init; }

        public int IdCliente { get; private init; }

        public string MetodoPago { get; private init; } = string.Empty;

        public string EstadoPago { get; private init; } = string.Empty;

        public string EstadoVenta { get; private init; } = string.Empty;

        public decimal Subtotal { get; private init; }

        public decimal MontoDescuento { get; private init; }

        public decimal PorcentajeDescuento { get; private init; }

        public decimal MontoTotal { get; private init; }

        public DateTime FechaRegistro { get; private init; }

        public List<LineaVentaDetalle> Detalles { get; private init; } = new();

        public static ResultadoVenta Correcto(
            Venta venta,
            IEnumerable<LineaVentaDetalle> detalles,
            bool idempotente = false)
        {
            return Construir(venta, detalles, idempotente);
        }

        public static ResultadoVenta Fallido(string mensaje)
        {
            return new ResultadoVenta
            {
                Exito = false,
                Mensaje = mensaje
            };
        }

        private static ResultadoVenta Construir(
            Venta venta,
            IEnumerable<LineaVentaDetalle> detalles,
            bool idempotente)
        {
            return new ResultadoVenta
            {
                Exito = true,
                Idempotente = idempotente,
                Mensaje = idempotente
                    ? "La venta ya había sido registrada con esta clave."
                    : "Venta registrada correctamente.",
                IdVenta = venta.IdVenta,
                CodigoVenta = venta.CodigoVenta,
                IdCliente = venta.IdCliente,
                MetodoPago = venta.MetodoPago,
                EstadoPago = venta.EstadoPago,
                EstadoVenta = venta.EstadoVenta,
                Subtotal = venta.Subtotal,
                MontoDescuento = venta.MontoDescuento,
                PorcentajeDescuento = venta.PorcentajeDescuento,
                MontoTotal = venta.MontoTotal,
                FechaRegistro = venta.FechaRegistro,
                Detalles = detalles.ToList()
            };
        }
    }
}
