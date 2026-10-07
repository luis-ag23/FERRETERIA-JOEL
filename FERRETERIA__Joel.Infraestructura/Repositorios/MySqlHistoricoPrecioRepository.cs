using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlHistoricoPrecioRepository : IHistoricoPrecioRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlHistoricoPrecioRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<HistoricoPrecio> ObtenerTodos()
        {
            List<HistoricoPrecio> historicos = new();

            const string query = @"
                SELECT
                    h.IdHistoricoPrecio,
                    h.IdProducto,
                    p.Nombre AS NombreProducto,
                    h.Precio,
                    h.FechaInicioVigencia,
                    h.FechaFinVigencia,
                    h.MotivoCambio,
                    h.Estado,
                    h.FechaRegistro,
                    h.FechaActualizacion,
                    h.IdEmpleadoResponsable,
                    e.Nombre AS NombreEmpleadoResponsable
                FROM historico_precio h
                INNER JOIN producto p
                    ON p.IdProducto = h.IdProducto
                INNER JOIN empleado e
                    ON e.IdEmpleado = h.IdEmpleadoResponsable
                ORDER BY h.FechaInicioVigencia DESC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                historicos.Add(MapearHistoricoPrecio(reader));
            }

            return historicos;
        }

        public List<HistoricoPrecio> ObtenerActivas()
        {
            List<HistoricoPrecio> historicos = new();

            const string query = @"
                SELECT
                    h.IdHistoricoPrecio,
                    h.IdProducto,
                    p.Nombre AS NombreProducto,
                    h.Precio,
                    h.FechaInicioVigencia,
                    h.FechaFinVigencia,
                    h.MotivoCambio,
                    h.Estado,
                    h.FechaRegistro,
                    h.FechaActualizacion,
                    h.IdEmpleadoResponsable,
                    e.Nombre AS NombreEmpleadoResponsable
                FROM historico_precio h
                INNER JOIN producto p
                    ON p.IdProducto = h.IdProducto
                INNER JOIN empleado e
                    ON e.IdEmpleado = h.IdEmpleadoResponsable
                WHERE h.Estado = 1
                ORDER BY h.FechaInicioVigencia DESC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                historicos.Add(MapearHistoricoPrecio(reader));
            }

            return historicos;
        }

        public HistoricoPrecio? ObtenerPorId(int id)
        {
            const string query = @"
                SELECT
                    h.IdHistoricoPrecio,
                    h.IdProducto,
                    p.Nombre AS NombreProducto,
                    h.Precio,
                    h.FechaInicioVigencia,
                    h.FechaFinVigencia,
                    h.MotivoCambio,
                    h.Estado,
                    h.FechaRegistro,
                    h.FechaActualizacion,
                    h.IdEmpleadoResponsable,
                    e.Nombre AS NombreEmpleadoResponsable
                FROM historico_precio h
                INNER JOIN producto p
                    ON p.IdProducto = h.IdProducto
                INNER JOIN empleado e
                    ON e.IdEmpleado = h.IdEmpleadoResponsable
                WHERE h.IdHistoricoPrecio = @id
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
                ? MapearHistoricoPrecio(reader)
                : null;
        }

        public int Insertar(HistoricoPrecio historicoPrecio)
        {
            const string query = @"
        INSERT INTO historico_precio
        (
            IdProducto,
            Precio,
            FechaInicioVigencia,
            FechaFinVigencia,
            MotivoCambio,
            Estado,
            IdEmpleadoResponsable
        )
        VALUES
        (
            @idProducto,
            @precio,
            CURRENT_TIMESTAMP,
            NULL,
            @motivoCambio,
            1,
            @idEmpleadoResponsable
        )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idProducto",
                historicoPrecio.IdProducto);

            command.Parameters.AddWithValue(
                "@precio",
                historicoPrecio.Precio);

            command.Parameters.AddWithValue(
                "@motivoCambio",
                historicoPrecio.MotivoCambio);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                historicoPrecio.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        public List<HistoricoPrecio> ObtenerPorProducto(int idProducto)
        {
            List<HistoricoPrecio> historicos = new();

            const string query = @"
                SELECT
                    h.IdHistoricoPrecio,
                    h.IdProducto,
                    p.Nombre AS NombreProducto,
                    h.Precio,
                    h.FechaInicioVigencia,
                    h.FechaFinVigencia,
                    h.MotivoCambio,
                    h.Estado,
                    h.FechaRegistro,
                    h.FechaActualizacion,
                    h.IdEmpleadoResponsable,
                    e.Nombre AS NombreEmpleadoResponsable
                FROM historico_precio h
                INNER JOIN producto p
                    ON p.IdProducto = h.IdProducto
                INNER JOIN empleado e
                    ON e.IdEmpleado = h.IdEmpleadoResponsable
                WHERE h.IdProducto = @IdProducto
                ORDER BY h.FechaInicioVigencia DESC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@IdProducto",
                idProducto);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                historicos.Add(MapearHistoricoPrecio(reader));
            }

            return historicos;
        }

        public HistoricoPrecio? ObtenerPrecioVigente(int idProducto)
        {
            const string query = @"
        SELECT
            IdHistoricoPrecio,
            IdProducto,
            Precio,
            FechaInicioVigencia,
            FechaFinVigencia,
            MotivoCambio,
            Estado,
            FechaRegistro,
            FechaActualizacion,
            IdEmpleadoResponsable
        FROM historico_precio
        WHERE IdProducto = @idProducto
        AND FechaFinVigencia IS NULL
        AND Estado = 1
        ORDER BY FechaInicioVigencia DESC
        LIMIT 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@idProducto", idProducto);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
                return null;

            return new HistoricoPrecio
            {
                IdHistoricoPrecio =
                    reader.GetInt32("IdHistoricoPrecio"),
                IdProducto =
                    reader.GetInt32("IdProducto"),
                Precio =
                    reader.GetDecimal("Precio"),
                FechaInicioVigencia =
                    reader.GetDateTime("FechaInicioVigencia"),
                FechaFinVigencia =
                    reader["FechaFinVigencia"] == DBNull.Value
                        ? null
                        : reader.GetDateTime("FechaFinVigencia"),
                MotivoCambio =
                    reader["MotivoCambio"].ToString() ?? "",
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
        public void CerrarPrecioVigente(int idProducto)
        {
            const string query = @"
        UPDATE historico_precio
        SET FechaFinVigencia = CURRENT_TIMESTAMP,
            FechaActualizacion = CURRENT_TIMESTAMP
        WHERE IdProducto = @idProducto
        AND FechaFinVigencia IS NULL
        AND Estado = 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idProducto",
                idProducto);

            connection.Open();

            command.ExecuteNonQuery();
        }
        private HistoricoPrecio MapearHistoricoPrecio(
            MySqlDataReader reader)
        {
            return new HistoricoPrecio
            {
                IdHistoricoPrecio =
                    reader.GetInt32("IdHistoricoPrecio"),

                IdProducto =
                    reader.GetInt32("IdProducto"),

                NombreProducto =
                    reader["NombreProducto"].ToString(),

                Precio =
                    reader.GetDecimal("Precio"),

                FechaInicioVigencia =
                    reader.GetDateTime("FechaInicioVigencia"),

                FechaFinVigencia =
                    reader["FechaFinVigencia"] == DBNull.Value
                        ? null
                        : reader.GetDateTime("FechaFinVigencia"),

                MotivoCambio =
                    reader["MotivoCambio"].ToString() ?? "",

                Estado =
                    reader.GetByte("Estado"),

                FechaRegistro =
                    reader.GetDateTime("FechaRegistro"),

                FechaActualizacion =
                    reader["FechaActualizacion"] == DBNull.Value
                        ? null
                        : reader.GetDateTime("FechaActualizacion"),

                IdEmpleadoResponsable =
                    reader.GetInt32("IdEmpleadoResponsable"),

                NombreEmpleadoResponsable =
                    reader["NombreEmpleadoResponsable"].ToString()
            };
        }
    }
}
