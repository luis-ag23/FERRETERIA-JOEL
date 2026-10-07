using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class CategoriaRepositoryCreator : RepositoryCreator<IRepository<Categoria>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CategoriaRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IRepository<Categoria> CreateRepository()
        {
            return new MySqlCategoriaRepository(_connectionFactory);
        }
    }
}