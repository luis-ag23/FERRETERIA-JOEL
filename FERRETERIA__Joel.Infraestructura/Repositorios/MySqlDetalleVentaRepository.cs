using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlDetalleVentaRepository : IRepository<DetalleVenta>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlDetalleVentaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<DetalleVenta> ObtenerTodos()
        {
            List<DetalleVenta> detalles = new();

            const string query = @"
                SELECT
                    IdDetalleVenta,
                    IdVenta,
                    IdProducto,
                    Cantidad,
                    PrecioUnitario,
                    TotalProducto,
                    FechaRegistro,
                    IdEmpleadoResponsable
                FROM detalle_venta
                ORDER BY IdDetalleVenta ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                detalles.Add(MapearDetalleVenta(reader));
            }

            return detalles;
        }

        public int Insertar(DetalleVenta detalleVenta)
        {
            const string query = @"
                INSERT INTO detalle_venta
                (
                    IdVenta,
                    IdProducto,
                    Cantidad,
                    PrecioUnitario,
                    TotalProducto,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @idVenta,
                    @idProducto,
                    @cantidad,
                    @precioUnitario,
                    @totalProducto,
                    @idEmpleadoResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idVenta",
                detalleVenta.IdVenta);

            command.Parameters.AddWithValue(
                "@idProducto",
                detalleVenta.IdProducto);

            command.Parameters.AddWithValue(
                "@cantidad",
                detalleVenta.Cantidad);

            command.Parameters.AddWithValue(
                "@precioUnitario",
                detalleVenta.PrecioUnitario);

            command.Parameters.AddWithValue(
                "@totalProducto",
                detalleVenta.TotalProducto);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                detalleVenta.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Funciones fuera del contrato ==================

        public List<DetalleVenta> ObtenerActivas()
        {
            return ObtenerTodos();
        }

        public DetalleVenta? ObtenerPorId(int id)
        {
            const string query = @"
                SELECT
                    IdDetalleVenta,
                    IdVenta,
                    IdProducto,
                    Cantidad,
                    PrecioUnitario,
                    TotalProducto,
                    FechaRegistro,
                    IdEmpleadoResponsable
                FROM detalle_venta
                WHERE IdDetalleVenta = @id
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
                ? MapearDetalleVenta(reader)
                : null;
        }

        public void Actualizar(DetalleVenta detalleVenta)
        {
            const string query = @"
                UPDATE detalle_venta
                SET
                    IdVenta = @idVenta,
                    IdProducto = @idProducto,
                    Cantidad = @cantidad,
                    PrecioUnitario = @precioUnitario,
                    TotalProducto = @totalProducto,
                    IdEmpleadoResponsable = @idEmpleadoResponsable
                WHERE IdDetalleVenta = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", detalleVenta.IdDetalleVenta);

            command.Parameters.AddWithValue(
                "@idVenta",
                detalleVenta.IdVenta);

            command.Parameters.AddWithValue(
                "@idProducto",
                detalleVenta.IdProducto);

            command.Parameters.AddWithValue(
                "@cantidad",
                detalleVenta.Cantidad);

            command.Parameters.AddWithValue(
                "@precioUnitario",
                detalleVenta.PrecioUnitario);

            command.Parameters.AddWithValue(
                "@totalProducto",
                detalleVenta.TotalProducto);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                detalleVenta.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public void CambiarEstado(int id)
        {
            const string query = @"
                DELETE FROM detalle_venta
                WHERE IdDetalleVenta = @id";

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
                FROM detalle_venta";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private DetalleVenta MapearDetalleVenta(MySqlDataReader reader)
        {
            return new DetalleVenta
            {
                IdDetalleVenta =
                    reader.GetInt32("IdDetalleVenta"),

                IdVenta =
                    reader.GetInt32("IdVenta"),

                IdProducto =
                    reader.GetInt32("IdProducto"),

                Cantidad =
                    reader.GetInt32("Cantidad"),

                PrecioUnitario =
                    reader.GetDecimal("PrecioUnitario"),

                TotalProducto =
                    reader.GetDecimal("TotalProducto"),

                FechaRegistro =
                    reader.GetDateTime("FechaRegistro"),

                IdEmpleadoResponsable =
                    reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}
