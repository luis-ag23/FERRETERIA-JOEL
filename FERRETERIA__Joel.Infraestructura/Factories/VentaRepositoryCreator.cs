using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class VentaRepositoryCreator : RepositoryCreator<IRepository<Venta>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public VentaRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IRepository<Venta> CreateRepository()
        {
            return new MySqlVentaRepository(_connectionFactory);
        }
    }
}
