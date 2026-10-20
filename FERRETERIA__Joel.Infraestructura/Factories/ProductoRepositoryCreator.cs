using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;
using FERRETERIA__Joel.Infraestructura.Servicios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ProductoRepositoryCreator : RepositoryCreator<IGestionarRepository<Producto>>
    {
        private readonly IConexionTransaccional _conexion;
        private readonly RedisCacheHelper? _cache;

        public ProductoRepositoryCreator(IConexionTransaccional conexion, RedisCacheHelper? cache = null)
        {
            _conexion = conexion;
            _cache = cache;
        }

        public override IGestionarRepository<Producto> CreateRepository()
        {
            return new MySqlProductoRepository(_conexion, _cache);
        }
    }
}
