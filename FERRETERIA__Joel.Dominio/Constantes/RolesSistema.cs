namespace FERRETERIA__Joel.Dominio.Constantes
{
    /// <summary>
    /// Roles de sistema usados en la autenticación y en las políticas de
    /// autorización. El rol distingue a los clientes (acceso de solo lectura)
    /// de los empleados (acceso total a la gestión).
    /// </summary>
    public static class RolesSistema
    {
        public const string Usuario = "Usuario";

        public const string Empleado = "Empleado";
    }
}
