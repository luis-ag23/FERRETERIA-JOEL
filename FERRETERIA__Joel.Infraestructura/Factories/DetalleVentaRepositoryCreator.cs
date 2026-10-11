using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class DetalleVentaRepositoryCreator : RepositoryCreator<IRepository<DetalleVenta>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DetalleVentaRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IRepository<DetalleVenta> CreateRepository()
        {
            return new MySqlDetalleVentaRepository(_connectionFactory);
        }
    }
}
