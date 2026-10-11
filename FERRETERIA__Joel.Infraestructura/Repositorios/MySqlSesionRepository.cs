using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlSesionRepository : ISesionRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlSesionRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Registrar(int idEmpleado, string accion)
        {
            const string query = @"
                INSERT INTO sesion_usuario
                (
                    IdEmpleado,
                    Accion,
                    FechaInicio
                )
                VALUES
                (
                    @idEmpleado,
                    @accion,
                    CURRENT_TIMESTAMP
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idEmpleado", idEmpleado);
            command.Parameters.AddWithValue("@accion", accion);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        public void Cerrar(int idSesion)
        {
            const string query = @"
                UPDATE sesion_usuario
                SET FechaFin = CURRENT_TIMESTAMP
                WHERE IdSesion = @idSesion";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idSesion", idSesion);

            connection.Open();

            command.ExecuteNonQuery();
        }
    }
}
