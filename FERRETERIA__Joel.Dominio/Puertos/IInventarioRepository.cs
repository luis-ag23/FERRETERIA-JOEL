using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Operaciones de stock que participan en la transacción de venta.
    /// El descuento valida disponibilidad en la misma sentencia SQL.
    /// También expone las lecturas de catálogo que necesita la venta
    /// (productos activos y detalle de productos vendidos).
    /// </summary>
    public interface IInventarioRepository
    {
        Task<Producto?> ObtenerParaVentaAsync(int idProducto);

        /// <summary>
        /// Descuenta stock solo si hay existencias suficientes.
        /// Devuelve true cuando se descontó; false si no alcanzaba.
        /// </summary>
        Task<bool> DescontarAsync(int idProducto, int cantidad);

        Task ReponerAsync(int idProducto, int cantidad);

        Task<List<Producto>> ListarActivosAsync();

        Task<List<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids);
    }
}
