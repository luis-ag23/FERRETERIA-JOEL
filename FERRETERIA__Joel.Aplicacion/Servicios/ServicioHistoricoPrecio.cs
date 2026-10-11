using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioHistoricoPrecio
    {
        private readonly RepositoryCreator<IHistoricoPrecioRepository> _creador;
        private IHistoricoPrecioRepository? _repositorio;

        public ServicioHistoricoPrecio(
            RepositoryCreator<IHistoricoPrecioRepository> creador)
        {
            _creador = creador;
        }

        private IHistoricoPrecioRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public List<HistoricoPrecio> ObtenerPorProducto(int idProducto)
        {
            return Repositorio.ObtenerPorProducto(idProducto);
        }

        public HistoricoPrecio? ObtenerPrecioVigente(int idProducto)
        {
            return Repositorio.ObtenerPrecioVigente(idProducto);
        }

        public void CerrarPrecioVigente(int idProducto)
        {
            Repositorio.CerrarPrecioVigente(idProducto);
        }

        public int Registrar(HistoricoPrecio historicoPrecio)
        {
            return Repositorio.Insertar(historicoPrecio);
        }
    }
}
