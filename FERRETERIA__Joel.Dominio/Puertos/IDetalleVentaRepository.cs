using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IDetalleVentaRepository : IRepository<DetalleVenta>
    {
        List<DetalleVenta> ObtenerPorVenta(int idVenta);
    }
}
