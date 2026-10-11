using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class VentaRepositoryCreator : RepositoryCreator<IVentaRepository>
    {
        private readonly IConexionTransaccional _conexion;

        public VentaRepositoryCreator(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        public override IVentaRepository CreateRepository()
        {
            return new MySqlVentaRepository(_conexion);
        }
    }
}
