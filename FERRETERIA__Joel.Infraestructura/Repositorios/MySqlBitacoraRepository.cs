using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Repositorio MySQL de la bitácora de sesiones. Inserta una fila por cada
    /// evento (login/logout) con el usuario, su tipo y la fecha/hora.
    /// </summary>
    public class MySqlBitacoraRepository : IBitacoraRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlBitacoraRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Registrar(
            string accion,
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario)
        {
            const string query = @"
                INSERT INTO bitacora_sesion
                (
                    Accion,
                    IdUsuario,
                    NombreUsuario,
                    TipoUsuario,
                    FechaHora
                )
                VALUES
                (
                    @accion,
                    @idUsuario,
                    @nombreUsuario,
                    @tipoUsuario,
                    CURRENT_TIMESTAMP
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@accion", accion);

            command.Parameters.AddWithValue(
                "@idUsuario",
                (object?)idUsuario ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@nombreUsuario",
                (object?)nombreUsuario ?? DBNull.Value);

            command.Parameters.AddWithValue("@tipoUsuario", tipoUsuario);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }
    }
}
