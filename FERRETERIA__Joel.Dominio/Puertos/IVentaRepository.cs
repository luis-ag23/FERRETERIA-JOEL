using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Persistencia de la cabecera de venta. Todas las operaciones son
    /// asíncronas y se ejecutan sobre la conexión/transacción compartida
    /// del request.
    /// </summary>
    public interface IVentaRepository
    {
        Task<List<Venta>> ObtenerTodosAsync();

        Task<int> InsertarAsync(Venta venta);

        /// <summary>
        /// Asigna el CodigoVenta definitivo a partir del IdVenta
        /// autoincremental. Debe ejecutarse dentro de la misma
        /// transacción del INSERT de la cabecera.
        /// </summary>
        Task AsignarCodigoAsync(int idVenta);

        Task<Venta?> ObtenerPorIdAsync(int idVenta);

        Task<Venta?> ObtenerPorClaveIdempotenciaAsync(string claveIdempotencia);

        /// <summary>
        /// Marca la venta como anulada solo si está 'Registrada'.
        /// Devuelve true únicamente cuando afectó una fila.
        /// </summary>
        Task<bool> MarcarAnuladaAsync(
            int idVenta,
            int idEmpleadoAnulacion,
            string motivo);
    }
}
