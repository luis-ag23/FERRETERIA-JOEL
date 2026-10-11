using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Puerto exclusivo de autenticación. Se mantiene separado de
    /// IRepository&lt;Empleado&gt; para no alterar el Factory Method
    /// ni el ServicioEmpleado existentes.
    /// </summary>
    public interface IAutenticacionRepository
    {
        Empleado? ObtenerPorCorreo(string correo);

        void ActualizarContrasena(int idEmpleado, string hash);
    }
}
