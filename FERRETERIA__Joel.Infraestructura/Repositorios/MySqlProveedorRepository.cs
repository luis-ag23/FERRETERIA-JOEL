using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    public class MySqlProveedorRepository : IRepository<Proveedor>, IModificacionRepository<Proveedor>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlProveedorRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }


        public List<Proveedor> ObtenerTodos()
        {
            List<Proveedor> proveedores = new();

            const string query = @"
                SELECT
                    IdProveedor,
                    RazonSocial,
                    NombreComercial,
                    NombreContacto,
                    Telefono,
                    CorreoElectronico,
                    Direccion,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM proveedor
                ORDER BY RazonSocial ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                proveedores.Add(MapearProveedor(reader));
            }

            return proveedores;
        }


        public List<Proveedor> ObtenerActivas()
        {
            List<Proveedor> proveedores = new();

            const string query = @"
                SELECT
                    IdProveedor,
                    RazonSocial,
                    NombreComercial,
                    NombreContacto,
                    Telefono,
                    CorreoElectronico,
                    Direccion,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM proveedor
                WHERE Estado = 1
                ORDER BY RazonSocial ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                proveedores.Add(MapearProveedor(reader));
            }

            return proveedores;
        }


        public Proveedor? ObtenerPorId(int idProveedor)
        {
            const string query = @"
                SELECT
                    IdProveedor,
                    RazonSocial,
                    NombreComercial,
                    NombreContacto,
                    Telefono,
                    CorreoElectronico,
                    Direccion,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM proveedor
                WHERE IdProveedor = @idProveedor";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idProveedor",
                idProveedor);

            connection.Open();

            using MySqlDataReader reader =
                command.ExecuteReader();

            if (!reader.Read())
            {
                return null;
            }

            return MapearProveedor(reader);
        }


        public int Insertar(Proveedor proveedor)
        {
            const string query = @"
                INSERT INTO proveedor
                (
                    RazonSocial,
                    NombreComercial,
                    NombreContacto,
                    Telefono,
                    CorreoElectronico,
                    Direccion,
                    Estado,
                    FechaRegistro,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @razonSocial,
                    @nombreComercial,
                    @nombreContacto,
                    @telefono,
                    @correoElectronico,
                    @direccion,
                    1,
                    CURRENT_TIMESTAMP,
                    @idEmpleadoResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@razonSocial",
                proveedor.RazonSocial);

            command.Parameters.AddWithValue(
                "@nombreComercial",
                proveedor.NombreComercial);

            command.Parameters.AddWithValue(
                "@nombreContacto",
                proveedor.NombreContacto);

            command.Parameters.AddWithValue(
                "@telefono",
                proveedor.Telefono);

            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)proveedor.CorreoElectronico
                ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@direccion",
                (object?)proveedor.Direccion
                ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                proveedor.IdEmpleadoResponsable);

            connection.Open();

            command.ExecuteNonQuery();

            return Convert.ToInt32(command.LastInsertedId);
        }


        public void Actualizar(Proveedor proveedor)
        {
            const string query = @"
                UPDATE proveedor
                SET
                    RazonSocial = @razonSocial,
                    NombreComercial = @nombreComercial,
                    NombreContacto = @nombreContacto,
                    Telefono = @telefono,
                    CorreoElectronico = @correoElectronico,
                    Direccion = @direccion,
                    Estado = @estado,
                    FechaActualizacion = CURRENT_TIMESTAMP,
                    IdEmpleadoResponsable = @idEmpleadoResponsable
                WHERE IdProveedor = @idProveedor";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idProveedor",
                proveedor.IdProveedor);

            command.Parameters.AddWithValue(
                "@razonSocial",
                proveedor.RazonSocial);

            command.Parameters.AddWithValue(
                "@nombreComercial",
                proveedor.NombreComercial);

            command.Parameters.AddWithValue(
                "@nombreContacto",
                proveedor.NombreContacto);

            command.Parameters.AddWithValue(
                "@telefono",
                proveedor.Telefono);

            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)proveedor.CorreoElectronico
                ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@direccion",
                (object?)proveedor.Direccion
                ?? DBNull.Value);

            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                proveedor.IdEmpleadoResponsable);

            command.Parameters.AddWithValue(
                "@estado",
                proveedor.Estado);

            connection.Open();

            command.ExecuteNonQuery();
        }


        public void CambiarEstado(int idProveedor)
        {
            const string query = @"
                UPDATE proveedor
                SET
                    Estado = CASE
                        WHEN Estado = 1 THEN 0
                        ELSE 1
                    END,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdProveedor = @idProveedor";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@idProveedor",
                idProveedor);

            connection.Open();

            command.ExecuteNonQuery();
        }


        public int Count()
        {
            const string query = @"
                SELECT COUNT(*)
                FROM proveedor";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command =
                new MySqlCommand(query, connection);

            connection.Open();

            return Convert.ToInt32(command.ExecuteScalar());
        }


        private Proveedor MapearProveedor(MySqlDataReader reader)
        {
            return new Proveedor
            {
                IdProveedor =
                    reader.GetInt32("IdProveedor"),

                RazonSocial =
                    reader["RazonSocial"].ToString() ?? "",

                NombreComercial =
                    reader["NombreComercial"].ToString() ?? "",

                NombreContacto =
                    reader["NombreContacto"].ToString() ?? "",

                Telefono =
                    reader["Telefono"].ToString() ?? "",

                CorreoElectronico =
                    reader["CorreoElectronico"] == DBNull.Value
                        ? null
                        : reader["CorreoElectronico"].ToString(),

                Direccion =
                    reader["Direccion"] == DBNull.Value
                        ? null
                        : reader["Direccion"].ToString(),

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
