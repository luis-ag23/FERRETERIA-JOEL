using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;

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

        public async Task<List<Marca>> ObtenerTodasAsync()
        {
            return await Repositorio.ObtenerTodosAsync();
        }

        public async Task<List<Marca>> ObtenerActivasAsync()
        {
            return await Repositorio.MostrarActivosAsync();
        }

        public async Task<Marca?> ObtenerPorIdAsync(int id)
        {
            List<Marca> marcas = await Repositorio.ObtenerTodosAsync();

            return marcas.FirstOrDefault(m => m.IdMarca == id);
        }

        public async Task<int> InsertarAsync(Marca marca)
        {
            return await Repositorio.InsertarAsync(marca);
        }

        public async Task ActualizarAsync(Marca marca)
        {
            await Repositorio.ActualizarAsync(marca);
        }

        public async Task CambiarEstadoAsync(
            int id,
            int idEmpleadoResponsable)
        {
            await Repositorio.CambiarEstadoAsync(id, idEmpleadoResponsable);
        }
    }
}
