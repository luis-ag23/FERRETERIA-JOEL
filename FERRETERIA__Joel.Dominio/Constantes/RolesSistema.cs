namespace FERRETERIA__Joel.Dominio.Constantes
{
    /// <summary>
    /// Roles del sistema expuestos a la capa de presentación.
    /// Las cuentas con rol Administrador reciben el rol completo;
    /// el resto de los roles operativos se agrupan como Vendedor.
    /// </summary>
    public static class RolesSistema
    {
        public const string Administrador = "Administrador";

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
