using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioCategoria
    {
        private readonly IRepository<Categoria> _repositorio;
        private readonly IModificacionRepository<Categoria> _modificacion;

        public ServicioCategoria(
            IRepository<Categoria> repositorio,
            IModificacionRepository<Categoria> modificacion)
        {
            _repositorio = repositorio;
            _modificacion = modificacion;
        }

        public List<Categoria> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Categoria> ObtenerActivas()
        {
            return _repositorio.ObtenerActivas();
        }

        public Categoria? ObtenerPorId(int id)
        {
            return _repositorio.ObtenerPorId(id);
        }

        public string SiguienteCodigo()
        {
            return $"CAT-{_modificacion.Count() + 1:D3}";
        }

        public int Insertar(Categoria categoria)
        {
            return _repositorio.Insertar(categoria);
        }

        public void Actualizar(Categoria categoria)
        {
            _modificacion.Actualizar(categoria);
        }

        public void CambiarEstado(int id)
        {
            _modificacion.CambiarEstado(id);
        }
    }
}
