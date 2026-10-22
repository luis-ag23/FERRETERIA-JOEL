using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Repositorio de ventas. Implementa el contrato genérico
    /// <see cref="IRepository{T}"/> y expone las operaciones propias del flujo
    /// de venta (folios, y anulación) como métodos concretos que
    /// se inyectan directamente por DI. Participa de la transacción ambiente.
    /// </summary>
    public class MySqlVentaRepository : IRepository<Venta>
    {
        private const string Columnas = @"
            IdVenta,
            CodigoVenta,
            EstadoVenta,
            IdCliente,
            MetodoPago,
            EstadoPago,
            MontoTotal,
            FechaRegistro,
            FechaActualizacion,
            FechaAnulacion,
            MotivoAnulacion,
            IdEmpleadoAnulacion,
            IdEmpleadoResponsable";

        private readonly IConexionTransaccional _conexion;
        private readonly ICacheSeleccion? _cache;

        public MySqlVentaRepository(
            IConexionTransaccional conexion,
            ICacheSeleccion? cache = null)
        {
            _conexion = conexion;
            _cache = cache;
        }

        private async Task<MySqlCommand> CrearComandoAsync(string query)
        {
            MySqlCommand command = new(query, await _conexion.ObtenerConexionAsync());

            if (_conexion.Transaccion is not null)
            {
                command.Transaction = _conexion.Transaccion;
            }

            return command;
        }

        // ================== Contrato genérico ==================

        public virtual async Task<List<Venta>> ObtenerTodosAsync()
        {
            // Historial de ventas servido desde la caché distribuida: la
            // primera lectura va a MySQL y deja el JSON en Redis; las
            // siguientes la toman de Redis (mismo patrón cache-aside del
            // catálogo). La invalidación se hace tras el COMMIT, en las
            // fachadas, para no repoblar la caché con datos previos al
            // commit.
            const string cacheKey = "cache:select:ventas:todos";

            if (_cache != null)
            {
                var resultado = await _cache.ObtenerAsync<List<Venta>>(cacheKey);
                if (resultado.EsHit && resultado.Valor is not null)
                    return resultado.Valor;
            }

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

            if (_cache != null)
                await _cache.GuardarAsync(cacheKey, ventas);

            return ventas;
        }

        public virtual async Task<int> InsertarAsync(Venta venta)
        {
            const string query = @"
                INSERT INTO venta
                (
                    EstadoVenta,
                    IdCliente,
                    MetodoPago,
                    EstadoPago,
                    MontoTotal,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @estadoVenta,
                    @idCliente,
                    @metodoPago,
                    @estadoPago,
                    @montoTotal,
                    @idEmpleadoResponsable
                )";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@estadoVenta", venta.EstadoVenta);
            command.Parameters.AddWithValue("@idCliente", venta.IdCliente);
            command.Parameters.AddWithValue("@metodoPago", venta.MetodoPago);
            command.Parameters.AddWithValue("@estadoPago", venta.EstadoPago);
            command.Parameters.AddWithValue("@montoTotal", venta.MontoTotal);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                venta.IdEmpleadoResponsable);

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Operaciones propias de Ventas ==================

        /// <summary>
        /// Invalida la caché del historial de ventas. Se invoca DESPUÉS del
        /// COMMIT (en las fachadas) para que la siguiente lectura vuelva a
        /// MySQL y no sirva un historial anterior a la venta.
        /// </summary>
        public virtual async Task InvalidarCacheAsync()
        {
            if (_cache != null)
                await _cache.InvalidarPorPrefijoAsync("cache:select:ventas:*");
        }

        public virtual async Task AsignarCodigoAsync(int idVenta)
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

        public virtual async Task<Venta?> ObtenerPorIdAsync(int idVenta)
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

        public virtual async Task<bool> MarcarAnuladaAsync(
            int idVenta,
            int idEmpleadoAnulacion,
            string motivo)
        {
            const string query = @"
        UPDATE venta
        SET
            EstadoVenta = @estadoAnulada,
            MontoTotal = 0.00,
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