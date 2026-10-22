using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class CategoriaRepositoryCreator : RepositoryCreator<IGestionarRepository<Categoria>>
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ICacheSeleccion? _cache;

        public CategoriaRepositoryCreator(IDbConnectionFactory connectionFactory, ICacheSeleccion? cache = null)
        {
            _connectionFactory = connectionFactory;
            _cache = cache;
        }

        public override IGestionarRepository<Categoria> CreateRepository()
        {
            return new MySqlCategoriaRepository(_connectionFactory, _cache);
        }
    }
}
