using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ProductoRepositoryCreator : RepositoryCreator<IGestionarRepository<Producto>>
    {
        private readonly IConexionTransaccional _conexion;
        private readonly ICacheSeleccion? _cache;

        public ProductoRepositoryCreator(IConexionTransaccional conexion, ICacheSeleccion? cache = null)
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
