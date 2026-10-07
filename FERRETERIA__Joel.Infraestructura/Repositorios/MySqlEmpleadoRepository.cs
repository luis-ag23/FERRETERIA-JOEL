using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlEmpleadoRepository : IRepository<Empleado>, IModificacionRepository<Empleado>
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
                    Nombre
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

        public List<Empleado> ObtenerActivas()
        {
            List<Empleado> empleados = new();

            const string query = @"
                SELECT
                    IdEmpleado,
                    Nombre
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
                    Nombre
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

        public int Insertar(Empleado empleado)
        {
            const string query = @"
                INSERT INTO empleado
                (
                    Nombre
                )
                VALUES
                (
                    @nombre
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@nombre", empleado.Nombre);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        public void Actualizar(Empleado empleado)
        {
            const string query = @"
                UPDATE empleado
                SET
                    Nombre = @nombre
                WHERE IdEmpleado = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", empleado.IdEmpleado);
            command.Parameters.AddWithValue("@nombre", empleado.Nombre);

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
                IdEmpleado = reader.GetInt32("IdEmpleado"),
                Nombre = reader["Nombre"].ToString() ?? ""
            };
        }
    }
}