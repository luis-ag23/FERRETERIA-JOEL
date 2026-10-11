using MySql.Data.MySqlClient;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlMarcaRepository : IGestionarRepository<Marca>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlMarcaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Marca> ObtenerTodos()
        {
            List<Marca> marcas = new();

            const string query = @"
                SELECT
                    IdMarca,
                    Nombre,
                    Descripcion,
                    PaisOrigen,
                    Calidad,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
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

        public int Insertar(Marca marca)
        {
            const string query = @"
                INSERT INTO marca
                (
                    Nombre,
                    Descripcion,
                    PaisOrigen,
                    Calidad,
                    Estado,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @nombre,
                    @descripcion,
                    @paisOrigen,
                    @calidad,
                    1,
                    @idEmpleadoResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@nombre",
                marca.Nombre.Trim());

            command.Parameters.AddWithValue(
                "@descripcion",
                (object?)marca.Descripcion ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@paisOrigen",
                marca.PaisOrigen);

            command.Parameters.AddWithValue(
                "@calidad",
                marca.Calidad);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                marca.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        public void Actualizar(Marca marca)
        {
            const string query = @"
                UPDATE marca
                SET
                    Nombre = @nombre,
                    Descripcion = @descripcion,
                    PaisOrigen = @paisOrigen,
                    Calidad = @calidad,
                    Estado = @estado,
                    FechaActualizacion = CURRENT_TIMESTAMP,
                    IdEmpleadoResponsable = @idEmpleadoResponsable
                WHERE IdMarca = @idMarca";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idMarca",
                marca.IdMarca);

            command.Parameters.AddWithValue(
                "@nombre",
                marca.Nombre.Trim());

            command.Parameters.AddWithValue(
                "@descripcion",
                (object?)marca.Descripcion ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@paisOrigen",
                marca.PaisOrigen);

            command.Parameters.AddWithValue(
                "@calidad",
                marca.Calidad);

            command.Parameters.AddWithValue(
                "@estado",
                marca.Estado);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                marca.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public void CambiarEstado(int id)
        {
            const string query = @"
                UPDATE marca
                SET
                    Estado = CASE
                        WHEN Estado = 1 THEN 0
                        ELSE 1
                    END,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdMarca = @idMarca";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idMarca", id);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public List<Marca> MostrarActivos()
        {
            List<Marca> marcas = new();

            const string query = @"
                SELECT
                    IdMarca,
                    Nombre,
                    Descripcion,
                    PaisOrigen,
                    Calidad,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
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

        // ================== Funciones fuera del contrato ==================

        private static Marca MapearMarca(MySqlDataReader reader)
        {
            return new Marca
            {
                IdMarca =
                    reader.GetInt32("IdMarca"),

                Nombre =
                    reader["Nombre"].ToString() ?? "",

                Descripcion =
                    reader["Descripcion"] == DBNull.Value
                    ? null
                    : reader["Descripcion"].ToString(),

                PaisOrigen =
                    reader["PaisOrigen"].ToString() ?? "",

                Calidad =
                    reader["Calidad"].ToString() ?? "",

                Estado =
                    reader.GetByte("Estado"),

                FechaRegistro =
                    reader.GetDateTime("FechaRegistro"),

                FechaActualizacion =
                    reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion"),

                IdEmpleadoResponsable =
                    reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}
