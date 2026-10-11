using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class SesionRepositoryCreator : RepositoryCreator<ISesionRepository>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SesionRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override ISesionRepository CreateRepository()
        {
            return new MySqlSesionRepository(_connectionFactory);
        }
    }
}
