using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class MarcaRepositoryCreator : RepositoryCreator<IGestionarRepository<Marca>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ICacheSeleccion? _cache;

        public MarcaRepositoryCreator(IDbConnectionFactory connectionFactory, ICacheSeleccion? cache = null)
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
