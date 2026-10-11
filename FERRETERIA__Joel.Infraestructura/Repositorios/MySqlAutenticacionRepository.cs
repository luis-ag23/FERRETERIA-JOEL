using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlAutenticacionRepository : IAutenticacionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlAutenticacionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Empleado? ObtenerPorCorreo(string correo)
        {
            const string query = @"
                SELECT
                    IdEmpleado,
                    Nombre,
                    Apellido,
                    Rol,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM empleado
                WHERE CorreoElectronico = @correo
                  AND Estado = 1
                LIMIT 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@correo", correo);

            connection.Open();

            using MySqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new Empleado
            {
                IdEmpleado = reader.GetInt32("IdEmpleado"),
                Nombre = reader["Nombre"].ToString() ?? "",
                Apellido = reader["Apellido"].ToString() ?? "",
                Rol = reader["Rol"].ToString() ?? "",
                Telefono = reader["Telefono"].ToString() ?? "",
                CorreoElectronico = reader["CorreoElectronico"] == DBNull.Value
                    ? null
                    : reader["CorreoElectronico"].ToString(),
                Contrasena = reader["Contrasena"].ToString() ?? "",
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

        public void ActualizarContrasena(int idEmpleado, string hash)
        {
            const string query = @"
                UPDATE empleado
                SET
                    Contrasena = @hash,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdEmpleado = @idEmpleado";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idEmpleado", idEmpleado);
            command.Parameters.AddWithValue("@hash", hash);

            connection.Open();

            command.ExecuteNonQuery();
        }
    }
}
