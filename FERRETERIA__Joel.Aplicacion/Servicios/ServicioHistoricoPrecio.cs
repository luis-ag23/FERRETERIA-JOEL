using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioHistoricoPrecio
    {
        private readonly IHistoricoPrecioRepository _repositorio;

        public ServicioHistoricoPrecio(IHistoricoPrecioRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public List<HistoricoPrecio> ObtenerPorProducto(int idProducto)
        {
            return _repositorio.ObtenerPorProducto(idProducto);
        }

        public HistoricoPrecio? ObtenerPrecioVigente(int idProducto)
        {
            return _repositorio.ObtenerPrecioVigente(idProducto);
        }

        public void CerrarPrecioVigente(int idProducto)
        {
            _repositorio.CerrarPrecioVigente(idProducto);
        }

        public int Registrar(HistoricoPrecio historicoPrecio)
        {
            return _repositorio.Insertar(historicoPrecio);
        }
    }
}
