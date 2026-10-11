using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlEmpleadoRepository : IRepository<Empleado>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlEmpleadoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Empleado> ObtenerTodos()
        {
            List<Empleado> empleados = new();

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
                empleados.Add(MapearEmpleado(reader));
            }

            return empleados;
        }

        public int Insertar(Empleado empleado)
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

            using MySqlCommand command =
                new MySqlCommand(query, connection);

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

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Funciones fuera del contrato ==================

        public List<Empleado> ObtenerActivas()
        {
            List<Empleado> empleados = new();

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
                empleados.Add(MapearEmpleado(reader));
            }

            return empleados;
        }

        public Empleado? ObtenerPorId(int id)
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
                WHERE IdEmpleado = @id
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
                ? MapearEmpleado(reader)
                : null;
        }

        public void Actualizar(Empleado empleado)
        {
            const string query = @"
                UPDATE empleado
                SET
                    Nombre = @nombre,
                    Apellido = @apellido,
                    Rol = @rol,
                    Telefono = @telefono,
                    CorreoElectronico = @correoElectronico,
                    Contrasena = @contrasena,
                    Estado = @estado,
                    FechaActualizacion = CURRENT_TIMESTAMP,
                    IdEmpleadoResponsable = @idEmpleadoResponsable
                WHERE IdEmpleado = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", empleado.IdEmpleado);
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

            command.Parameters.AddWithValue("@estado", empleado.Estado);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                (object?)empleado.IdEmpleadoResponsable ?? DBNull.Value);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public void CambiarEstado(int id)
        {
            const string query = @"
                UPDATE empleado
                SET
                    Estado = CASE
                        WHEN Estado = 1 THEN 0
                        ELSE 1
                    END
                WHERE IdEmpleado = @id";

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
                FROM empleado";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private Empleado MapearEmpleado(MySqlDataReader reader)
        {
            return new Empleado
            {
                IdEmpleado =
                    reader.GetInt32("IdEmpleado"),
                Nombre =
                    reader["Nombre"].ToString() ?? "",
                Apellido =
                    reader["Apellido"].ToString() ?? "",
                Rol =
                    reader["Rol"].ToString() ?? "",
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
                IdEmpleadoResponsable =
                    reader["IdEmpleadoResponsable"] == DBNull.Value
                        ? null
                        : reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}
