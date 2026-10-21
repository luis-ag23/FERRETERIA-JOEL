using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;
using FERRETERIA__Joel.Infraestructura.Servicios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class MarcaRepositoryCreator : RepositoryCreator<IGestionarRepository<Marca>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly RedisCacheHelper? _cache;

        public MarcaRepositoryCreator(IDbConnectionFactory connectionFactory, RedisCacheHelper? cache = null)
        {
            _connectionFactory = connectionFactory;
            _cache = cache;
        }

        public override IGestionarRepository<Marca> CreateRepository()
        {
            return new MySqlMarcaRepository(_connectionFactory, _cache);
        }
    }
}
