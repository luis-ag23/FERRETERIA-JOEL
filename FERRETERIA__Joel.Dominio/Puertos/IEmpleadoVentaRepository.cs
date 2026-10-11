using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Lectura de empleados requerida por la venta para resolver el
    /// nombre del responsable y del anulador en el comprobante.
    /// </summary>
    public interface IEmpleadoVentaRepository
    {
        Task<List<Empleado>> ListarAsync();
    }
}
