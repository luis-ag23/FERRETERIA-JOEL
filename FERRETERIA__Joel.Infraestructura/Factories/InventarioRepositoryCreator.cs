using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class InventarioRepositoryCreator : RepositoryCreator<IInventarioRepository>
    {
        private readonly IConexionTransaccional _conexion;

        public InventarioRepositoryCreator(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        public override IInventarioRepository CreateRepository()
        {
            return new MySqlInventarioRepository(_conexion);
        }
    }
}
