using MySql.Data.MySqlClient;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlCategoriaRepository : IRepository<Categoria>, IModificacionRepository<Categoria>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlCategoriaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Categoria> ObtenerTodos()
        {
            List<Categoria> categorias = new();

            const string query = @"
                SELECT
                    IdCategoria,
                    Codigo,
                    Nombre,
                    Descripcion,
                    PorcentajeGanancia,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM categoria
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
                categorias.Add(MapearCategoria(reader));
            }

            return categorias;
        }

        public List<Categoria> ObtenerActivas()
        {
            List<Categoria> categorias = new();

            const string query = @"
                SELECT
                    IdCategoria,
                    Codigo,
                    Nombre,
                    Descripcion,
                    PorcentajeGanancia,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM categoria
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
                categorias.Add(MapearCategoria(reader));
            }

            return categorias;
        }

        public Categoria? ObtenerPorId(int id)
        {
            const string query = @"
                SELECT
                    IdCategoria,
                    Codigo,
                    Nombre,
                    Descripcion,
                    PorcentajeGanancia,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM categoria
                WHERE IdCategoria = @IdCategoria
                LIMIT 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@IdCategoria",
                id);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            return reader.Read()
                ? MapearCategoria(reader)
                : null;
        }

        public int Insertar(Categoria categoria)
        {
            const string query = @"INSERT INTO categoria 
                                  (Codigo, Nombre, Descripcion, PorcentajeGanancia, Estado, IdEmpleadoResponsable) 
                                  VALUES 
                                  (@Codigo, @Nombre, @Descripcion, @PorcentajeGanancia, @Estado, @IdEmpleadoResponsable);";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@Codigo", categoria.Codigo.Trim().ToUpper());
            command.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());
            command.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(categoria.Descripcion) ? (object)DBNull.Value : categoria.Descripcion.Trim());
            command.Parameters.AddWithValue("@PorcentajeGanancia", categoria.PorcentajeGanancia);
            command.Parameters.AddWithValue("@Estado", categoria.Estado);
            command.Parameters.AddWithValue("@IdEmpleadoResponsable", categoria.IdEmpleadoResponsable);

            connection.Open();
            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        public void Actualizar(Categoria categoria)
        {
            const string query = @"UPDATE categoria 
                                  SET Codigo = @Codigo, 
                                      Nombre = @Nombre, 
                                      Descripcion = @Descripcion, 
                                      PorcentajeGanancia = @PorcentajeGanancia,
                                      Estado = @Estado, 
                                      FechaActualizacion = CURRENT_TIMESTAMP 
                                  WHERE IdCategoria = @IdCategoria;";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@Codigo", categoria.Codigo.Trim().ToUpper());
            command.Parameters.AddWithValue("@Nombre", categoria.Nombre.Trim());
            command.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(categoria.Descripcion) ? (object)DBNull.Value : categoria.Descripcion.Trim());
            command.Parameters.AddWithValue("@PorcentajeGanancia", categoria.PorcentajeGanancia);
            command.Parameters.AddWithValue("@Estado", categoria.Estado);
            command.Parameters.AddWithValue("@IdCategoria", categoria.IdCategoria);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void CambiarEstado(int id)
        {
            const string query = @"UPDATE categoria 
                                  SET Estado = CASE
                                      WHEN Estado = 1 THEN 0
                                      ELSE 1
                                  END,
                                  FechaActualizacion = CURRENT_TIMESTAMP 
                                  WHERE IdCategoria = @IdCategoria;";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@IdCategoria", id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public int Count()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM categoria";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private Categoria MapearCategoria(MySqlDataReader reader)
        {
            return new Categoria
            {
                IdCategoria = reader.GetInt32("IdCategoria"),
                Codigo = reader["Codigo"].ToString() ?? "",
                Nombre = reader["Nombre"].ToString() ?? "",
                Descripcion = reader["Descripcion"] == DBNull.Value
                    ? null
                    : reader["Descripcion"].ToString(),
                PorcentajeGanancia = reader.GetDecimal("PorcentajeGanancia"),
                Estado = reader.GetByte("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion"),
                IdEmpleadoResponsable = reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}