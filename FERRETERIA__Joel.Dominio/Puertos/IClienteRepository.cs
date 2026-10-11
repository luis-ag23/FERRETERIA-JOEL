using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> ObtenerTodosAsync();

        Task<int> InsertarAsync(Cliente cliente);

        Task<Cliente?> ObtenerPorIdAsync(int idCliente);

        /// <summary>
        /// Busca clientes activos por nombre, apellido o teléfono.
        /// </summary>
        Task<List<Cliente>> BuscarAsync(string termino);
    }
}
