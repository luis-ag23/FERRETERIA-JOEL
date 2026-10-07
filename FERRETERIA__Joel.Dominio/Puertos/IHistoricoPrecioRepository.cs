using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IHistoricoPrecioRepository : IRepository<HistoricoPrecio>
    {
        List<HistoricoPrecio> ObtenerPorProducto(int idProducto);

        HistoricoPrecio? ObtenerPrecioVigente(int idProducto);

        void CerrarPrecioVigente(int idProducto);
    }
}
