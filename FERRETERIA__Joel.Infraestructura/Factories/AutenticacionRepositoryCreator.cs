using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class AutenticacionRepositoryCreator
        : RepositoryCreator<IAutenticacionRepository>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AutenticacionRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IAutenticacionRepository CreateRepository()
        {
            return new MySqlAutenticacionRepository(_connectionFactory);
        }
    }
}
