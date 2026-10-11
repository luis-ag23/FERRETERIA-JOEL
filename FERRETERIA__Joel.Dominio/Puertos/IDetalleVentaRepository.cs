using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IDetalleVentaRepository
    {
        Task<List<DetalleVenta>> ObtenerPorVentaAsync(int idVenta);

        Task<int> InsertarAsync(DetalleVenta detalleVenta);
    }
}
