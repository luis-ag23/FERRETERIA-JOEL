using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Caso de uso de productos. El alta y, sobre todo, la actualización de
    /// precio se ejecutan dentro de una unidad de trabajo: el producto, el
    /// cierre del precio vigente y el nuevo registro del histórico se
    /// confirman o revierten juntos.
    /// </summary>
    public class ServicioProducto
    {
        private readonly RepositoryCreator<IGestionarRepository<Producto>> _creador;
        private readonly ServicioHistoricoPrecio _historicoPrecio;
        private readonly IUnidadDeTrabajo _unidadDeTrabajo;
        private IGestionarRepository<Producto>? _repositorio;

        public ServicioProducto(
            RepositoryCreator<IGestionarRepository<Producto>> creador,
            ServicioHistoricoPrecio historicoPrecio,
            IUnidadDeTrabajo unidadDeTrabajo)
        {
            _creador = creador;
            _historicoPrecio = historicoPrecio;
            _unidadDeTrabajo = unidadDeTrabajo;
        }

        private IGestionarRepository<Producto> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            return await Repositorio.ObtenerTodosAsync();
        }

        public async Task<List<Producto>> ObtenerActivasAsync()
        {
            return await Repositorio.MostrarActivosAsync();
        }

        public async Task<List<Producto>> ListarAsync(bool soloActivos)
        {
            return soloActivos
                ? await ObtenerActivasAsync()
                : await ObtenerTodosAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            List<Producto> productos = await Repositorio.ObtenerTodosAsync();

            return productos.FirstOrDefault(p => p.IdProducto == id);
        }

        public async Task<string> SiguienteCodigoAsync()
        {
            List<Producto> productos = await Repositorio.ObtenerTodosAsync();

            return $"PROD-{productos.Count + 1:D3}";
        }

        public async Task<int> CrearAsync(Producto producto)
        {
            await _unidadDeTrabajo.IniciarAsync();

            try
            {
                int idProducto = await Repositorio.InsertarAsync(producto);

                HistoricoPrecio historicoPrecio = new()
                {
                    IdProducto = idProducto,
                    Precio = producto.PrecioVenta,
                    MotivoCambio = "Precio inicial",
                    IdEmpleadoResponsable = producto.IdEmpleadoResponsable
                };

                await _historicoPrecio.RegistrarAsync(historicoPrecio);

                await _unidadDeTrabajo.ConfirmarAsync();

                return idProducto;
            }
            catch
            {
                await _unidadDeTrabajo.RevertirAsync();
                throw;
            }
        }

        public async Task ActualizarAsync(Producto producto)
        {
            await _unidadDeTrabajo.IniciarAsync();

            try
            {
                HistoricoPrecio? precioVigente =
                    await _historicoPrecio.ObtenerPrecioVigenteAsync(
                        producto.IdProducto);

                await Repositorio.ActualizarAsync(producto);

                bool cambioPrecio =
                    precioVigente is null ||
                    precioVigente.Precio != producto.PrecioVenta;

                if (!cambioPrecio)
                {
                    await _unidadDeTrabajo.ConfirmarAsync();
                    return;
                }

                if (precioVigente is not null)
                {
                    await _historicoPrecio.CerrarPrecioVigenteAsync(
                        producto.IdProducto);
                }

                HistoricoPrecio nuevoHistorico = new()
                {
                    IdProducto = producto.IdProducto,
                    Precio = producto.PrecioVenta,
                    MotivoCambio = "Cambio de precio",
                    IdEmpleadoResponsable = producto.IdEmpleadoResponsable
                };

                await _historicoPrecio.RegistrarAsync(nuevoHistorico);

                await _unidadDeTrabajo.ConfirmarAsync();
            }
            catch
            {
                await _unidadDeTrabajo.RevertirAsync();
                throw;
            }
        }

        public async Task CambiarEstadoAsync(
            int idProducto,
            int idEmpleadoResponsable)
        {
            await Repositorio.CambiarEstadoAsync(
                idProducto,
                idEmpleadoResponsable);
        }
    }
}
