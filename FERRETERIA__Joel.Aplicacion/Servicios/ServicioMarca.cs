using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioMarca
    {
        private readonly RepositoryCreator<IGestionarRepository<Marca>> _creador;
        private IGestionarRepository<Marca>? _repositorio;

        public ServicioMarca(
            RepositoryCreator<IGestionarRepository<Marca>> creador)
        {
            _creador = creador;
        }

        private IGestionarRepository<Marca> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public List<Marca> ObtenerTodas()
        {
            return Repositorio.ObtenerTodos();
        }

        public List<Marca> ObtenerActivas()
        {
            return Repositorio.MostrarActivos();
        }

        public Marca? ObtenerPorId(int id)
        {
            return Repositorio
                .ObtenerTodos()
                .FirstOrDefault(m => m.IdMarca == id);
        }

        public int Insertar(Marca marca)
        {
            return Repositorio.Insertar(marca);
        }

        public void Actualizar(Marca marca)
        {
            Repositorio.Actualizar(marca);
        }

        public void CambiarEstado(int id, int idEmpleadoResponsable)
        {
            Repositorio.CambiarEstado(id, idEmpleadoResponsable);
        }
    }
}
