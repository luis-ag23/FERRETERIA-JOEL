using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Politicas;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Patrón Facade (Fachada) para el proceso de ventas. Presenta una
    /// única interfaz de alto nivel que coordina los subsistemas:
    /// catálogo, clientes, inventario, repositorios de venta/detalle,
    /// la unidad de trabajo (transacción), el selector de descuentos
    /// (Strategy) y la identidad del usuario autenticado.
    ///
    /// La presentación no conoce repositorios, transacciones ni
    /// políticas: solo consume esta fachada.
    /// </summary>
    public class FachadaVenta
    {
        private readonly RepositoryCreator<IGestionarRepository<Producto>> _creadorProductos;
        private readonly RepositoryCreator<IClienteRepository> _creadorClientes;
        private readonly RepositoryCreator<IVentaRepository> _creadorVentas;
        private readonly RepositoryCreator<IDetalleVentaRepository> _creadorDetalles;
        private readonly RepositoryCreator<IInventarioRepository> _creadorInventario;
        private readonly IUnidadDeTrabajo _unidadDeTrabajo;
        private readonly SelectorDescuento _selectorDescuento;
        private readonly IContextoUsuario _contextoUsuario;

        private IGestionarRepository<Producto>? _productos;
        private IClienteRepository? _clientes;
        private IVentaRepository? _ventas;
        private IDetalleVentaRepository? _detalles;
        private IInventarioRepository? _inventario;

        public FachadaVenta(
            RepositoryCreator<IGestionarRepository<Producto>> creadorProductos,
            RepositoryCreator<IClienteRepository> creadorClientes,
            RepositoryCreator<IVentaRepository> creadorVentas,
            RepositoryCreator<IDetalleVentaRepository> creadorDetalles,
            RepositoryCreator<IInventarioRepository> creadorInventario,
            IUnidadDeTrabajo unidadDeTrabajo,
            SelectorDescuento selectorDescuento,
            IContextoUsuario contextoUsuario)
        {
            _creadorProductos = creadorProductos;
            _creadorClientes = creadorClientes;
            _creadorVentas = creadorVentas;
            _creadorDetalles = creadorDetalles;
            _creadorInventario = creadorInventario;
            _unidadDeTrabajo = unidadDeTrabajo;
            _selectorDescuento = selectorDescuento;
            _contextoUsuario = contextoUsuario;
        }

        private IGestionarRepository<Producto> Productos =>
            _productos ??= _creadorProductos.CreateRepository();

        private IClienteRepository Clientes =>
            _clientes ??= _creadorClientes.CreateRepository();

        private IVentaRepository Ventas =>
            _ventas ??= _creadorVentas.CreateRepository();

        private IDetalleVentaRepository Detalles =>
            _detalles ??= _creadorDetalles.CreateRepository();

        private IInventarioRepository Inventario =>
            _inventario ??= _creadorInventario.CreateRepository();

        public List<Cliente> BuscarClientes(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return new List<Cliente>();
            }

            return Clientes.Buscar(termino.Trim());
        }

        public List<Producto> BuscarProductos(string termino)
        {
            List<Producto> activos = Productos.MostrarActivos();

            if (string.IsNullOrWhiteSpace(termino))
            {
                return activos.Take(20).ToList();
            }

            string patron = termino.Trim();

            return activos
                .Where(p =>
                    p.Nombre.Contains(patron, StringComparison.OrdinalIgnoreCase) ||
                    p.Codigo.Contains(patron, StringComparison.OrdinalIgnoreCase))
                .Take(20)
                .ToList();
        }

        public Producto? ObtenerProducto(int idProducto)
        {
            return Productos
                .ObtenerTodos()
                .FirstOrDefault(p => p.IdProducto == idProducto);
        }

        public LineaVentaDetalle? CalcularLinea(
            int idProducto,
            int cantidad,
            out string? error)
        {
            error = null;

            if (cantidad <= 0)
            {
                error = "La cantidad debe ser mayor que cero.";
                return null;
            }

            Producto? producto = ObtenerProducto(idProducto);

            if (producto is null || producto.Estado != 1)
            {
                error = "El producto seleccionado no existe o está inactivo.";
                return null;
            }

            if (cantidad > producto.CantidadStock)
            {
                error = $"Stock insuficiente para '{producto.Nombre}'. " +
                        $"Disponible: {producto.CantidadStock}.";
                return null;
            }

            return new LineaVentaDetalle
            {
                IdProducto = producto.IdProducto,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                UnidadMedida = producto.UnidadMedida,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta,
                TotalProducto = Math.Round(producto.PrecioVenta * cantidad, 2),
                CantidadStock = producto.CantidadStock
            };
        }

        public ResumenVenta CalcularResumen(IEnumerable<LineaVentaDetalle> lineas)
        {
            List<LineaVentaDetalle> lista = lineas.ToList();
            decimal subtotal = lista.Sum(l => l.TotalProducto);
            int unidades = lista.Sum(l => l.Cantidad);

            SeleccionDescuento descuento =
                _selectorDescuento.Seleccionar(subtotal, unidades);

            return new ResumenVenta
            {
                Subtotal = subtotal,
                MontoDescuento = descuento.Monto,
                PorcentajeDescuento = descuento.Porcentaje,
                MontoTotal = Math.Round(subtotal - descuento.Monto, 2),
                PoliticaDescuento = descuento.Politica
            };
        }

        public ResultadoVenta RegistrarVenta(SolicitudVenta solicitud)
        {
            if (solicitud is null)
            {
                return ResultadoVenta.Fallido(
                    "La solicitud de venta es obligatoria.");
            }

            if (solicitud.IdCliente <= 0)
            {
                return ResultadoVenta.Fallido("Debe seleccionar un cliente.");
            }

            if (solicitud.Lineas is null || solicitud.Lineas.Count == 0)
            {
                return ResultadoVenta.Fallido(
                    "Debe agregar al menos un producto a la venta.");
            }

            if (string.IsNullOrWhiteSpace(solicitud.MetodoPago))
            {
                return ResultadoVenta.Fallido(
                    "Debe indicar el método de pago.");
            }

            Dictionary<int, int> cantidades = new();

            foreach (LineaVentaSolicitada linea in solicitud.Lineas)
            {
                if (linea.Cantidad <= 0)
                {
                    return ResultadoVenta.Fallido(
                        "Todas las cantidades deben ser mayores que cero.");
                }

                cantidades[linea.IdProducto] = cantidades.TryGetValue(
                    linea.IdProducto,
                    out int acumulada)
                        ? acumulada + linea.Cantidad
                        : linea.Cantidad;
            }

            int idEmpleado = _contextoUsuario.IdEmpleado;
            string? clave = string.IsNullOrWhiteSpace(solicitud.ClaveIdempotencia)
                ? null
                : solicitud.ClaveIdempotencia.Trim();

            string metodoPago = solicitud.MetodoPago.Trim();
            string estadoPago = string.IsNullOrWhiteSpace(solicitud.EstadoPago)
                ? "Pagado"
                : solicitud.EstadoPago.Trim();

            _unidadDeTrabajo.Iniciar();

            try
            {
                if (clave is not null)
                {
                    Venta? existente =
                        Ventas.ObtenerPorClaveIdempotencia(clave);

                    if (existente is not null)
                    {
                        _unidadDeTrabajo.Confirmar();
                        return ArmarResultado(
                            existente,
                            MapearDetalles(Detalles.ObtenerPorVenta(existente.IdVenta)),
                            idempotente: true);
                    }
                }

                Cliente? cliente = Clientes.ObtenerPorId(solicitud.IdCliente);

                if (cliente is null || cliente.Estado != 1)
                {
                    _unidadDeTrabajo.Revertir();
                    return ResultadoVenta.Fallido(
                        "El cliente seleccionado no existe o está inactivo.");
                }

                List<DetalleVenta> detalles = new();
                List<LineaVentaDetalle> lineasCalculadas = new();
                decimal subtotal = 0m;
                int unidades = 0;

                foreach (KeyValuePair<int, int> par in cantidades)
                {
                    Producto? producto = Inventario.ObtenerParaVenta(par.Key);

                    if (producto is null)
                    {
                        _unidadDeTrabajo.Revertir();
                        return ResultadoVenta.Fallido(
                            $"El producto con id {par.Key} no está disponible.");
                    }

                    if (par.Value > producto.CantidadStock)
                    {
                        _unidadDeTrabajo.Revertir();
                        return ResultadoVenta.Fallido(
                            $"Stock insuficiente para '{producto.Nombre}'. " +
                            $"Disponible: {producto.CantidadStock}.");
                    }

                    if (!Inventario.Descontar(par.Key, par.Value))
                    {
                        _unidadDeTrabajo.Revertir();
                        return ResultadoVenta.Fallido(
                            $"No se pudo descontar el stock de " +
                            $"'{producto.Nombre}'.");
                    }

                    decimal totalProducto =
                        Math.Round(producto.PrecioVenta * par.Value, 2);

                    subtotal += totalProducto;
                    unidades += par.Value;

                    detalles.Add(new DetalleVenta
                    {
                        IdProducto = producto.IdProducto,
                        Cantidad = par.Value,
                        PrecioUnitario = producto.PrecioVenta,
                        TotalProducto = totalProducto,
                        IdEmpleadoResponsable = idEmpleado
                    });

                    lineasCalculadas.Add(new LineaVentaDetalle
                    {
                        IdProducto = producto.IdProducto,
                        Codigo = producto.Codigo,
                        Nombre = producto.Nombre,
                        UnidadMedida = producto.UnidadMedida,
                        Cantidad = par.Value,
                        PrecioUnitario = producto.PrecioVenta,
                        TotalProducto = totalProducto,
                        CantidadStock = producto.CantidadStock - par.Value
                    });
                }

                SeleccionDescuento descuento =
                    _selectorDescuento.Seleccionar(subtotal, unidades);

                Venta venta = new()
                {
                    EstadoVenta = EstadoVenta.Registrada,
                    IdCliente = cliente.IdCliente,
                    MetodoPago = metodoPago,
                    EstadoPago = estadoPago,
                    Subtotal = subtotal,
                    MontoDescuento = descuento.Monto,
                    PorcentajeDescuento = descuento.Porcentaje,
                    MontoTotal = Math.Round(subtotal - descuento.Monto, 2),
                    ClaveIdempotencia = clave,
                    IdEmpleadoResponsable = idEmpleado
                };

                int idVenta = Ventas.Insertar(venta);
                Ventas.AsignarCodigo(idVenta);

                foreach (DetalleVenta detalle in detalles)
                {
                    detalle.IdVenta = idVenta;
                    Detalles.Insertar(detalle);
                }

                _unidadDeTrabajo.Confirmar();

                Venta guardada = Ventas.ObtenerPorId(idVenta) ?? venta;

                return ArmarResultado(
                    guardada,
                    lineasCalculadas,
                    idempotente: false);
            }
            catch (Exception)
            {
                _unidadDeTrabajo.Revertir();

                if (clave is not null)
                {
                    Venta? existente =
                        Ventas.ObtenerPorClaveIdempotencia(clave);

                    if (existente is not null)
                    {
                        return ArmarResultado(
                            existente,
                            MapearDetalles(Detalles.ObtenerPorVenta(existente.IdVenta)),
                            idempotente: true);
                    }
                }

                return ResultadoVenta.Fallido(
                    "No se pudo registrar la venta. Intente nuevamente.");
            }
        }

        public List<Venta> ListarVentas()
        {
            return Ventas.ObtenerTodos();
        }

        public ResultadoVenta? ObtenerVenta(int idVenta)
        {
            Venta? venta = Ventas.ObtenerPorId(idVenta);

            if (venta is null)
            {
                return null;
            }

            return ArmarResultado(
                venta,
                MapearDetalles(Detalles.ObtenerPorVenta(idVenta)),
                idempotente: false);
        }

        public ResultadoVenta AnularVenta(int idVenta, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return ResultadoVenta.Fallido(
                    "Debe indicar el motivo de la anulación.");
            }

            int idEmpleado = _contextoUsuario.IdEmpleado;

            _unidadDeTrabajo.Iniciar();

            try
            {
                Venta? venta = Ventas.ObtenerPorId(idVenta);

                if (venta is null)
                {
                    _unidadDeTrabajo.Revertir();
                    return ResultadoVenta.Fallido("La venta no existe.");
                }

                if (!string.Equals(
                        venta.EstadoVenta,
                        EstadoVenta.Registrada,
                        StringComparison.Ordinal))
                {
                    _unidadDeTrabajo.Revertir();
                    return ResultadoVenta.Fallido(
                        "Solo se pueden anular ventas en estado 'Registrada'.");
                }

                List<DetalleVenta> detalles = Detalles.ObtenerPorVenta(idVenta);

                if (!Ventas.MarcarAnulada(idVenta, idEmpleado, motivo.Trim()))
                {
                    _unidadDeTrabajo.Revertir();
                    return ResultadoVenta.Fallido(
                        "No se pudo anular la venta; pudo haber sido " +
                        "anulada por otra sesión.");
                }

                foreach (DetalleVenta detalle in detalles)
                {
                    Inventario.Reponer(detalle.IdProducto, detalle.Cantidad);
                }

                _unidadDeTrabajo.Confirmar();

                Venta actualizada = Ventas.ObtenerPorId(idVenta) ?? venta;

                return ArmarResultado(
                    actualizada,
                    MapearDetalles(detalles),
                    idempotente: false);
            }
            catch (Exception)
            {
                _unidadDeTrabajo.Revertir();
                return ResultadoVenta.Fallido(
                    "No se pudo anular la venta. Intente nuevamente.");
            }
        }

        private static List<LineaVentaDetalle> MapearDetalles(
            IEnumerable<DetalleVenta> detalles)
        {
            return detalles
                .Select(d => new LineaVentaDetalle
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    TotalProducto = d.TotalProducto
                })
                .ToList();
        }

        private static ResultadoVenta ArmarResultado(
            Venta venta,
            List<LineaVentaDetalle> detalles,
            bool idempotente)
        {
            return ResultadoVenta.Correcto(venta, detalles, idempotente);
        }
    }
}
