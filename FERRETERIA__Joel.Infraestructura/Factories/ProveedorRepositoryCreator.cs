using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;
using FERRETERIA__Joel.Infraestructura.Servicios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ProveedorRepositoryCreator : RepositoryCreator<IGestionarRepository<Proveedor>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly RedisCacheHelper? _cache;

        public ProveedorRepositoryCreator(IDbConnectionFactory connectionFactory, RedisCacheHelper? cache = null)
        {
            _connectionFactory = connectionFactory;
            _cache = cache;
        }

        public override IGestionarRepository<Proveedor> CreateRepository()
        {
            return new MySqlProveedorRepository(_connectionFactory, _cache);
        }
    }
}
