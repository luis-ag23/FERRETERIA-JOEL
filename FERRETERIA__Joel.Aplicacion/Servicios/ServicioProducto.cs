using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioProducto
    {
        private readonly IGestionarRepository<Producto> _repositorio;
        private readonly ServicioHistoricoPrecio _historicoPrecio;

        public ServicioProducto(
            IGestionarRepository<Producto> repositorio,
            ServicioHistoricoPrecio historicoPrecio)
        {
            _repositorio = repositorio;
            _historicoPrecio = historicoPrecio;
        }

        public List<Producto> ObtenerTodos()
        {
            return _repositorio.ObtenerTodos();
        }

        public List<Producto> ObtenerActivas()
        {
            return _repositorio.MostrarActivos();
        }

        public List<Producto> Listar(bool soloActivos)
        {
            return soloActivos
                ? ObtenerActivas()
                : ObtenerTodos();
        }

        public Producto? ObtenerPorId(int id)
        {
            return _repositorio
                .ObtenerTodos()
                .FirstOrDefault(p => p.IdProducto == id);
        }

        public string SiguienteCodigo()
        {
            return $"PROD-{_repositorio.ObtenerTodos().Count + 1:D3}";
        }

        public int Crear(Producto producto)
        {
            int idProducto = _repositorio.Insertar(producto);

            HistoricoPrecio historicoPrecio = new()
            {
                IdProducto = idProducto,
                Precio = producto.PrecioVenta,
                MotivoCambio = "Precio inicial",
                IdEmpleadoResponsable = producto.IdEmpleadoResponsable
            };

            _historicoPrecio.Registrar(historicoPrecio);

            return idProducto;
        }

        public void Actualizar(Producto producto)
        {
            HistoricoPrecio? precioVigente =
                _historicoPrecio.ObtenerPrecioVigente(
                    producto.IdProducto);

            _repositorio.Actualizar(producto);

            bool cambioPrecio =
                precioVigente is null ||
                precioVigente.Precio != producto.PrecioVenta;

            if (!cambioPrecio)
            {
                return;
            }

            if (precioVigente is not null)
            {
                _historicoPrecio.CerrarPrecioVigente(
                    producto.IdProducto);
            }

            HistoricoPrecio nuevoHistorico = new()
            {
                IdProducto = producto.IdProducto,
                Precio = producto.PrecioVenta,
                MotivoCambio = "Cambio de precio",
                IdEmpleadoResponsable = producto.IdEmpleadoResponsable
            };

            _historicoPrecio.Registrar(nuevoHistorico);
        }

        public void CambiarEstado(int idProducto)
        {
            _repositorio.CambiarEstado(idProducto);
        }
    }
}
