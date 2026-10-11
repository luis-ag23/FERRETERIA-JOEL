using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioProveedor
    {
        private readonly RepositoryCreator<IGestionarRepository<Proveedor>> _creador;
        private IGestionarRepository<Proveedor>? _repositorio;

        public ServicioProveedor(
            RepositoryCreator<IGestionarRepository<Proveedor>> creador)
        {
            _creador = creador;
        }

        private IGestionarRepository<Proveedor> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public List<Proveedor> ObtenerTodos()
        {
            return Repositorio.ObtenerTodos();
        }

        public List<Proveedor> ObtenerActivas()
        {
            return Repositorio.MostrarActivos();
        }

        public List<Proveedor> Listar(bool soloActivos)
        {
            return soloActivos
                ? ObtenerActivas()
                : ObtenerTodos();
        }

        public Proveedor? ObtenerPorId(int id)
        {
            return Repositorio
                .ObtenerTodos()
                .FirstOrDefault(p => p.IdProveedor == id);
        }

        public int Insertar(Proveedor proveedor)
        {
            return Repositorio.Insertar(proveedor);
        }

        public void Actualizar(Proveedor proveedor)
        {
            Repositorio.Actualizar(proveedor);
        }

        public void CambiarEstado(int id)
        {
            Repositorio.CambiarEstado(id);
        }
    }
}
