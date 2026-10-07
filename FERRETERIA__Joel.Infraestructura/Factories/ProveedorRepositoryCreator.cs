using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ProveedorRepositoryCreator : RepositoryCreator<IRepository<Proveedor>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ProveedorRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IRepository<Proveedor> CreateRepository()
        {
            return new MySqlProveedorRepository(_connectionFactory);
        }
    }
}