using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ClienteRepositoryCreator : RepositoryCreator<IRepository<Cliente>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ClienteRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IRepository<Cliente> CreateRepository()
        {
            return new MySqlClienteRepository(_connectionFactory);
        }
    }
}
