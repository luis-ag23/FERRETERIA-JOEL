using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Conexion
{
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public MySqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("MySqlConnection")
                ?? throw new InvalidOperationException(
                    "No se encontró la cadena de conexión MySqlConnection.");
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}