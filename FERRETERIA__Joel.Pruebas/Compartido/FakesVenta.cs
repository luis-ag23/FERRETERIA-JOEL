using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Repositorios;

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

        public Task IniciarAsync()
        {
            VecesIniciar++;
            Activa = true;
            return Task.CompletedTask;
        }

        public Task ConfirmarAsync()
        {
            VecesConfirmar++;
            Activa = false;
            return Task.CompletedTask;
        }

        public Task RevertirAsync()
        {
            VecesRevertir++;
            Activa = false;
            return Task.CompletedTask;
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
        public string Rol => RolesSistema.EmpleadoVenta;
    }

    /// <summary>
    /// Repositorio falso de clientes. Hereda de la implementación real y
    /// sustituye (métodos virtuales) el acceso a la base de datos.
    /// </summary>
    public sealed class ClienteRepositorioFake : MySqlClienteRepository
    {
        private readonly List<Cliente> _clientes = new();

        public ClienteRepositorioFake() : base(null!)
        {
        }

        public void Agregar(Cliente cliente)
        {
            cliente.IdCliente = _clientes.Count + 1;
            _clientes.Add(cliente);
        }

                public override Task<bool> ExisteAsync(
            string nombre,
            string apellido,
            string telefono)
        {
            return Task.FromResult(_clientes.Any(c =>
                c.Estado == 1
                && string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase)
                && string.Equals(c.Apellido, apellido, StringComparison.OrdinalIgnoreCase)
                && c.Telefono == telefono));
        }

        public override Task<List<Cliente>> ObtenerTodosAsync()
        {
            return Task.FromResult(_clientes.ToList());
        }

        public override Task<int> InsertarAsync(Cliente entidad)
        {
            entidad.IdCliente = _clientes.Count + 1;
            _clientes.Add(entidad);
            return Task.FromResult(entidad.IdCliente);
        }

        public override Task<Cliente?> ObtenerPorIdAsync(int idCliente)
        {
            return Task.FromResult(
                _clientes.FirstOrDefault(c => c.IdCliente == idCliente));
        }

        public override Task<List<Cliente>> BuscarAsync(string termino)
        {
            List<Cliente> resultado = _clientes
                .Where(c => c.Estado == 1 &&
                    (c.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                     c.Apellido.Contains(termino, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return Task.FromResult(resultado);
        }
    }

    public sealed class VentaRepositorioFake : MySqlVentaRepository
    {
        private readonly List<Venta> _ventas = new();
        private int _secuencia;

        public VentaRepositorioFake() : base(null!)
        {
        }

        public int VecesInsertar { get; private set; }

        public override Task<List<Venta>> ObtenerTodosAsync()
        {
            return Task.FromResult(_ventas.ToList());
        }

        public override Task<int> InsertarAsync(Venta entidad)
        {
            entidad.IdVenta = ++_secuencia;
            entidad.FechaRegistro = DateTime.Now;
            _ventas.Add(entidad);
            VecesInsertar++;
            return Task.FromResult(entidad.IdVenta);
        }

        public override Task AsignarCodigoAsync(int idVenta)
        {
            Venta? venta = _ventas.FirstOrDefault(v => v.IdVenta == idVenta);
            if (venta is not null)
            {
                venta.CodigoVenta = $"V-TEST-{idVenta:D6}";
            }
            return Task.CompletedTask;
        }

        public override Task<Venta?> ObtenerPorIdAsync(int idVenta)
        {
            return Task.FromResult(
                _ventas.FirstOrDefault(v => v.IdVenta == idVenta));
        }

        public override Task<Venta?> ObtenerPorClaveIdempotenciaAsync(string claveIdempotencia)
        {
            return Task.FromResult(
                _ventas.FirstOrDefault(v =>
                    v.ClaveIdempotencia == claveIdempotencia));
        }

        public override Task<bool> MarcarAnuladaAsync(
            int idVenta,
            int idEmpleadoAnulacion,
            string motivo)
        {
            Venta? venta = _ventas.FirstOrDefault(v => v.IdVenta == idVenta);

            if (venta is null ||
                venta.EstadoVenta != EstadoVenta.Registrada)
            {
                return Task.FromResult(false);
            }

            venta.EstadoVenta = EstadoVenta.Anulada;
            venta.MotivoAnulacion = motivo;
            venta.IdEmpleadoAnulacion = idEmpleadoAnulacion;
            venta.FechaAnulacion = DateTime.Now;

            return Task.FromResult(true);
        }
    }

    public sealed class DetalleVentaRepositorioFake : MySqlDetalleVentaRepository
    {
        private readonly List<DetalleVenta> _detalles = new();
        private int _secuencia;

        public DetalleVentaRepositorioFake() : base(null!)
        {
        }

        public override Task<List<DetalleVenta>> ObtenerTodosAsync()
        {
            return Task.FromResult(_detalles.ToList());
        }

        public override Task<List<DetalleVenta>> ObtenerPorVentaAsync(int idVenta)
        {
            List<DetalleVenta> resultado = _detalles
                .Where(d => d.IdVenta == idVenta)
                .ToList();
            return Task.FromResult(resultado);
        }

        public override Task<int> InsertarAsync(DetalleVenta entidad)
        {
            entidad.IdDetalleVenta = ++_secuencia;
            _detalles.Add(entidad);
            return Task.FromResult(entidad.IdDetalleVenta);
        }
    }

    public sealed class EmpleadoVentaRepositorioFake : MySqlEmpleadoRepository
    {
        private readonly List<Empleado> _empleados = new();

        public EmpleadoVentaRepositorioFake() : base(null!)
        {
        }

        public void Agregar(Empleado empleado)
        {
            _empleados.Add(empleado);
        }

        public override Task<List<Empleado>> ObtenerTodosAsync()
        {
            return Task.FromResult(_empleados.ToList());
        }
    }

    public sealed class InventarioRepositorioFake : MySqlInventarioRepository
    {
        private readonly Dictionary<int, Producto> _productos = new();

        public InventarioRepositorioFake() : base(null!)
        {
        }

        public void Agregar(Producto producto)
        {
            _productos[producto.IdProducto] = producto;
        }

        public override Task<Producto?> ObtenerParaVentaAsync(int idProducto)
        {
            Producto? resultado = _productos.TryGetValue(idProducto, out Producto? producto) &&
                                   producto.Estado == 1
                ? producto
                : null;
            return Task.FromResult(resultado);
        }

        public override Task<bool> DescontarAsync(int idProducto, int cantidad)
        {
            if (!_productos.TryGetValue(idProducto, out Producto? producto) ||
                producto.Estado != 1 ||
                producto.CantidadStock < cantidad)
            {
                return Task.FromResult(false);
            }

            producto.CantidadStock -= cantidad;
            return Task.FromResult(true);
        }

        public override Task ReponerAsync(int idProducto, int cantidad)
        {
            if (_productos.TryGetValue(idProducto, out Producto? producto))
            {
                producto.CantidadStock += cantidad;
            }
            return Task.CompletedTask;
        }

        public override Task<List<Producto>> ListarActivosAsync()
        {
            List<Producto> resultado = _productos.Values
                .Where(p => p.Estado == 1)
                .ToList();
            return Task.FromResult(resultado);
        }

        public override Task<List<Producto>> ObtenerPorIdsAsync(IReadOnlyCollection<int> ids)
        {
            HashSet<int> conjunto = new(ids);
            List<Producto> resultado = _productos.Values
                .Where(p => conjunto.Contains(p.IdProducto))
                .ToList();
            return Task.FromResult(resultado);
        }

        public int Stock(int idProducto)
        {
            return _productos.TryGetValue(idProducto, out Producto? producto)
                ? producto.CantidadStock
                : 0;
        }
    }
}
