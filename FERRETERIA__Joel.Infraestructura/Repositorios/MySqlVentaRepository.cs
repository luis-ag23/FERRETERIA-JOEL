using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlVentaRepository : IVentaRepository
    {
        private const string Columnas = @"
            IdVenta,
            CodigoVenta,
            EstadoVenta,
            IdCliente,
            MetodoPago,
            EstadoPago,
            MontoTotal,
            Subtotal,
            MontoDescuento,
            PorcentajeDescuento,
            ClaveIdempotencia,
            FechaRegistro,
            FechaActualizacion,
            FechaAnulacion,
            MotivoAnulacion,
            IdEmpleadoAnulacion,
            IdEmpleadoResponsable";

        private readonly IConexionTransaccional _conexion;

        public MySqlVentaRepository(IConexionTransaccional conexion)
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

        public async Task<List<Venta>> ObtenerTodosAsync()
        {
            List<Venta> ventas = new();

            string query = $@"
                SELECT {Columnas}
                FROM venta
                ORDER BY FechaRegistro DESC";

            using MySqlCommand command = await CrearComandoAsync(query);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ventas.Add(MapearVenta(reader));
            }

            return ventas;
        }

        public async Task<int> InsertarAsync(Venta venta)
        {
            const string query = @"
                INSERT INTO venta
                (
                    EstadoVenta,
                    IdCliente,
                    MetodoPago,
                    EstadoPago,
                    MontoTotal,
                    Subtotal,
                    MontoDescuento,
                    PorcentajeDescuento,
                    ClaveIdempotencia,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @estadoVenta,
                    @idCliente,
                    @metodoPago,
                    @estadoPago,
                    @montoTotal,
                    @subtotal,
                    @montoDescuento,
                    @porcentajeDescuento,
                    @claveIdempotencia,
                    @idEmpleadoResponsable
                )";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@estadoVenta", venta.EstadoVenta);
            command.Parameters.AddWithValue("@idCliente", venta.IdCliente);
            command.Parameters.AddWithValue("@metodoPago", venta.MetodoPago);
            command.Parameters.AddWithValue("@estadoPago", venta.EstadoPago);
            command.Parameters.AddWithValue("@montoTotal", venta.MontoTotal);
            command.Parameters.AddWithValue("@subtotal", venta.Subtotal);
            command.Parameters.AddWithValue("@montoDescuento", venta.MontoDescuento);
            command.Parameters.AddWithValue("@porcentajeDescuento", venta.PorcentajeDescuento);
            command.Parameters.AddWithValue(
                "@claveIdempotencia",
                (object?)venta.ClaveIdempotencia ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                venta.IdEmpleadoResponsable);

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        public async Task AsignarCodigoAsync(int idVenta)
        {
            const string query = @"
                UPDATE venta
                SET CodigoVenta = CONCAT(
                        'V-',
                        DATE_FORMAT(FechaRegistro, '%Y%m%d'),
                        '-',
                        LPAD(IdVenta, 6, '0'))
                WHERE IdVenta = @idVenta";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idVenta", idVenta);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<Venta?> ObtenerPorIdAsync(int idVenta)
        {
            string query = $@"
                SELECT {Columnas}
                FROM venta
                WHERE IdVenta = @idVenta
                LIMIT 1";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idVenta", idVenta);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapearVenta(reader) : null;
        }

        public async Task<Venta?> ObtenerPorClaveIdempotenciaAsync(
            string claveIdempotencia)
        {
            string query = $@"
                SELECT {Columnas}
                FROM venta
                WHERE ClaveIdempotencia = @clave
                LIMIT 1";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@clave", claveIdempotencia);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapearVenta(reader) : null;
        }

        public async Task<bool> MarcarAnuladaAsync(
            int idVenta,
            int idEmpleadoAnulacion,
            string motivo)
        {
            const string query = @"
                UPDATE venta
                SET
                    EstadoVenta = @estadoAnulada,
                    FechaAnulacion = CURRENT_TIMESTAMP,
                    MotivoAnulacion = @motivo,
                    IdEmpleadoAnulacion = @idEmpleadoAnulacion,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdVenta = @idVenta
                  AND EstadoVenta = @estadoRegistrada";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idVenta", idVenta);
            command.Parameters.AddWithValue("@estadoAnulada", EstadoVenta.Anulada);
            command.Parameters.AddWithValue("@estadoRegistrada", EstadoVenta.Registrada);
            command.Parameters.AddWithValue("@motivo", motivo);
            command.Parameters.AddWithValue("@idEmpleadoAnulacion", idEmpleadoAnulacion);

            return await command.ExecuteNonQueryAsync() == 1;
        }

        private static Venta MapearVenta(MySqlDataReader reader)
        {
            return new Venta
            {
                IdVenta = reader.GetInt32("IdVenta"),
                CodigoVenta = reader["CodigoVenta"] == DBNull.Value
                    ? null
                    : reader["CodigoVenta"].ToString(),
                EstadoVenta = reader["EstadoVenta"].ToString() ?? EstadoVenta.Registrada,
                IdCliente = reader.GetInt32("IdCliente"),
                MetodoPago = reader["MetodoPago"].ToString() ?? "",
                EstadoPago = reader["EstadoPago"].ToString() ?? "",
                MontoTotal = reader.GetDecimal("MontoTotal"),
                Subtotal = reader.GetDecimal("Subtotal"),
                MontoDescuento = reader.GetDecimal("MontoDescuento"),
                PorcentajeDescuento = reader.GetDecimal("PorcentajeDescuento"),
                ClaveIdempotencia = reader["ClaveIdempotencia"] == DBNull.Value
                    ? null
                    : reader["ClaveIdempotencia"].ToString(),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion"),
                FechaAnulacion = reader["FechaAnulacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaAnulacion"),
                MotivoAnulacion = reader["MotivoAnulacion"] == DBNull.Value
                    ? null
                    : reader["MotivoAnulacion"].ToString(),
                IdEmpleadoAnulacion = reader["IdEmpleadoAnulacion"] == DBNull.Value
                    ? null
                    : reader.GetInt32("IdEmpleadoAnulacion"),
                IdEmpleadoResponsable = reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}
