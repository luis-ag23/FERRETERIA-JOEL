using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioEmpleado
    {
        private readonly IRepository<Empleado> _repositorio;

        public ServicioEmpleado(IRepository<Empleado> repositorio)
        {
            _repositorio = repositorio;
        }

        public List<Empleado> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }
    }
}
