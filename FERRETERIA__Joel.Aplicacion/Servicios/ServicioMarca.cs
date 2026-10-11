using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioMarca
    {
        private readonly IGestionarRepository<Marca> _repositorio;

        public ServicioMarca(
            IGestionarRepository<Marca> repositorio)
        {
            _repositorio = repositorio;
        }

        public List<Marca> ObtenerTodas()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Marca> ObtenerActivas()
        {
            return _repositorio.MostrarActivos();
        }

        public Marca? ObtenerPorId(int id)
        {
            return _repositorio
                .ObtenerTodos()
                .FirstOrDefault(m => m.IdMarca == id);
        }

        public int Insertar(Marca marca)
        {
            return _repositorio.Insertar(marca);
        }

        public void Actualizar(Marca marca)
        {
            _repositorio.Actualizar(marca);
        }

        public void CambiarEstado(int id)
        {
            _repositorio.CambiarEstado(id);
        }
    }
}