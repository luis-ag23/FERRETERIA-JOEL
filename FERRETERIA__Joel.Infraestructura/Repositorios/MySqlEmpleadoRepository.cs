using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Repositorio de empleados. Implementa el contrato genérico
    /// <see cref="IRepository{T}"/> y expone operaciones propias de la
    /// autenticación y del flujo de activación por correo (búsqueda por correo,
    /// búsqueda y consumo del token, y actualización de contraseña) que se
    /// inyectan directamente por DI.
    /// </summary>
    public class MySqlEmpleadoRepository : IRepository<Empleado>
    {
        private const string Columnas = @"
            IdEmpleado,
            Nombre,
            Apellido,
            Rol,
            Telefono,
            CorreoElectronico,
            Contrasena,
            TokenActivacion,
            TokenExpira,
            TokenUsado,
            Estado,
            FechaRegistro,
            FechaActualizacion,
            IdEmpleadoResponsable";

        private readonly IDbConnectionFactory _connectionFactory;

        public MySqlEmpleadoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // ================== Contrato genérico ==================

        public virtual async Task<List<Empleado>> ObtenerTodosAsync()
        {
            List<Empleado> empleados = new();

            string query = $@"
                SELECT {Columnas}
                FROM empleado
                ORDER BY Nombre ASC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                empleados.Add(MapearEmpleado(reader));
            }

            return empleados;
        }

        public virtual async Task<int> InsertarAsync(Empleado empleado)
        {
            const string query = @"
                INSERT INTO empleado
                (
                    Nombre,
                    Apellido,
                    Rol,
                    Telefono,
                    CorreoElectronico,
                    Contrasena,
                    TokenActivacion,
                    TokenExpira,
                    TokenUsado,
                    Estado,
                    IdEmpleadoResponsable
                )
                VALUES
                (
                    @nombre,
                    @apellido,
                    @rol,
                    @telefono,
                    @correoElectronico,
                    @contrasena,
                    @tokenActivacion,
                    @tokenExpira,
                    @tokenUsado,
                    @estado,
                    @idEmpleadoResponsable
                )";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@nombre", empleado.Nombre);
            command.Parameters.AddWithValue("@apellido", empleado.Apellido);
            command.Parameters.AddWithValue("@rol", empleado.Rol);
            command.Parameters.AddWithValue("@telefono", empleado.Telefono);
            command.Parameters.AddWithValue(
                "@correoElectronico",
                (object?)empleado.CorreoElectronico ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@contrasena",
                (object?)empleado.Contrasena ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@tokenActivacion",
                (object?)empleado.TokenActivacion ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@tokenExpira",
                (object?)empleado.TokenExpira ?? DBNull.Value);
            command.Parameters.AddWithValue("@tokenUsado", empleado.TokenUsado);
            command.Parameters.AddWithValue("@estado", empleado.Estado);
            command.Parameters.AddWithValue(
                "@idEmpleadoResponsable",
                (object?)empleado.IdEmpleadoResponsable ?? DBNull.Value);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();

            return Convert.ToInt32(command.LastInsertedId);
        }

        // ================== Autenticación ==================

        public virtual async Task<Empleado?> ObtenerPorCorreoAsync(string correo)
        {
            string query = $@"
                SELECT {Columnas}
                FROM empleado
                WHERE CorreoElectronico = @correo
                LIMIT 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@correo", correo);

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            return await reader.ReadAsync()
                ? MapearEmpleado(reader)
                : null;
        }

        public virtual async Task ActualizarContrasenaAsync(
            int id,
            string contrasena)
        {
            const string query = @"
                UPDATE empleado
                SET
                    Contrasena = @contrasena,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdEmpleado = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@contrasena", contrasena);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        // ================== Activación por correo ==================

        /// <summary>
        /// Busca al empleado dueño de un token de activación, sin importar si
        /// ya venció o se usó: la capa de aplicación decide si es válido.
        /// </summary>
        public virtual async Task<Empleado?> ObtenerPorTokenActivacionAsync(
            string token)
        {
            string query = $@"
                SELECT {Columnas}
                FROM empleado
                WHERE TokenActivacion = @token
                LIMIT 1";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@token", token);

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            return await reader.ReadAsync()
                ? MapearEmpleado(reader)
                : null;
        }

        /// <summary>
        /// Empleados pendientes de activación (Estado -1). Se usan para
        /// listarlos y permitir reenviarles el enlace cuando el token venció.
        /// </summary>
        public virtual async Task<List<Empleado>> ObtenerPendientesActivacionAsync()
        {
            List<Empleado> empleados = new();

            string query = $@"
                SELECT {Columnas}
                FROM empleado
                WHERE Estado = @estado
                ORDER BY FechaRegistro DESC";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@estado",
                FERRETERIA__Joel.Dominio.Constantes.EstadoEmpleado.PendienteActivacion);

            await connection.OpenAsync();

            using MySqlDataReader reader =
                (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                empleados.Add(MapearEmpleado(reader));
            }

            return empleados;
        }

        /// <summary>
        /// Reemplaza el token de activación de un empleado pendiente y reinicia
        /// su vigencia. Se usa al reenviar el enlace de activación.
        /// </summary>
        public virtual async Task ReasignarTokenActivacionAsync(
            int id,
            string token,
            DateTime expira)
        {
            const string query = @"
                UPDATE empleado
                SET
                    TokenActivacion = @token,
                    TokenExpira = @expira,
                    TokenUsado = 0,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdEmpleado = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@token", token);
            command.Parameters.AddWithValue("@expira", expira);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Activa la cuenta: guarda la contraseña definida por el empleado,
        /// marca el token como usado y pasa el Estado a activo (1).
        /// </summary>
        public virtual async Task ActivarCuentaAsync(
            int id,
            string contrasena,
            sbyte estadoActivo)
        {
            const string query = @"
                UPDATE empleado
                SET
                    Contrasena = @contrasena,
                    TokenUsado = 1,
                    Estado = @estado,
                    FechaActualizacion = CURRENT_TIMESTAMP
                WHERE IdEmpleado = @id";

            using MySqlConnection connection =
                _connectionFactory.CreateConnection();

            using MySqlCommand command = new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@contrasena", contrasena);
            command.Parameters.AddWithValue("@estado", estadoActivo);

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        private static Empleado MapearEmpleado(MySqlDataReader reader)
        {
            return new Empleado
            {
                IdEmpleado = reader.GetInt32("IdEmpleado"),
                Nombre = reader["Nombre"].ToString() ?? "",
                Apellido = reader["Apellido"].ToString() ?? "",
                Rol = reader["Rol"].ToString() ?? "",
                Telefono = reader["Telefono"].ToString() ?? "",
                CorreoElectronico = reader["CorreoElectronico"] == DBNull.Value
                    ? null
                    : reader["CorreoElectronico"].ToString(),
                Contrasena = reader["Contrasena"] == DBNull.Value
                    ? null
                    : reader["Contrasena"].ToString(),
                TokenActivacion = reader["TokenActivacion"] == DBNull.Value
                    ? null
                    : reader["TokenActivacion"].ToString(),
                TokenExpira = reader["TokenExpira"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("TokenExpira"),
                TokenUsado = Convert.ToBoolean(reader["TokenUsado"]),
                Estado = reader.GetSByte("Estado"),
                FechaRegistro = reader.GetDateTime("FechaRegistro"),
                FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value
                    ? null
                    : reader.GetDateTime("FechaActualizacion"),
                IdEmpleadoResponsable = reader["IdEmpleadoResponsable"] == DBNull.Value
                    ? null
                    : reader.GetInt32("IdEmpleadoResponsable")
            };
        }
    }
}
