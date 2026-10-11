using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioProducto
    {
        private readonly RepositoryCreator<IGestionarRepository<Producto>> _creador;
        private readonly ServicioHistoricoPrecio _historicoPrecio;
        private IGestionarRepository<Producto>? _repositorio;

        public ServicioProducto(
            RepositoryCreator<IGestionarRepository<Producto>> creador,
            ServicioHistoricoPrecio historicoPrecio)
        {
            _creador = creador;
            _historicoPrecio = historicoPrecio;
        }

        private IGestionarRepository<Producto> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public List<Producto> ObtenerTodos()
        {
            return Repositorio.ObtenerTodos();
        }

        public List<Producto> ObtenerActivas()
        {
            return Repositorio.MostrarActivos();
        }

        public List<Producto> Listar(bool soloActivos)
        {
            return soloActivos
                ? ObtenerActivas()
                : ObtenerTodos();
        }

        public Producto? ObtenerPorId(int id)
        {
            return Repositorio
                .ObtenerTodos()
                .FirstOrDefault(p => p.IdProducto == id);
        }

        public string SiguienteCodigo()
        {
            return $"PROD-{Repositorio.ObtenerTodos().Count + 1:D3}";
        }

        public int Crear(Producto producto)
        {
            int idProducto = Repositorio.Insertar(producto);

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

            Repositorio.Actualizar(producto);

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
            Repositorio.CambiarEstado(idProducto);
        }
    }
}
