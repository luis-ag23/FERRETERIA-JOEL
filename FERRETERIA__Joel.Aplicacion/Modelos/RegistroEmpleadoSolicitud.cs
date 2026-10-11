namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Datos permitidos del formulario de auto-registro de un empleado.
    /// El estado, el rol y las fechas los asigna el servicio; el responsable
    /// queda nulo.
    /// </summary>
    public class RegistroEmpleadoSolicitud : IRegistroCredenciales
    {
        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty;

        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}
