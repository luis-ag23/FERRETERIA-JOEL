using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;

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

        public async Task<List<Categoria>> ObtenerTodosAsync()
        {
            return await Repositorio.ObtenerTodosAsync();
        }

        public async Task<List<Categoria>> ObtenerActivasAsync()
        {
            return await Repositorio.MostrarActivosAsync();
        }

        public async Task<Categoria?> ObtenerPorIdAsync(int id)
        {
            List<Categoria> categorias = await Repositorio.ObtenerTodosAsync();

            return categorias.FirstOrDefault(c => c.IdCategoria == id);
        }

        public async Task<string> SiguienteCodigoAsync()
        {
            List<Categoria> categorias = await Repositorio.ObtenerTodosAsync();

            return $"CAT-{categorias.Count + 1:D3}";
        }

        public async Task<int> InsertarAsync(Categoria categoria)
        {
            return await Repositorio.InsertarAsync(categoria);
        }

        public async Task ActualizarAsync(Categoria categoria)
        {
            await Repositorio.ActualizarAsync(categoria);
        }

        public async Task CambiarEstadoAsync(
            int id,
            int idEmpleadoResponsable)
        {
            await Repositorio.CambiarEstadoAsync(id, idEmpleadoResponsable);
        }
    }
}
