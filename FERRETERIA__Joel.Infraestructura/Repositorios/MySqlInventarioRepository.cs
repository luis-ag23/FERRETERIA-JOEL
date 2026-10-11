using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlInventarioRepository : IInventarioRepository
    {
        private readonly IConexionTransaccional _conexion;

        public MySqlInventarioRepository(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        private MySqlCommand CrearComando(string query)
        {
            MySqlCommand command = new(query, _conexion.Conexion);

            if (_conexion.Transaccion is not null)
            {
                command.Transaction = _conexion.Transaccion;
            }

            return command;
        }

        public Producto? ObtenerParaVenta(int idProducto)
        {
            const string query = @"
                SELECT
                    p.IdProducto,
                    p.IdCategoria,
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.Marca AS IdMarca,
                    p.UnidadMedida,
                    p.PrecioVenta,
                    p.CantidadStock,
                    p.Estado,
                    p.FechaRegistro,
                    p.FechaActualizacion,
                    p.IdEmpleadoResponsable
                FROM producto p
                WHERE p.IdProducto = @idProducto
                  AND p.Estado = 1
                LIMIT 1";

            using MySqlCommand command = CrearComando(query);

            command.Parameters.AddWithValue("@idProducto", idProducto);

            using MySqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return new Producto
            {
                IdProducto = reader.GetInt32("IdProducto"),
                IdCategoria = reader.GetInt32("IdCategoria"),
                IdMarca = reader.GetInt32("IdMarca"),
                Codigo = reader["Codigo"].ToString() ?? "",
                Nombre = reader["Nombre"].ToString() ?? "",
                Descripcion = reader["Descripcion"] == DBNull.Value
                    ? null
                    : reader["Descripcion"].ToString(),
                UnidadMedida = reader["UnidadMedida"].ToString() ?? "",
                PrecioVenta = reader.GetDecimal("PrecioVenta"),
                CantidadStock = reader.GetInt32("CantidadStock"),
                Estado = reader.GetByte("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion"),
                IdEmpleadoResponsable = reader.GetInt32("IdEmpleadoResponsable")
            };
        }

        public bool Descontar(int idProducto, int cantidad)
        {
            const string query = @"
                UPDATE producto
                SET
                    CantidadStock = CantidadStock - @cantidad,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdProducto = @idProducto
                  AND Estado = 1
                  AND CantidadStock >= @cantidad";

            using MySqlCommand command = CrearComando(query);

            command.Parameters.AddWithValue("@idProducto", idProducto);
            command.Parameters.AddWithValue("@cantidad", cantidad);

            return command.ExecuteNonQuery() == 1;
        }

        public void Reponer(int idProducto, int cantidad)
        {
            const string query = @"
                UPDATE producto
                SET
                    CantidadStock = CantidadStock + @cantidad,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdProducto = @idProducto";

            using MySqlCommand command = CrearComando(query);

            command.Parameters.AddWithValue("@idProducto", idProducto);
            command.Parameters.AddWithValue("@cantidad", cantidad);

            command.ExecuteNonQuery();
        }
    }
}
