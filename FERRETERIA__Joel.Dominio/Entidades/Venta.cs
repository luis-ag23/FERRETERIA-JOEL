namespace FERRETERIA__Joel.Dominio.Entidades
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public string CodigoVenta { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public int IdEmpleadoResponsable { get; set; }
    }
}
