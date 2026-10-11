using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Repositorios
{
    /// <summary>
    /// Lectura asíncrona de empleados para el comprobante de venta.
    /// Es una dependencia propia de ventas y no toca el repositorio de
    /// empleados que usa el CRUD de otros módulos.
    /// </summary>
    public class MySqlEmpleadoVentaRepository : IEmpleadoVentaRepository
    {
        private readonly IConexionTransaccional _conexion;

        public MySqlEmpleadoVentaRepository(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        public async Task<List<Empleado>> ListarAsync()
        {
            List<Empleado> empleados = new();

            const string query = @"
                SELECT
                    IdEmpleado,
                    Nombre,
                    Apellido,
                    Rol,
                    Telefono,
                    CorreoElectronico,
                    Estado,
                    FechaRegistro,
                    FechaActualizacion,
                    IdEmpleadoResponsable
                FROM empleado
                ORDER BY Nombre ASC";

            MySqlConnection conexion = await _conexion.ObtenerConexionAsync();

            using MySqlCommand command = new(query, conexion);

            if (_conexion.Transaccion is not null)
            {
                command.Transaction = _conexion.Transaccion;
            }

            using MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                empleados.Add(new Empleado
                {
                    IdEmpleado = reader.GetInt32("IdEmpleado"),
                    Nombre = reader["Nombre"].ToString() ?? "",
                    Apellido = reader["Apellido"].ToString() ?? "",
                    Rol = reader["Rol"].ToString() ?? "",
                    Telefono = reader["Telefono"].ToString() ?? "",
                    CorreoElectronico = reader["CorreoElectronico"] == DBNull.Value
                        ? null
                        : reader["CorreoElectronico"].ToString(),
                    Estado = reader.GetByte("Estado"),
                    FechaRegistro = reader.GetDateTime("FechaRegistro"),
                    FechaActualizacion = reader["FechaActualizacion"] == DBNull.Value
                        ? null
                        : reader.GetDateTime("FechaActualizacion"),
                    IdEmpleadoResponsable = reader["IdEmpleadoResponsable"] == DBNull.Value
                        ? null
                        : reader.GetInt32("IdEmpleadoResponsable")
                });
            }

            return empleados;
        }
    }
}
