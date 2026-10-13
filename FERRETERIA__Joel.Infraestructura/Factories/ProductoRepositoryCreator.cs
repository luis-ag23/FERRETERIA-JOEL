using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ProductoRepositoryCreator : RepositoryCreator<IGestionarRepository<Producto>>
    {
        private readonly IConexionTransaccional _conexion;

        public ProductoRepositoryCreator(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        public override IGestionarRepository<Producto> CreateRepository()
        {
            return new MySqlProductoRepository(_conexion);
        }
    }
}
