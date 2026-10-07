using MySql.Data.MySqlClient;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlMarcaRepository : IMarcaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlMarcaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Marca> ObtenerTodas()
        {
            List<Marca> marcas = new();

            const string query = @"
                SELECT
                    IdMarca,
                    Nombre,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion
                FROM marca
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
                marcas.Add(MapearMarca(reader));
            }

            return marcas;
        }

        public List<Marca> ObtenerActivas()
        {
            List<Marca> marcas = new();

            const string query = @"
                SELECT
                    IdMarca,
                    Nombre,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion
                FROM marca
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
                marcas.Add(MapearMarca(reader));
            }

            return marcas;
        }

        private static Marca MapearMarca(MySqlDataReader reader)
        {
            return new Marca
            {
                IdMarca =
                    reader.GetInt32("IdMarca"),

                Nombre =
                    reader["Nombre"].ToString() ?? "",

                Estado =
                    reader.GetByte("Estado"),

                FechaRegistro =
                    reader.GetDateTime("FechaRegistro"),

                FechaActualizacion =
                    reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion")
            };
        }
    }
}
