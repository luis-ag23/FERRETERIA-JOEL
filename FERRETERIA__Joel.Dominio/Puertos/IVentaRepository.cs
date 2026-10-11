using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IVentaRepository : IRepository<Venta>
    {
        /// <summary>
        /// Asigna el CodigoVenta definitivo a partir del IdVenta
        /// autoincremental. Debe ejecutarse dentro de la misma
        /// transacción del INSERT de la cabecera.
        /// </summary>
        void AsignarCodigo(int idVenta);

        Venta? ObtenerPorId(int idVenta);

        Venta? ObtenerPorClaveIdempotencia(string claveIdempotencia);

        /// <summary>
        /// Marca la venta como anulada solo si está 'Registrada'.
        /// Devuelve true únicamente cuando afectó una fila.
        /// </summary>
        bool MarcarAnulada(
            int idVenta,
            int idEmpleadoAnulacion,
            string motivo);
    }
}
