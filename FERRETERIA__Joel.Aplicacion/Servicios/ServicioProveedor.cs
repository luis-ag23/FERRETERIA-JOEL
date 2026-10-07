using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioProveedor
    {
        private readonly IRepository<Proveedor> _repositorio;
        private readonly IModificacionRepository<Proveedor> _modificacion;

        public ServicioProveedor(
            IRepository<Proveedor> repositorio,
            IModificacionRepository<Proveedor> modificacion)
        {
            _repositorio = repositorio;
            _modificacion = modificacion;
        }

        public List<Proveedor> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Proveedor> ObtenerActivas()
        {
            return _repositorio.ObtenerActivas();
        }

        public List<Proveedor> Listar(bool soloActivos)
        {
            return soloActivos
                ? ObtenerActivas()
                : ObtenerTodos();
        }

        public Proveedor? ObtenerPorId(int id)
        {
            return _repositorio.ObtenerPorId(id);
        }

        public int Insertar(Proveedor proveedor)
        {
            return _repositorio.Insertar(proveedor);
        }

        public void Actualizar(Proveedor proveedor)
        {
            _modificacion.Actualizar(proveedor);
        }

        public void CambiarEstado(int id)
        {
            _modificacion.CambiarEstado(id);
        }
    }
}
