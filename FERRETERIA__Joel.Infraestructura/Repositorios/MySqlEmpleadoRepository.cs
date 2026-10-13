using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Repositorio de empleados. Implementa el contrato genérico
    /// <see cref="IRepository{T}"/> y expone operaciones propias de la
    /// autenticación (búsqueda por correo y actualización de contraseña) que
    /// se inyectan directamente por DI.
    /// </summary>
    public class MySqlEmpleadoRepository : IRepository<Empleado>
    {
        private const string Columnas = @"
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
            IdEmpleadoResponsable";

        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlEmpleadoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // ================== Contrato genérico ==================

        public virtual async Task<List<Empleado>> ObtenerTodosAsync()
        {
            List<Empleado> empleados = new();

            string query = $@"
                SELECT {Columnas}
                FROM empleado
                ORDER BY Nombre ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                empleados.Add(MapearEmpleado(reader));
            }

            return empleados;
        }

        public virtual async Task<int> InsertarAsync(Empleado empleado)
        {
            const string query = @"
                INSERT INTO empleado
                (
                    Nombre,
                    Apellido,
                    Rol,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    Estado,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @nombre,
                    @apellido,
                    @rol,
                    @telefono,
                    @correoElectronico,
                    @contrasena,
                    1,
                    @idEmpleadoResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@nombre", empleado.Nombre);
            command.Parameters.AddWithValue("@apellido", empleado.Apellido);
            command.Parameters.AddWithValue("@rol", empleado.Rol);
            command.Parameters.AddWithValue("@telefono", empleado.Telefono);
            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)empleado.CorreoElectronico ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@contrasena",
                empleado.Contrasena);
            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                (object?)empleado.IdEmpleadoResponsable ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Autenticación ==================

        public virtual async Task<Empleado?> ObtenerPorCorreoAsync(string correo)
        {
            string query = $@"
                SELECT {Columnas}
                FROM empleado
                WHERE CorreoElectronico = @correo
                LIMIT 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@correo", correo);

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            return await reader.ReadAsync()
                ? MapearEmpleado(reader)
                : null;
        }

        public virtual async Task ActualizarContrasenaAsync(
            int id,
            string contrasena)
        {
            const string query = @"
                UPDATE empleado
                SET
                    Contrasena = @contrasena,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdEmpleado = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@contrasena", contrasena);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        private static Empleado MapearEmpleado(MySqlDataReader reader)
        {
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
    }
}
