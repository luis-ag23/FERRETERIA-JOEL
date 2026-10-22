using MySql.Data.MySqlClient;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlMarcaRepository : IGestionarRepository<Marca>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ICacheSeleccion? _cache;

        public MySqlMarcaRepository(IDbConnectionFactory connectionFactory, ICacheSeleccion? cache = null)
        {
            _connectionFactory = connectionFactory;
            _cache = cache;
        }

        public virtual async Task<List<Marca>> ObtenerTodosAsync()
        {
            const string cacheKey = "cache:select:marcas:todos";
            if (_cache != null)
            {
                var resultado = await _cache.ObtenerAsync<List<Marca>>(cacheKey);
                if (resultado.EsHit && resultado.Valor is not null)
                    return resultado.Valor;
            }

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

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                marcas.Add(MapearMarca(reader));
            }

            if (_cache != null)
                await _cache.GuardarAsync(cacheKey, marcas);

            return marcas;
        }

        public virtual async Task<int> InsertarAsync(Marca marca)
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

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
            if (_cache != null)
                await _cache.InvalidarPorPrefijoAsync("cache:select:marcas:*");

            return Convert.ToInt32(command.LastInsertedId);
        }

        public virtual async Task ActualizarAsync(Marca marca)
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

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
            if (_cache != null)
                await _cache.InvalidarPorPrefijoAsync("cache:select:marcas:*");
        }

        public virtual async Task CambiarEstadoAsync(int id, int idEmpleadoResponsable)
        {
            const string query = @"
                UPDATE marca
                SET
                    Estado = CASE
                        WHEN Estado = 1 THEN 0
                        ELSE 1
                    END,
                    FechaActualizacion = CURRENT_TIMESTAMP,
                    IdEmpleadoResponsable = @idEmpleadoResponsable
                WHERE IdMarca = @idMarca";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idMarca", id);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                idEmpleadoResponsable);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
            if (_cache != null)
                await _cache.InvalidarPorPrefijoAsync("cache:select:marcas:*");
        }

        public virtual async Task<List<Marca>> MostrarActivosAsync()
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

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
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
