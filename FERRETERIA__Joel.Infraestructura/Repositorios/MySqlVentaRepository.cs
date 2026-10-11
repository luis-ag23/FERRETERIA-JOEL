using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlVentaRepository : IRepository<Venta>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlVentaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Venta> ObtenerTodos()
        {
            List<Venta> ventas = new();

            const string query = @"
                SELECT
                    IdVenta,
                    CodigoVenta,
                    IdCliente,
                    MetodoPago,
                    EstadoPago,
                    MontoTotal,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM venta
                ORDER BY FechaRegistro DESC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                ventas.Add(MapearVenta(reader));
            }

            return ventas;
        }

        public int Insertar(Venta venta)
        {
            const string query = @"
                INSERT INTO venta
                (
                    CodigoVenta,
                    IdCliente,
                    MetodoPago,
                    EstadoPago,
                    MontoTotal,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @codigoVenta,
                    @idCliente,
                    @metodoPago,
                    @estadoPago,
                    @montoTotal,
                    @idEmpleadoResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@codigoVenta",
                venta.CodigoVenta);

            command.Parameters.AddWithValue(
                "@idCliente",
                venta.IdCliente);

            command.Parameters.AddWithValue(
                "@metodoPago",
                venta.MetodoPago);

            command.Parameters.AddWithValue(
                "@estadoPago",
                venta.EstadoPago);

            command.Parameters.AddWithValue(
                "@montoTotal",
                venta.MontoTotal);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                venta.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Funciones fuera del contrato ==================

        public List<Venta> ObtenerActivas()
        {
            return ObtenerTodos();
        }

        public Venta? ObtenerPorId(int id)
        {
            const string query = @"
                SELECT
                    IdVenta,
                    CodigoVenta,
                    IdCliente,
                    MetodoPago,
                    EstadoPago,
                    MontoTotal,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM venta
                WHERE IdVenta = @id
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
                ? MapearVenta(reader)
                : null;
        }

        public void Actualizar(Venta venta)
        {
            const string query = @"
                UPDATE venta
                SET
                    CodigoVenta = @codigoVenta,
                    IdCliente = @idCliente,
                    MetodoPago = @metodoPago,
                    EstadoPago = @estadoPago,
                    MontoTotal = @montoTotal,
                    FechaActualizacion = CURRENT_TIMESTAMP,
                    IdEmpleadoResponsable = @idEmpleadoResponsable
                WHERE IdVenta = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", venta.IdVenta);

            command.Parameters.AddWithValue(
                "@codigoVenta",
                venta.CodigoVenta);

            command.Parameters.AddWithValue(
                "@idCliente",
                venta.IdCliente);

            command.Parameters.AddWithValue(
                "@metodoPago",
                venta.MetodoPago);

            command.Parameters.AddWithValue(
                "@estadoPago",
                venta.EstadoPago);

            command.Parameters.AddWithValue(
                "@montoTotal",
                venta.MontoTotal);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                venta.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public int Count()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM venta";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private Venta MapearVenta(MySqlDataReader reader)
        {
            return new Venta
            {
                IdVenta =
                    reader.GetInt32("IdVenta"),

                CodigoVenta =
                    reader["CodigoVenta"].ToString() ?? "",

                IdCliente =
                    reader.GetInt32("IdCliente"),

                MetodoPago =
                    reader["MetodoPago"].ToString() ?? "",

                EstadoPago =
                    reader["EstadoPago"].ToString() ?? "",

                MontoTotal =
                    reader.GetDecimal("MontoTotal"),

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
