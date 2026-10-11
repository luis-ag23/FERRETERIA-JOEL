using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Pruebas.Compartido
{
    /// <summary>
    /// Creador genérico (patrón Factory Method) que devuelve siempre la
    /// misma instancia de repositorio falso.
    /// </summary>
    public sealed class CreadorFake<T> : RepositoryCreator<T>
    {
        private readonly T _repositorio;

        public CreadorFake(T repositorio)
        {
            _repositorio = repositorio;
        }

        public override T CreateRepository()
        {
            return _repositorio;
        }
    }

    public sealed class UnidadDeTrabajoFake : IUnidadDeTrabajo
    {
        public int VecesIniciar { get; private set; }

        public int VecesConfirmar { get; private set; }

        public int VecesRevertir { get; private set; }

        public bool Activa { get; private set; }

        public void Iniciar()
        {
            VecesIniciar++;
            Activa = true;
        }

        public void Confirmar()
        {
            VecesConfirmar++;
            Activa = false;
        }

        public void Revertir()
        {
            VecesRevertir++;
            Activa = false;
        }
    }

    public sealed class ContextoUsuarioFake : IContextoUsuario
    {
        public ContextoUsuarioFake(int idEmpleado = 42)
        {
            IdEmpleado = idEmpleado;
        }

        public int IdEmpleado { get; }

        public string NombreCompleto => "Usuario de prueba";

        public string Rol => RolesSistema.Vendedor;
    }

    public sealed class ProductoRepositorioFake : IGestionarRepository<Producto>
    {
        private readonly List<Producto> _productos = new();

        public void Agregar(Producto producto)
        {
            _productos.Add(producto);
        }

        public List<Producto> ObtenerTodos()
        {
            return _productos.ToList();
        }

        public List<Producto> MostrarActivos()
        {
            return _productos.Where(p => p.Estado == 1).ToList();
        }

        public int Insertar(Producto entidad)
        {
            entidad.IdProducto = _productos.Count + 1;
            _productos.Add(entidad);
            return entidad.IdProducto;
        }

        public void Actualizar(Producto entidad)
        {
            int indice = _productos.FindIndex(p => p.IdProducto == entidad.IdProducto);

            if (indice >= 0)
            {
                _productos[indice] = entidad;
            }
        }

        public void CambiarEstado(int id)
        {
            Producto? producto = _productos.FirstOrDefault(p => p.IdProducto == id);

            if (producto is not null)
            {
                producto.Estado = (byte)(producto.Estado == 1 ? 0 : 1);
            }
        }
    }

    public sealed class ClienteRepositorioFake : IClienteRepository
    {
        private readonly List<Cliente> _clientes = new();

        public void Agregar(Cliente cliente)
        {
            cliente.IdCliente = _clientes.Count + 1;
            _clientes.Add(cliente);
        }

        public List<Cliente> ObtenerTodos()
        {
            return _clientes.ToList();
        }

        public int Insertar(Cliente entidad)
        {
            entidad.IdCliente = _clientes.Count + 1;
            _clientes.Add(entidad);
            return entidad.IdCliente;
        }

        public Cliente? ObtenerPorId(int idCliente)
        {
            return _clientes.FirstOrDefault(c => c.IdCliente == idCliente);
        }

        public List<Cliente> Buscar(string termino)
        {
            return _clientes
                .Where(c => c.Estado == 1 &&
                    (c.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                     c.Apellido.Contains(termino, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }

    public sealed class VentaRepositorioFake : IVentaRepository
    {
        private readonly List<Venta> _ventas = new();
        private int _secuencia;

        public int VecesInsertar { get; private set; }

        public List<Venta> ObtenerTodos()
        {
            return _ventas.ToList();
        }

        public int Insertar(Venta entidad)
        {
            entidad.IdVenta = ++_secuencia;
            entidad.FechaRegistro = DateTime.Now;
            _ventas.Add(entidad);
            VecesInsertar++;
            return entidad.IdVenta;
        }

        public void AsignarCodigo(int idVenta)
        {
            Venta? venta = _ventas.FirstOrDefault(v => v.IdVenta == idVenta);

            if (venta is not null)
            {
                venta.CodigoVenta = $"V-TEST-{idVenta:D6}";
            }
        }

        public Venta? ObtenerPorId(int idVenta)
        {
            return _ventas.FirstOrDefault(v => v.IdVenta == idVenta);
        }

        public Venta? ObtenerPorClaveIdempotencia(string claveIdempotencia)
        {
            return _ventas.FirstOrDefault(v =>
                v.ClaveIdempotencia == claveIdempotencia);
        }

        public bool MarcarAnulada(int idVenta, int idEmpleadoAnulacion, string motivo)
        {
            Venta? venta = _ventas.FirstOrDefault(v => v.IdVenta == idVenta);

            if (venta is null ||
                venta.EstadoVenta != EstadoVenta.Registrada)
            {
                return false;
            }

            venta.EstadoVenta = EstadoVenta.Anulada;
            venta.MotivoAnulacion = motivo;
            venta.IdEmpleadoAnulacion = idEmpleadoAnulacion;
            venta.FechaAnulacion = DateTime.Now;

            return true;
        }
    }

    public sealed class DetalleVentaRepositorioFake : IDetalleVentaRepository
    {
        private readonly List<DetalleVenta> _detalles = new();
        private int _secuencia;

        public List<DetalleVenta> ObtenerTodos()
        {
            return _detalles.ToList();
        }

        public List<DetalleVenta> ObtenerPorVenta(int idVenta)
        {
            return _detalles.Where(d => d.IdVenta == idVenta).ToList();
        }

        public int Insertar(DetalleVenta entidad)
        {
            entidad.IdDetalleVenta = ++_secuencia;
            _detalles.Add(entidad);
            return entidad.IdDetalleVenta;
        }
    }

    public sealed class EmpleadoRepositorioFake : IRepository<Empleado>
    {
        private readonly List<Empleado> _empleados = new();

        public void Agregar(Empleado empleado)
        {
            _empleados.Add(empleado);
        }

        public List<Empleado> ObtenerTodos()
        {
            return _empleados.ToList();
        }

        public int Insertar(Empleado entidad)
        {
            entidad.IdEmpleado = _empleados.Count + 1;
            _empleados.Add(entidad);
            return entidad.IdEmpleado;
        }
    }

    public sealed class InventarioRepositorioFake : IInventarioRepository
    {
        private readonly Dictionary<int, Producto> _productos = new();

        public void Agregar(Producto producto)
        {
            _productos[producto.IdProducto] = producto;
        }

        public Producto? ObtenerParaVenta(int idProducto)
        {
            return _productos.TryGetValue(idProducto, out Producto? producto) &&
                   producto.Estado == 1
                ? producto
                : null;
        }

        public bool Descontar(int idProducto, int cantidad)
        {
            if (!_productos.TryGetValue(idProducto, out Producto? producto) ||
                producto.Estado != 1 ||
                producto.CantidadStock < cantidad)
            {
                return false;
            }

            producto.CantidadStock -= cantidad;
            return true;
        }

        public void Reponer(int idProducto, int cantidad)
        {
            if (_productos.TryGetValue(idProducto, out Producto? producto))
            {
                producto.CantidadStock += cantidad;
            }
        }

        public int Stock(int idProducto)
        {
            return _productos.TryGetValue(idProducto, out Producto? producto)
                ? producto.CantidadStock
                : 0;
        }
    }
}
