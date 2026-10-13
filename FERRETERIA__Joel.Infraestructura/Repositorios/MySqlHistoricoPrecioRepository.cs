using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Repositorio del histórico de precios. Implementa
    /// <see cref="IRepository{T}"/> y participa de la transacción ambiente:
    /// cierre del precio vigente y alta del nuevo precio se confirman juntos
    /// con la actualización del producto.
    /// </summary>
    public class MySqlHistoricoPrecioRepository : IRepository<HistoricoPrecio>
    {
        private const string Columnas = @"
            h.IdHistoricoPrecio,
            h.IdProducto,
            p.Nombre AS NombreProducto,
            h.Precio,
            h.FechaInicioVigencia,
            h.FechaFinVigencia,
            h.MotivoCambio,
            h.FechaRegistro,
            h.IdEmpleadoResponsable,
            e.Nombre AS NombreEmpleadoResponsable";

        private const string Joins = @"
            FROM historico_precio h
            INNER JOIN producto p ON p.IdProducto = h.IdProducto
            INNER JOIN empleado e ON e.IdEmpleado = h.IdEmpleadoResponsable";

        private readonly IConexionTransaccional _conexion;

        public MySqlHistoricoPrecioRepository(IConexionTransaccional conexion)
        {
            _conexion = conexion;
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

        public virtual async Task<List<HistoricoPrecio>> ObtenerTodosAsync()
        {
            List<HistoricoPrecio> historicos = new();

            string query = $@"
                SELECT {Columnas}
                {Joins}
                ORDER BY h.FechaInicioVigencia DESC";

            using MySqlCommand command = await CrearComandoAsync(query);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                historicos.Add(MapearHistoricoPrecio(reader));
            }

            return historicos;
        }

        public virtual async Task<int> InsertarAsync(HistoricoPrecio historicoPrecio)
        {
            const string query = @"
                INSERT INTO historico_precio
                (
                    IdProducto,
                    Precio,
                    FechaInicioVigencia,
                    FechaFinVigencia,
                    MotivoCambio,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @idProducto,
                    @precio,
                    CURRENT_TIMESTAMP,
                    NULL,
                    @motivoCambio,
                    @idEmpleadoResponsable
                )";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idProducto", historicoPrecio.IdProducto);
            command.Parameters.AddWithValue("@precio", historicoPrecio.Precio);
            command.Parameters.AddWithValue("@motivoCambio", historicoPrecio.MotivoCambio);
            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                historicoPrecio.IdEmpleadoResponsable);

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Operaciones propias ==================

        public virtual async Task<List<HistoricoPrecio>> ObtenerPorProductoAsync(int idProducto)
        {
            List<HistoricoPrecio> historicos = new();

            string query = $@"
                SELECT {Columnas}
                {Joins}
                WHERE h.IdProducto = @idProducto
                ORDER BY h.FechaInicioVigencia DESC";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idProducto", idProducto);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                historicos.Add(MapearHistoricoPrecio(reader));
            }

            return historicos;
        }

        public virtual async Task<HistoricoPrecio?> ObtenerPrecioVigenteAsync(int idProducto)
        {
            const string query = @"
                SELECT
                    IdHistoricoPrecio,
                    IdProducto,
                    Precio,
                    FechaInicioVigencia,
                    FechaFinVigencia,
                    MotivoCambio,
                    FechaRegistro,
                    IdEmpleadoResponsable
                FROM historico_precio
                WHERE IdProducto = @idProducto
                  AND FechaFinVigencia IS NULL
                ORDER BY FechaInicioVigencia DESC
                LIMIT 1";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idProducto", idProducto);

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new HistoricoPrecio
            {
                IdHistoricoPrecio = reader.GetInt32("IdHistoricoPrecio"),
                IdProducto = reader.GetInt32("IdProducto"),
                Precio = reader.GetDecimal("Precio"),
                FechaInicioVigencia = reader.GetDateTime("FechaInicioVigencia"),
                FechaFinVigencia = reader["FechaFinVigencia"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaFinVigencia"),
                MotivoCambio = reader["MotivoCambio"].ToString() ?? "",
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                IdEmpleadoResponsable = reader.GetInt32("IdEmpleadoResponsable")
            };
        }

        public virtual async Task CerrarPrecioVigenteAsync(int idProducto)
        {
            const string query = @"
                UPDATE historico_precio
                SET FechaFinVigencia = CURRENT_TIMESTAMP
                WHERE IdProducto = @idProducto
                  AND FechaFinVigencia IS NULL";

            using MySqlCommand command = await CrearComandoAsync(query);

            command.Parameters.AddWithValue("@idProducto", idProducto);

            await command.ExecuteNonQueryAsync();
        }

        private static HistoricoPrecio MapearHistoricoPrecio(MySqlDataReader reader)
        {
            return new HistoricoPrecio
            {
                IdHistoricoPrecio = reader.GetInt32("IdHistoricoPrecio"),
                IdProducto = reader.GetInt32("IdProducto"),
                NombreProducto = reader["NombreProducto"].ToString(),
                Precio = reader.GetDecimal("Precio"),
                FechaInicioVigencia = reader.GetDateTime("FechaInicioVigencia"),
                FechaFinVigencia = reader["FechaFinVigencia"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaFinVigencia"),
                MotivoCambio = reader["MotivoCambio"].ToString() ?? "",
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                IdEmpleadoResponsable = reader.GetInt32("IdEmpleadoResponsable"),
                NombreEmpleadoResponsable = reader["NombreEmpleadoResponsable"].ToString()
            };
        }
    }
}
