using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public class HistoricoPrecioRepositoryCreator : RepositoryCreator<IHistoricoPrecioRepository>
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public HistoricoPrecioRepositoryCreator(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public override IHistoricoPrecioRepository CreateRepository()
        {
            return new MySqlHistoricoPrecioRepository(_connectionFactory);
        }
    }
}
