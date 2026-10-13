using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Caso de uso del histórico de precios. Depende del repositorio concreto
    /// (operación especial: precio vigente y cierre de vigencia) que comparte
    /// la transacción ambiente con el repositorio de productos.
    /// </summary>
    public class ServicioHistoricoPrecio
    {
        private readonly MySqlHistoricoPrecioRepository _repositorio;

        public ServicioHistoricoPrecio(
            MySqlHistoricoPrecioRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<HistoricoPrecio>> ObtenerPorProductoAsync(
            int idProducto)
        {
            return await _repositorio.ObtenerPorProductoAsync(idProducto);
        }

        public async Task<HistoricoPrecio?> ObtenerPrecioVigenteAsync(
            int idProducto)
        {
            return await _repositorio.ObtenerPrecioVigenteAsync(idProducto);
        }

        public async Task CerrarPrecioVigenteAsync(int idProducto)
        {
            await _repositorio.CerrarPrecioVigenteAsync(idProducto);
        }

        public async Task<int> RegistrarAsync(
            HistoricoPrecio historicoPrecio)
        {
            return await _repositorio.InsertarAsync(historicoPrecio);
        }
    }
}
