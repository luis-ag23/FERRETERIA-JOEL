using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;

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

        public async Task<List<Proveedor>> ObtenerTodosAsync()
        {
            return await Repositorio.ObtenerTodosAsync();
        }

        public async Task<List<Proveedor>> ObtenerActivasAsync()
        {
            return await Repositorio.MostrarActivosAsync();
        }

        public async Task<List<Proveedor>> ListarAsync(bool soloActivos)
        {
            return soloActivos
                ? await ObtenerActivasAsync()
                : await ObtenerTodosAsync();
        }

        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            List<Proveedor> proveedores = await Repositorio.ObtenerTodosAsync();

            return proveedores.FirstOrDefault(p => p.IdProveedor == id);
        }

        public async Task<int> InsertarAsync(Proveedor proveedor)
        {
            return await Repositorio.InsertarAsync(proveedor);
        }

        public async Task ActualizarAsync(Proveedor proveedor)
        {
            await Repositorio.ActualizarAsync(proveedor);
        }

        public async Task CambiarEstadoAsync(
            int id,
            int idEmpleadoResponsable)
        {
            await Repositorio.CambiarEstadoAsync(id, idEmpleadoResponsable);
        }
    }
}
