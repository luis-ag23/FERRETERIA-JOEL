using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class HistoricoPrecioRepositoryCreator : RepositoryCreator<IRepository<HistoricoPrecio>>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public HistoricoPrecioRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IRepository<HistoricoPrecio> CreateRepository()
        {
            return new MySqlHistoricoPrecioRepository(_connectionFactory);
        }
    }
}
