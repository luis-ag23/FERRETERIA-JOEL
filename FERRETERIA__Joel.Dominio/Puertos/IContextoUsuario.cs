namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Identidad del usuario autenticado. El dominio y la aplicación
    /// dependen de esta abstracción; la capa Web la resuelve desde
    /// las claims de la cookie. Evita acoplar Aplicacion a ASP.NET.
    /// </summary>
    public interface IContextoUsuario
    {
        int IdEmpleado { get; }

        string NombreCompleto { get; }

        string Rol { get; }
    }
}
