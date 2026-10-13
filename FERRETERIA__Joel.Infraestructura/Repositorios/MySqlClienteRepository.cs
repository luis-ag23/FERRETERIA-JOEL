using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Repositorio de clientes. Implementa el contrato genérico
    /// <see cref="IRepository{T}"/> y expone operaciones propias del flujo de
    /// venta (búsqueda y consulta por id) como métodos concretos que se
    /// inyectan directamente por DI. Ningún método dispone la conexión: su
    /// ciclo de vida lo administra <see cref="IConexionTransaccional"/>.
    /// </summary>
    public class MySqlClienteRepository : IRepository<Cliente>
    {
        private const string Columnas = @"
            IdCliente,
            Nombre,
            Apellido,
            Telefono,
            Estado,
            FechaRegistro,
            FechaActualizacion,
            IdEmpleadoResponsable";

        private readonly IConexionTransaccional _conexion;

        public MySqlClienteRepository(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        private async Task<MySqlCommand> CrearComandoAsync(string query)
        {
            MySqlCommand command = new(query, await _conexion.ObtenerConexionAsync());

            if (_conexion.Transaccion is not null)
            {
                command.Transaction = _conexion.Transaccion;
            }

            return command;
        }

        // ================== Contrato genérico ==================

        public virtual async Task<List<Cliente>> ObtenerTodosAsync()
        {
            List<Cliente> clientes = new();

            string query = $@"
                SELECT {Columnas}
                FROM cliente
                ORDER BY Nombre ASC";

            using MySqlCommand command = await CrearComandoAsync(query);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clientes.Add(MapearCliente(reader));
            }

            return clientes;
        }

        public virtual async Task<int> InsertarAsync(Cliente cliente)
        {
            const string query = @"
                INSERT INTO cliente
                (
                    Nombre,
                    Apellido,
                    Telefono,
                    Estado,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @nombre,
                    @apellido,
                    @telefono,
                    1,
                    @idEmpleadoResponsable
                )";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@apellido", cliente.Apellido);
            command.Parameters.AddWithValue("@telefono", cliente.Telefono);
            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                (object?)cliente.IdEmpleadoResponsable ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Operaciones propias de Ventas ==================

        public virtual async Task<List<Cliente>> BuscarAsync(string termino)
        {
            List<Cliente> clientes = new();

            string query = $@"
                SELECT {Columnas}
                FROM cliente
                WHERE Estado = 1
                  AND (
                        Nombre LIKE @termino
                     OR Apellido LIKE @termino
                     OR Telefono LIKE @termino
                     OR CONCAT(Nombre, ' ', Apellido) LIKE @termino
                  )
                ORDER BY Nombre ASC
                LIMIT 20";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@termino", $"%{termino}%");

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                clientes.Add(MapearCliente(reader));
            }

            return clientes;
        }

        public virtual async Task<Cliente?> ObtenerPorIdAsync(int idCliente)
        {
            string query = $@"
                SELECT {Columnas}
                FROM cliente
                WHERE IdCliente = @idCliente
                LIMIT 1";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idCliente", idCliente);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapearCliente(reader) : null;
        }

        private static Cliente MapearCliente(MySqlDataReader reader)
        {
            return new Cliente
            {
                IdCliente = reader.GetInt32("IdCliente"),
                Nombre = reader["Nombre"].ToString() ?? "",
                Apellido = reader["Apellido"].ToString() ?? "",
                Telefono = reader["Telefono"].ToString() ?? "",
                Estado = reader.GetByte("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion"),
                IdEmpleadoResponsable = reader["IdEmpleadoResponsable"] == DBNull.Value
                    ? null
                    : reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}
