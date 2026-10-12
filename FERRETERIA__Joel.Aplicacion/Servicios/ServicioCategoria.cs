using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioCategoria
    {
        private readonly RepositoryCreator<IGestionarRepository<Categoria>> _creador;
        private IGestionarRepository<Categoria>? _repositorio;

        public ServicioCategoria(
            RepositoryCreator<IGestionarRepository<Categoria>> creador)
        {
            _creador = creador;
        }

        private IGestionarRepository<Categoria> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public List<Categoria> ObtenerTodos()
        {
            return Repositorio.ObtenerTodos();
        }

        public List<Categoria> ObtenerActivas()
        {
            return Repositorio.MostrarActivos();
        }

        public Categoria? ObtenerPorId(int id)
        {
            return Repositorio
                .ObtenerTodos()
                .FirstOrDefault(c => c.IdCategoria == id);
        }

        public string SiguienteCodigo()
        {
            return $"CAT-{Repositorio.ObtenerTodos().Count + 1:D3}";
        }

        public int Insertar(Categoria categoria)
        {
            return Repositorio.Insertar(categoria);
        }

        public void Actualizar(Categoria categoria)
        {
            Repositorio.Actualizar(categoria);
        }

        public void CambiarEstado(int id, int idEmpleadoResponsable)
        {
            Repositorio.CambiarEstado(id, idEmpleadoResponsable);
        }
    }
}
