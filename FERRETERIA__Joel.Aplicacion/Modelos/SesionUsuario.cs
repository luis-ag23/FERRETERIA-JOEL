namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Datos de la identidad autenticada. Solo los empleados inician sesión,
    /// con rol operativo Administrador o EmpleadoVenta.
    /// </summary>
    public class SesionUsuario
    {
        public int Id { get; init; }

        public string NombreCompleto { get; init; } = string.Empty;

        public string CorreoElectronico { get; init; } = string.Empty;

        /// <summary>Rol operativo: Administrador o EmpleadoVenta.</summary>
        public string Rol { get; init; } = string.Empty;
    }
}
