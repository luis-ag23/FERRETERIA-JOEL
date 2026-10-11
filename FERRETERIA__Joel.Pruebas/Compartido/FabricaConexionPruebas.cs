using FERRETERIA__Joel.Infraestructura.Conexion;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Pruebas.Compartido
{
    /// <summary>
    /// Fábrica de conexiones para las pruebas de integración. Evita
    /// depender de IConfiguration usando directamente la cadena de
    /// conexión provista por el entorno de pruebas.
    /// </summary>
    public sealed class FabricaConexionPruebas : IDbConnectionFactory
    {
        private readonly string _cadenaConexion;

        public FabricaConexionPruebas(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_cadenaConexion);
        }
    }
}
