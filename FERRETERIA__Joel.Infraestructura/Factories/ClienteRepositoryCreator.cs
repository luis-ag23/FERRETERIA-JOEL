using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class ClienteRepositoryCreator : RepositoryCreator<IClienteRepository>
    {
        private readonly IConexionTransaccional _conexion;

        public ClienteRepositoryCreator(IConexionTransaccional conexion)
        {
            _conexion = conexion;
        }

        public override IClienteRepository CreateRepository()
        {
            return new MySqlClienteRepository(_conexion);
        }
    }
}
