using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Repositorio de clientes compartido por el módulo de autenticación y
    /// por Ventas. Implementa el contrato sincrónico (registro de cuentas e
    /// inicio de sesión) y, sobre la misma conexión transaccional, el
    /// contrato asincrónico que Ventas necesita. Ningún método dispone la
    /// conexión: su ciclo de vida lo administra <see cref="IConexionTransaccional"/>.
    /// </summary>
    public class MySqlClienteRepository : IClienteRepository
    {
        private const string Columnas = @"
            IdCliente,
            Nombre,
            Apellido,
            Telefono,
            CorreoElectronico,
            Contrasena,
            Estado,
            FechaRegistro,
            FechaActualizacion,
            IdClienteResponsable,
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

        // Sincrónico (solo para los contratos del módulo de autenticación):
        // obtiene la conexión ya abierta por IConexionTransaccional. No se
        // usa desde el flujo asíncrono de Ventas.
        private MySqlCommand CrearComandoSincrono(string query)
        {
            MySqlConnection conexion =
                _conexion.ObtenerConexionAsync().GetAwaiter().GetResult();

            MySqlCommand command = new(query, conexion);

            if (_conexion.Transaccion is not null)
            {
                command.Transaction = _conexion.Transaccion;
            }

            return command;
        }

        // ================== Contrato asincrónico (Ventas) ==================

        public async Task<List<Cliente>> ObtenerTodosAsync()
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

        public async Task<List<Cliente>> BuscarAsync(string termino)
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

        public async Task<Cliente?> ObtenerPorIdAsync(int idCliente)
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

        public async Task<int> InsertarAsync(Cliente cliente)
        {
            const string query = @"
                INSERT INTO cliente
                (
                    Nombre,
                    Apellido,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    Estado,
                    IdClienteResponsable,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @nombre,
                    @apellido,
                    @telefono,
                    @correoElectronico,
                    @contrasena,
                    1,
                    @idClienteResponsable,
                    @idEmpleadoResponsable
                )";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@apellido", cliente.Apellido);
            command.Parameters.AddWithValue("@telefono", cliente.Telefono);
            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)cliente.CorreoElectronico ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@contrasena",
                (object?)cliente.Contrasena ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@idClienteResponsable",
                (object?)cliente.IdClienteResponsable ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                (object?)cliente.IdEmpleadoResponsable ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Contrato sincrónico (autenticación) ==================

        public List<Cliente> ObtenerTodos()
        {
            List<Cliente> clientes = new();

            string query = $@"
                SELECT {Columnas}
                FROM cliente
                ORDER BY Nombre ASC";

            using MySqlCommand command = CrearComandoSincrono(query);

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(MapearCliente(reader));
            }

            return clientes;
        }

        public int Insertar(Cliente cliente)
        {
            const string query = @"
                INSERT INTO cliente
                (
                    Nombre,
                    Apellido,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    Estado,
                    FechaRegistro,
                    IdClienteResponsable
                )
                VALUES
                (
                    @nombre,
                    @apellido,
                    @telefono,
                    @correoElectronico,
                    @contrasena,
                    @estado,
                    @fechaRegistro,
                    @idClienteResponsable
                )";

            using MySqlCommand command = CrearComandoSincrono(query);

            command.Parameters.AddWithValue("@nombre", cliente.Nombre);
            command.Parameters.AddWithValue("@apellido", cliente.Apellido);
            command.Parameters.AddWithValue("@telefono", cliente.Telefono);
            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)cliente.CorreoElectronico ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@contrasena",
                cliente.Contrasena ?? string.Empty);
            command.Parameters.AddWithValue("@estado", cliente.Estado);
            command.Parameters.AddWithValue(
                "@fechaRegistro",
                cliente.FechaRegistro);
            command.Parameters.AddWithValue(
                "@idClienteResponsable",
                (object?)cliente.IdClienteResponsable ?? DBNull.Value);

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        public bool ExisteCorreo(string correo)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM cliente
                WHERE CorreoElectronico = @correo";

            using MySqlCommand command = CrearComandoSincrono(query);

            command.Parameters.AddWithValue("@correo", correo);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public Cliente? ObtenerPorCorreo(string correo)
        {
            string query = $@"
                SELECT {Columnas}
                FROM cliente
                WHERE CorreoElectronico = @correo
                LIMIT 1";

            using MySqlCommand command = CrearComandoSincrono(query);

            command.Parameters.AddWithValue("@correo", correo);

            using MySqlDataReader reader = command.ExecuteReader();

            return reader.Read()
                ? MapearCliente(reader)
                : null;
        }

        public void ActualizarContrasena(int id, string contrasena)
        {
            const string query = @"
                UPDATE cliente
                SET
                    Contrasena = @contrasena,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdCliente = @id";

            using MySqlCommand command = CrearComandoSincrono(query);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@contrasena", contrasena);

            command.ExecuteNonQuery();
        }

        private static Cliente MapearCliente(MySqlDataReader reader)
        {
            return new Cliente
            {
                IdCliente = reader.GetInt32("IdCliente"),
                Nombre = reader["Nombre"].ToString() ?? "",
                Apellido = reader["Apellido"].ToString() ?? "",
                Telefono = reader["Telefono"].ToString() ?? "",
                CorreoElectronico = reader["CorreoElectronico"] == DBNull.Value
                    ? null
                    : reader["CorreoElectronico"].ToString(),
                Contrasena = reader["Contrasena"] == DBNull.Value
                    ? null
                    : reader["Contrasena"].ToString(),
                Estado = reader.GetByte("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion"),
                IdClienteResponsable = reader["IdClienteResponsable"] == DBNull.Value
                    ? null
                    : reader.GetInt32("IdClienteResponsable"),
                IdEmpleadoResponsable = reader["IdEmpleadoResponsable"] == DBNull.Value
                    ? null
                    : reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}