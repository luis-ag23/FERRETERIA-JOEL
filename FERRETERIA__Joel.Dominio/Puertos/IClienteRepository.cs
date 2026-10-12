using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Contrato único para la tabla <c>cliente</c>. Reúne el contrato
    /// sincrónico del módulo de autenticación (registro de cuentas e inicio
    /// de sesión) y el contrato asincrónico del módulo de Ventas (búsqueda y
    /// alta de clientes en el flujo de venta) sobre la misma entidad y la
    /// misma tabla, sin duplicar repositorios.
    /// </summary>
    public interface IClienteRepository : IRepository<Cliente>
    {
        // ---------- Contrato del módulo de autenticación ----------
        bool ExisteCorreo(string correo);

        Cliente? ObtenerPorCorreo(string correo);

        void ActualizarContrasena(int id, string contrasena);

        // ---------- Contrato del módulo de Ventas ----------
        Task<List<Cliente>> ObtenerTodosAsync();

        Task<int> InsertarAsync(Cliente cliente);

        Task<Cliente?> ObtenerPorIdAsync(int idCliente);

        /// <summary>
        /// Busca clientes activos por nombre, apellido o teléfono.
        /// </summary>
        Task<List<Cliente>> BuscarAsync(string termino);
    }
}