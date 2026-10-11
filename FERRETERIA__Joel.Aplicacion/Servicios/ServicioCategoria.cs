using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioCategoria
    {
        private readonly IGestionarRepository<Categoria> _repositorio;

        public ServicioCategoria(
            IGestionarRepository<Categoria> repositorio)
        {
            _repositorio = repositorio;
        }

        public List<Categoria> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Categoria> ObtenerActivas()
        {
            return _repositorio.MostrarActivos();
        }

        public Categoria? ObtenerPorId(int id)
        {
            return _repositorio
                .ObtenerTodos()
                .FirstOrDefault(c => c.IdCategoria == id);
        }

        public string SiguienteCodigo()
        {
            return $"CAT-{_repositorio.ObtenerTodos().Count + 1:D3}";
        }

        public int Insertar(Categoria categoria)
        {
            return _repositorio.Insertar(categoria);
        }

        public void Actualizar(Categoria categoria)
        {
            _repositorio.Actualizar(categoria);
        }

        public void CambiarEstado(int id)
        {
            _repositorio.CambiarEstado(id);
        }
    }
}
