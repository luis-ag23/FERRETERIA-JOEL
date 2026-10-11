using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlDetalleVentaRepository : IDetalleVentaRepository
    {
        private const string Columnas = @"
            IdDetalleVenta,
            IdVenta,
            IdProducto,
            Cantidad,
            PrecioUnitario,
            TotalProducto,
            FechaRegistro,
            IdEmpleadoResponsable";

        private readonly IConexionTransaccional _conexion;

        public MySqlDetalleVentaRepository(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        private async Task<MySqlCommand> CrearComandoAsync(string query)
        {
            MySqlConnection conexion = await _conexion.ObtenerConexionAsync();

            MySqlCommand command = new(query, conexion);

            if (_conexion.Transaccion is not null)
            {
                command.Transaction = _conexion.Transaccion;
            }

            return command;
        }

        public async Task<List<DetalleVenta>> ObtenerPorVentaAsync(int idVenta)
        {
            List<DetalleVenta> detalles = new();

            string query = $@"
                SELECT {Columnas}
                FROM detalle_venta
                WHERE IdVenta = @idVenta
                ORDER BY IdDetalleVenta ASC";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idVenta", idVenta);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                detalles.Add(MapearDetalleVenta(reader));
            }

            return detalles;
        }

        public async Task<int> InsertarAsync(DetalleVenta detalleVenta)
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

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idVenta", detalleVenta.IdVenta);
            command.Parameters.AddWithValue("@idProducto", detalleVenta.IdProducto);
            command.Parameters.AddWithValue("@cantidad", detalleVenta.Cantidad);
            command.Parameters.AddWithValue("@precioUnitario", detalleVenta.PrecioUnitario);
            command.Parameters.AddWithValue("@totalProducto", detalleVenta.TotalProducto);
            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                detalleVenta.IdEmpleadoResponsable);

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        private static DetalleVenta MapearDetalleVenta(MySqlDataReader reader)
        {
            return new DetalleVenta
            {
                IdDetalleVenta = reader.GetInt32("IdDetalleVenta"),
                IdVenta = reader.GetInt32("IdVenta"),
                IdProducto = reader.GetInt32("IdProducto"),
                Cantidad = reader.GetInt32("Cantidad"),
                PrecioUnitario = reader.GetDecimal("PrecioUnitario"),
                TotalProducto = reader.GetDecimal("TotalProducto"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                IdEmpleadoResponsable = reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}
