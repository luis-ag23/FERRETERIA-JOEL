using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlProductoRepository : IRepository<Producto>, IModificacionRepository<Producto>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlProductoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        public List<Producto> ObtenerTodos()
        {
            List<Producto> productos = new();

            const string query = @"
                SELECT
                    p.IdProducto,
                    p.IdCategoria,
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.Marca AS IdMarca,
                    m.Nombre AS NombreMarca,
                    p.UnidadMedida,
                    p.PrecioVenta,
                    p.Estado,
                    p.FechaRegistro,
                    p.FechaActualizacion,
                    p.IdEmpleadoResponsable
                FROM producto p
                LEFT JOIN marca m ON m.IdMarca = p.Marca
                ORDER BY p.Nombre ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(MapearProducto(reader));
            }

            return productos;
        }

        public List<Producto> ObtenerActivas()
        {
            List<Producto> productos = new();

            const string query = @"
                SELECT
                    p.IdProducto,
                    p.IdCategoria,
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.Marca AS IdMarca,
                    m.Nombre AS NombreMarca,
                    p.UnidadMedida,
                    p.PrecioVenta,
                    p.Estado,
                    p.FechaRegistro,
                    p.FechaActualizacion,
                    p.IdEmpleadoResponsable
                FROM producto p
                LEFT JOIN marca m ON m.IdMarca = p.Marca
                WHERE p.Estado = 1
                ORDER BY p.Nombre ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(MapearProducto(reader));
            }

            return productos;
        }


        public Producto? ObtenerPorId(int idProducto)
        {
            const string query = @"
                SELECT
                    p.IdProducto,
                    p.IdCategoria,
                    p.Codigo,
                    p.Nombre,
                    p.Descripcion,
                    p.Marca AS IdMarca,
                    m.Nombre AS NombreMarca,
                    p.UnidadMedida,
                    p.PrecioVenta,
                    p.Estado,
                    p.FechaRegistro,
                    p.FechaActualizacion,
                    p.IdEmpleadoResponsable
                FROM producto p
                LEFT JOIN marca m ON m.IdMarca = p.Marca
                WHERE p.IdProducto = @idProducto";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idProducto",
                idProducto);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return MapearProducto(reader);
        }


        public int Insertar(Producto producto)
        {
            const string query = @"
                INSERT INTO producto
                (
                    IdCategoria,
                    Codigo,
                    Nombre,
                    Descripcion,
                    Marca,
                    UnidadMedida,
                    PrecioVenta,
                    Estado,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @idCategoria,
                    @codigo,
                    @nombre,
                    @descripcion,
                    @idMarca,
                    @unidadMedida,
                    @precioVenta,
                    1,
                    @idEmpleadoResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idCategoria",
                producto.IdCategoria);

            command.Parameters.AddWithValue(
                "@codigo",
                producto.Codigo);

            command.Parameters.AddWithValue(
                "@nombre",
                producto.Nombre);

            command.Parameters.AddWithValue(
                "@descripcion",
                (object?)producto.Descripcion ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@idMarca",
                producto.IdMarca);

            command.Parameters.AddWithValue(
                "@unidadMedida",
                producto.UnidadMedida);

            command.Parameters.AddWithValue(
                "@precioVenta",
                producto.PrecioVenta);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                producto.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();
            return Convert.ToInt32(command.LastInsertedId);
        }


        public void Actualizar(Producto producto)
        {
            const string query = @"
                UPDATE producto
                SET
                    IdCategoria = @idCategoria,
                    Codigo = @codigo,
                    Nombre = @nombre,
                    Descripcion = @descripcion,
                    Marca = @idMarca,
                    UnidadMedida = @unidadMedida,
                    PrecioVenta = @precioVenta,
                    IdEmpleadoResponsable = @idEmpleadoResponsable,
                    Estado = @estado,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdProducto = @idProducto";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idProducto",
                producto.IdProducto);

            command.Parameters.AddWithValue(
                "@idCategoria",
                producto.IdCategoria);

            command.Parameters.AddWithValue(
                "@codigo",
                producto.Codigo);

            command.Parameters.AddWithValue(
                "@nombre",
                producto.Nombre);

            command.Parameters.AddWithValue(
                "@descripcion",
                (object?)producto.Descripcion ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@idMarca",
                producto.IdMarca);

            command.Parameters.AddWithValue(
                "@unidadMedida",
                producto.UnidadMedida);

            command.Parameters.AddWithValue(
                "@precioVenta",
                producto.PrecioVenta);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                producto.IdEmpleadoResponsable);

            command.Parameters.AddWithValue(
                "@estado",
                producto.Estado);

            connection.Open();

            command.ExecuteNonQuery();
        }


        public void CambiarEstado(int idProducto)
        {
            const string query = @"
                UPDATE producto
                SET
                    Estado = CASE
                        WHEN Estado = 1 THEN 0
                        ELSE 1
                    END,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdProducto = @idProducto";

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


        public int Count()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM producto";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private Producto MapearProducto(MySqlDataReader reader)
        {
            return new Producto
            {
                IdProducto =
                    reader.GetInt32("IdProducto"),

                IdCategoria =
                    reader.GetInt32("IdCategoria"),

                IdMarca =
                    reader.GetInt32("IdMarca"),

                Codigo =
                    reader["Codigo"].ToString() ?? "",

                Nombre =
                    reader["Nombre"].ToString() ?? "",

                Descripcion =
                    reader["Descripcion"] == DBNull.Value
                    ? null
                    : reader["Descripcion"].ToString(),

                NombreMarca =
                    reader["NombreMarca"] == DBNull.Value
                    ? null
                    : reader["NombreMarca"].ToString(),

                UnidadMedida =
                    reader["UnidadMedida"].ToString() ?? "",

                PrecioVenta =
                    reader.GetDecimal("PrecioVenta"),

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
