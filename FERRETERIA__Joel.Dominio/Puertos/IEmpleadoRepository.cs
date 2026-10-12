using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IEmpleadoRepository : IRepository<Empleado>
    {
        Empleado? ObtenerPorCorreo(string correo);

        void ActualizarContrasena(int id, string contrasena);
    }
}
