using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Cliente? ObtenerPorId(int idCliente);

        /// <summary>
        /// Busca clientes activos por nombre, apellido o teléfono.
        /// </summary>
        List<Cliente> Buscar(string termino);
    }
}
