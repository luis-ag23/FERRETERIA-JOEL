using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Operaciones de stock que participan en la transacción de venta.
    /// El descuento valida disponibilidad en la misma sentencia SQL.
    /// </summary>
    public interface IInventarioRepository
    {
        Producto? ObtenerParaVenta(int idProducto);

        /// <summary>
        /// Descuenta stock solo si hay existencias suficientes.
        /// Devuelve true cuando se descontó; false si no alcanzaba.
        /// </summary>
        bool Descontar(int idProducto, int cantidad);

        void Reponer(int idProducto, int cantidad);
    }
}
