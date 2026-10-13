namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Datos de entrada para registrar una venta. Al enviar una
    /// ClaveIdempotencia no vacía, la operación se vuelve idempotente:
    /// un reenvío no genera una segunda venta.
    /// </summary>
    public class SolicitudVenta
    {
        public int IdCliente { get; set; }

        public string MetodoPago { get; set; } = string.Empty;

        public string EstadoPago { get; set; } = string.Empty;

        public string? ClaveIdempotencia { get; set; }

        public List<LineaVentaSolicitada> Lineas { get; set; } = new();
    }
}
