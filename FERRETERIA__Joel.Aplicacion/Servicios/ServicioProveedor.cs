using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioProveedor
    {
        private readonly IGestionarRepository<Proveedor> _repositorio;

        public ServicioProveedor(
            IGestionarRepository<Proveedor> repositorio)
        {
            _repositorio = repositorio;
        }

        public List<Proveedor> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Proveedor> ObtenerActivas()
        {
            return _repositorio.MostrarActivos();
        }

        public List<Proveedor> Listar(bool soloActivos)
        {
            return soloActivos
                ? ObtenerActivas()
                : ObtenerTodos();
        }

        public Proveedor? ObtenerPorId(int id)
        {
            return _repositorio
                .ObtenerTodos()
                .FirstOrDefault(p => p.IdProveedor == id);
        }

        public int Insertar(Proveedor proveedor)
        {
            return _repositorio.Insertar(proveedor);
        }

        public void Actualizar(Proveedor proveedor)
        {
            _repositorio.Actualizar(proveedor);
        }

        public void CambiarEstado(int id)
        {
            _repositorio.CambiarEstado(id);
        }
    }
}
