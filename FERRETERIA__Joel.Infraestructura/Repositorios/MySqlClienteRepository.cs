using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlClienteRepository : IRepository<Cliente>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlClienteRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Cliente> ObtenerTodos()
        {
            List<Cliente> clientes = new();

            const string query = @"
                SELECT
                    IdCliente,
                    Nombre,
                    Apellido,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdClienteResponsable
                FROM cliente
                ORDER BY Nombre ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

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
                    IdClienteResponsable
                )
                VALUES
                (
                    @nombre,
                    @apellido,
                    @telefono,
                    @correoElectronico,
                    @contrasena,
                    1,
                    @idClienteResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@nombre", cliente.Nombre);

            command.Parameters.AddWithValue("@apellido", cliente.Apellido);

            command.Parameters.AddWithValue("@telefono", cliente.Telefono);

            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)cliente.CorreoElectronico ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@contrasena",
                cliente.Contrasena);

            command.Parameters.AddWithValue(
                "@idClienteResponsable",
                (object?)cliente.IdClienteResponsable ?? DBNull.Value);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Funciones fuera del contrato ==================

        public List<Cliente> ObtenerActivas()
        {
            List<Cliente> clientes = new();

            const string query = @"
                SELECT
                    IdCliente,
                    Nombre,
                    Apellido,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdClienteResponsable
                FROM cliente
                WHERE Estado = 1
                ORDER BY Nombre ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(MapearCliente(reader));
            }

            return clientes;
        }

        public Cliente? ObtenerPorId(int id)
        {
            const string query = @"
                SELECT
                    IdCliente,
                    Nombre,
                    Apellido,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdClienteResponsable
                FROM cliente
                WHERE IdCliente = @id
                LIMIT 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            return reader.Read()
                ? MapearCliente(reader)
                : null;
        }

        public void Actualizar(Cliente cliente)
        {
            const string query = @"
                UPDATE cliente
                SET
                    Nombre = @nombre,
                    Apellido = @apellido,
                    Telefono = @telefono,
                    CorreoElectronico = @correoElectronico,
                    Contrasena = @contrasena,
                    Estado = @estado,
                    FechaActualizacion = CURRENT_TIMESTAMP,
                    IdClienteResponsable = @idClienteResponsable
                WHERE IdCliente = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", cliente.IdCliente);

            command.Parameters.AddWithValue("@nombre", cliente.Nombre);

            command.Parameters.AddWithValue("@apellido", cliente.Apellido);

            command.Parameters.AddWithValue("@telefono", cliente.Telefono);

            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)cliente.CorreoElectronico ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@contrasena",
                cliente.Contrasena);

            command.Parameters.AddWithValue("@estado", cliente.Estado);

            command.Parameters.AddWithValue(
                "@idClienteResponsable",
                (object?)cliente.IdClienteResponsable ?? DBNull.Value);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public void CambiarEstado(int id)
        {
            const string query = @"
                UPDATE cliente
                SET
                    Estado = CASE
                        WHEN Estado = 1 THEN 0
                        ELSE 1
                    END,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdCliente = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public int Count()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM cliente";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private Cliente MapearCliente(MySqlDataReader reader)
        {
            return new Cliente
            {
                IdCliente =
                    reader.GetInt32("IdCliente"),

                Nombre =
                    reader["Nombre"].ToString() ?? "",

                Apellido =
                    reader["Apellido"].ToString() ?? "",

                Telefono =
                    reader["Telefono"].ToString() ?? "",

                CorreoElectronico =
                    reader["CorreoElectronico"] == DBNull.Value
                        ? null
                        : reader["CorreoElectronico"].ToString(),

                Contrasena =
                    reader["Contrasena"].ToString() ?? "",

                Estado =
                    reader.GetByte("Estado"),

                FechaRegistro =
                    reader.GetDateTime("FechaRegistro"),

                FechaActualizacion =
                    reader["FechaActualizacion"] == DBNull.Value
                        ? null
                        : reader.GetDateTime("FechaActualizacion"),

                IdClienteResponsable =
                    reader["IdClienteResponsable"] == DBNull.Value
                        ? null
                        : reader.GetInt32("IdClienteResponsable")
            };
        }
    }
}
