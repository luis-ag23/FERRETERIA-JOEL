using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class DetalleVentaRepositoryCreator
        : RepositoryCreator<IDetalleVentaRepository>
    {
        private readonly IConexionTransaccional _conexion;

        public DetalleVentaRepositoryCreator(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        public override IDetalleVentaRepository CreateRepository()
        {
            return new MySqlDetalleVentaRepository(_conexion);
        }
    }
}
