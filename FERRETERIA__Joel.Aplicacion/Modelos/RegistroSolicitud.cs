namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Datos del formulario de auto-registro unificado. El usuario elige si la
    /// cuenta es de tipo "Usuario" (cliente, solo lectura) o "Empleado"
    /// (gestión completa). Para crear un empleado se exige una clave secreta.
    /// </summary>
    public class RegistroSolicitud : IRegistroCredenciales
    {
        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string CorreoElectronico { get; set; } = string.Empty;

        public string Contrasena { get; set; } = string.Empty;

        public string ConfirmarContrasena { get; set; } = string.Empty;

        /// <summary>Tipo de cuenta: "Usuario" o "Empleado".</summary>
        public string TipoCuenta { get; set; } = TiposCuenta.Usuario;

        /// <summary>Clave requerida para crear una cuenta de empleado.</summary>
        public string ClaveEmpleado { get; set; } = string.Empty;
    }

    /// <summary>Valores válidos del selector de tipo de cuenta.</summary>
    public static class TiposCuenta
    {
        public const string Usuario = "Usuario";

        public const string Empleado = "Empleado";
    }
}
