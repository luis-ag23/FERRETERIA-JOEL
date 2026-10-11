using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
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

        private MySqlCommand CrearComando(string query)
        {
            MySqlCommand command = new(query, _conexion.Conexion);

            if (_conexion.Transaccion is not null)
            {
                command.Transaction = _conexion.Transaccion;
            }

            return command;
        }

        public List<Cliente> ObtenerTodos()
        {
            List<Cliente> clientes = new();

            string query = $@"
                SELECT {Columnas}
                FROM cliente
                ORDER BY Nombre ASC";

            using MySqlCommand command = CrearComando(query);

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(MapearCliente(reader));
            }

            return clientes;
        }

        public List<Cliente> Buscar(string termino)
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

            using MySqlCommand command = CrearComando(query);

            command.Parameters.AddWithValue("@termino", $"%{termino}%");

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(MapearCliente(reader));
            }

            return clientes;
        }

        public Cliente? ObtenerPorId(int idCliente)
        {
            string query = $@"
                SELECT {Columnas}
                FROM cliente
                WHERE IdCliente = @idCliente
                LIMIT 1";

            using MySqlCommand command = CrearComando(query);

            command.Parameters.AddWithValue("@idCliente", idCliente);

            using MySqlDataReader reader = command.ExecuteReader();

            return reader.Read() ? MapearCliente(reader) : null;
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

            using MySqlCommand command = CrearComando(query);

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

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
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
