using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ProductoRepositoryCreator : RepositoryCreator<IRepository<Producto>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProductoRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IRepository<Producto> CreateRepository()
        {
            return new MySqlProductoRepository(_connectionFactory);
        }
    }
}