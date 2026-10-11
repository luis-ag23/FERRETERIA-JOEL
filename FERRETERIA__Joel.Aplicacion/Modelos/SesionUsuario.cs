namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Datos de la identidad autenticada, independientemente de si proviene de
    /// la tabla de clientes (rol Usuario) o de empleados (rol Empleado).
    /// </summary>
    public class SesionUsuario
    {
        public int Id { get; init; }

        public string NombreCompleto { get; init; } = string.Empty;

        public string CorreoElectronico { get; init; } = string.Empty;

        /// <summary>Tipo de cuenta: "Usuario" o "Empleado".</summary>
        public string Rol { get; init; } = string.Empty;
    }
}
