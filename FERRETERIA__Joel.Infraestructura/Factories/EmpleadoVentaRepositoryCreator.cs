using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class EmpleadoVentaRepositoryCreator
        : RepositoryCreator<IEmpleadoVentaRepository>
    {
        private readonly IConexionTransaccional _conexion;

        public EmpleadoVentaRepositoryCreator(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        public override IEmpleadoVentaRepository CreateRepository()
        {
            return new MySqlEmpleadoVentaRepository(_conexion);
        }
    }
}
