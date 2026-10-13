namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Datos del formulario de alta de empleado. El alta crea siempre una
    /// cuenta operativa con rol <c>EmpleadoVenta</c> y exige la clave secreta
    /// configurada en el <c>.env</c> para autorizar el registro.
    /// </summary>
    public class RegistroSolicitud : IRegistroCredenciales
    {
        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty;

        public string ConfirmarContrasena { get; set; } = string.Empty;

        /// <summary>Clave secreta requerida para crear la cuenta de empleado.</summary>
        public string ClaveEmpleado { get; set; } = string.Empty;
    }
}
