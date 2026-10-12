using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioEmpleado
    {
        private readonly RepositoryCreator<IEmpleadoRepository> _creador;
        private IEmpleadoRepository? _repositorio;

        public ServicioEmpleado(
            RepositoryCreator<IEmpleadoRepository> creador)
        {
            _creador = creador;
        }

        private IEmpleadoRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public List<Empleado> ObtenerTodos()
        {
            return Repositorio.ObtenerTodos();
        }
    }
}
