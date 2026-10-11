namespace FERRETERIA__Joel.Dominio.Entidades
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string? CorreoElectronico { get; set; }
        public string Contrasena { get; set; } = string.Empty;
        public byte Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public int? IdEmpleadoResponsable { get; set; }
    }
}
