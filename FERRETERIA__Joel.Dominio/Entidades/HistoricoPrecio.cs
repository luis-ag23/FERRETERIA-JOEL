namespace FERRETERIA__Joel.Dominio.Entidades
{
    public class HistoricoPrecio
    {
        public int IdHistoricoPrecio { get; set; }

        public int IdProducto { get; set; }

        public decimal Precio { get; set; }

        public DateTime FechaInicioVigencia { get; set; }

        public DateTime? FechaFinVigencia { get; set; }

        public string MotivoCambio { get; set; } = string.Empty;

        public byte Estado { get; set; }

        public DateTime FechaRegistro { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public int IdEmpleadoResponsable { get; set; }
        public string? NombreProducto { get; set; }

        public string? NombreEmpleadoResponsable { get; set; }
    }
}