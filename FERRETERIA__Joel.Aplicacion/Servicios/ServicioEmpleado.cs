using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioEmpleado
    {
        private readonly RepositoryCreator<IRepository<Empleado>> _creador;
        private IRepository<Empleado>? _repositorio;

        public ServicioEmpleado(
            RepositoryCreator<IRepository<Empleado>> creador)
        {
            _creador = creador;
        }

        private IRepository<Empleado> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public async Task<List<Empleado>> ObtenerTodosAsync()
        {
            return await Repositorio.ObtenerTodosAsync();
        }
    }
}
