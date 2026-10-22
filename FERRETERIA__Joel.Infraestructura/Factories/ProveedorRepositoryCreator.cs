using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ProveedorRepositoryCreator : RepositoryCreator<IGestionarRepository<Proveedor>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ICacheSeleccion? _cache;

        public ProveedorRepositoryCreator(IDbConnectionFactory connectionFactory, ICacheSeleccion? cache = null)
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
