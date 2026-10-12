namespace FERRETERIA__Joel.Dominio.Constantes
{
    /// <summary>
    /// Roles del sistema usados en la autenticación y en las políticas de
    /// autorización. El rol distingue a los clientes (acceso de solo lectura)
    /// de los empleados (acceso total a la gestión).
    /// </summary>
    public static class RolesSistema
    {
        public const string Usuario = "Usuario";

        public const string Empleado = "Empleado";

        // Ventas conserva la anulación restringida a este rol. El módulo de
        // autenticación del equipo todavía no emite 'Administrador': la
        // operación permanece protegida a la espera de que el responsable de
        // seguridad defina qué empleados pueden anular ventas.
        public const string Administrador = "Administrador";

        // Rol operativo legado usado por las pruebas del módulo de Ventas.
        public const string Vendedor = "Vendedor";

        public static string Normalizar(string rol)
        {
            return string.Equals(
                rol?.Trim(),
                Administrador,
                StringComparison.OrdinalIgnoreCase)
                ? Administrador
                : Vendedor;
        }
    }
}