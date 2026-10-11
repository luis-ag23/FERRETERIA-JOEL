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
    /// Todo el flujo de escritura es asíncrono (async/await) de extremo
    /// a extremo: la presentación, esta fachada, los repositorios y la
    /// transacción MySQL. La presentación no conoce repositorios,
    /// transacciones ni políticas: solo consume esta fachada.
    /// </summary>
    public class FachadaVenta
    {
        private readonly RepositoryCreator<IClienteRepository> _creadorClientes;
        private readonly RepositoryCreator<IVentaRepository> _creadorVentas;
        private readonly RepositoryCreator<IDetalleVentaRepository> _creadorDetalles;
        private readonly RepositoryCreator<IInventarioRepository> _creadorInventario;
        private readonly RepositoryCreator<IEmpleadoVentaRepository> _creadorEmpleados;
        private readonly IUnidadDeTrabajo _unidadDeTrabajo;
        private readonly SelectorDescuento _selectorDescuento;
        private readonly IContextoUsuario _contextoUsuario;

        private IClienteRepository? _clientes;
        private IVentaRepository? _ventas;
        private IDetalleVentaRepository? _detalles;
        private IInventarioRepository? _inventario;
        private IEmpleadoVentaRepository? _empleados;

        public FachadaVenta(
            RepositoryCreator<IClienteRepository> creadorClientes,
            RepositoryCreator<IVentaRepository> creadorVentas,
            RepositoryCreator<IDetalleVentaRepository> creadorDetalles,
            RepositoryCreator<IInventarioRepository> creadorInventario,
            RepositoryCreator<IEmpleadoVentaRepository> creadorEmpleados,
            IUnidadDeTrabajo unidadDeTrabajo,
            SelectorDescuento selectorDescuento,
            IContextoUsuario contextoUsuario)
        {
            _creadorClientes = creadorClientes;
            _creadorVentas = creadorVentas;
            _creadorDetalles = creadorDetalles;
            _creadorInventario = creadorInventario;
            _creadorEmpleados = creadorEmpleados;
            _unidadDeTrabajo = unidadDeTrabajo;
            _selectorDescuento = selectorDescuento;
            _contextoUsuario = contextoUsuario;
        }

        private IClienteRepository Clientes =>
            _clientes ??= _creadorClientes.CreateRepository();

        private IVentaRepository Ventas =>
            _ventas ??= _creadorVentas.CreateRepository();

        private IDetalleVentaRepository Detalles =>
            _detalles ??= _creadorDetalles.CreateRepository();

        private IInventarioRepository Inventario =>
            _inventario ??= _creadorInventario.CreateRepository();

        private IEmpleadoVentaRepository Empleados =>
            _empleados ??= _creadorEmpleados.CreateRepository();

        public async Task<List<Cliente>> BuscarClientesAsync(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return new List<Cliente>();
            }

            return await Clientes.BuscarAsync(termino.Trim());
        }

        public async Task<List<Producto>> BuscarProductosAsync(string termino)
        {
            List<Producto> activos = await Inventario.ListarActivosAsync();

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

        public Task<Producto?> ObtenerProductoAsync(int idProducto)
        {
            return Inventario.ObtenerParaVentaAsync(idProducto);
        }

        public async Task<(LineaVentaDetalle? Linea, string? Error)> CalcularLineaAsync(
            int idProducto,
            int cantidad)
        {
            if (cantidad <= 0)
            {
                return (null, "La cantidad debe ser mayor que cero.");
            }

            Producto? producto = await ObtenerProductoAsync(idProducto);

            if (producto is null || producto.Estado != 1)
            {
                return (null, "El producto seleccionado no existe o está inactivo.");
            }

            if (cantidad > producto.CantidadStock)
            {
                return (null,
                    $"Stock insuficiente para '{producto.Nombre}'. " +
                    $"Disponible: {producto.CantidadStock}.");
            }

            return (new LineaVentaDetalle
            {
                IdProducto = producto.IdProducto,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                UnidadMedida = producto.UnidadMedida,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta,
                TotalProducto = Math.Round(producto.PrecioVenta * cantidad, 2),
                CantidadStock = producto.CantidadStock
            }, null);
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

        public async Task<ResultadoVenta> RegistrarVentaAsync(SolicitudVenta solicitud)
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

            int idEmpleado = _contextoUsuario.IdEmpleado;

            if (idEmpleado <= 0)
            {
                return ResultadoVenta.Fallido(
                    "Debe iniciar sesión para registrar la venta.");
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

            string? clave = string.IsNullOrWhiteSpace(solicitud.ClaveIdempotencia)
                ? null
                : solicitud.ClaveIdempotencia.Trim();

            string metodoPago = solicitud.MetodoPago.Trim();
            string estadoPago = string.IsNullOrWhiteSpace(solicitud.EstadoPago)
                ? "Pagado"
                : solicitud.EstadoPago.Trim();

            await _unidadDeTrabajo.IniciarAsync();

            try
            {
                if (clave is not null)
                {
                    Venta? existente =
                        await Ventas.ObtenerPorClaveIdempotenciaAsync(clave);

                    if (existente is not null)
                    {
                        await _unidadDeTrabajo.ConfirmarAsync();
                        return ArmarResultado(
                            existente,
                            await MapearDetallesAsync(
                                await Detalles.ObtenerPorVentaAsync(existente.IdVenta)),
                            idempotente: true);
                    }
                }

                Cliente? cliente =
                    await Clientes.ObtenerPorIdAsync(solicitud.IdCliente);

                if (cliente is null || cliente.Estado != 1)
                {
                    await _unidadDeTrabajo.RevertirAsync();
                    return ResultadoVenta.Fallido(
                        "El cliente seleccionado no existe o está inactivo.");
                }

                List<DetalleVenta> detalles = new();
                List<LineaVentaDetalle> lineasCalculadas = new();
                decimal subtotal = 0m;
                int unidades = 0;

                foreach (KeyValuePair<int, int> par in cantidades)
                {
                    Producto? producto =
                        await Inventario.ObtenerParaVentaAsync(par.Key);

                    if (producto is null)
                    {
                        await _unidadDeTrabajo.RevertirAsync();
                        return ResultadoVenta.Fallido(
                            $"El producto con id {par.Key} no está disponible.");
                    }

                    if (par.Value > producto.CantidadStock)
                    {
                        await _unidadDeTrabajo.RevertirAsync();
                        return ResultadoVenta.Fallido(
                            $"Stock insuficiente para '{producto.Nombre}'. " +
                            $"Disponible: {producto.CantidadStock}.");
                    }

                    if (!await Inventario.DescontarAsync(par.Key, par.Value))
                    {
                        await _unidadDeTrabajo.RevertirAsync();
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

                int idVenta = await Ventas.InsertarAsync(venta);
                await Ventas.AsignarCodigoAsync(idVenta);

                foreach (DetalleVenta detalle in detalles)
                {
                    detalle.IdVenta = idVenta;
                    await Detalles.InsertarAsync(detalle);
                }

                await _unidadDeTrabajo.ConfirmarAsync();

                Venta guardada =
                    await Ventas.ObtenerPorIdAsync(idVenta) ?? venta;

                return ArmarResultado(
                    guardada,
                    lineasCalculadas,
                    idempotente: false);
            }
            catch (Exception)
            {
                await _unidadDeTrabajo.RevertirAsync();

                if (clave is not null)
                {
                    Venta? existente =
                        await Ventas.ObtenerPorClaveIdempotenciaAsync(clave);

                    if (existente is not null)
                    {
                        return ArmarResultado(
                            existente,
                            await MapearDetallesAsync(
                                await Detalles.ObtenerPorVentaAsync(existente.IdVenta)),
                            idempotente: true);
                    }
                }

                return ResultadoVenta.Fallido(
                    "No se pudo registrar la venta. Intente nuevamente.");
            }
        }

        public Task<List<Venta>> ListarVentasAsync()
        {
            return Ventas.ObtenerTodosAsync();
        }

        public async Task<List<VentaListado>> ListarVentasListadoAsync()
        {
            List<Venta> ventas = await Ventas.ObtenerTodosAsync();

            Dictionary<int, string> nombresCliente = new();

            foreach (int idCliente in ventas.Select(v => v.IdCliente).Distinct())
            {
                Cliente? cliente = await Clientes.ObtenerPorIdAsync(idCliente);

                if (cliente is not null)
                {
                    nombresCliente[idCliente] =
                        $"{cliente.Nombre} {cliente.Apellido}".Trim();
                }
            }

            return ventas
                .Select(v => new VentaListado
                {
                    Venta = v,
                    NombreCliente = nombresCliente.TryGetValue(
                        v.IdCliente,
                        out string? nombre)
                        ? nombre
                        : "Cliente eliminado"
                })
                .ToList();
        }

        public async Task<ResultadoVenta?> ObtenerVentaAsync(int idVenta)
        {
            Venta? venta = await Ventas.ObtenerPorIdAsync(idVenta);

            if (venta is null)
            {
                return null;
            }

            return ArmarResultado(
                venta,
                await MapearDetallesAsync(
                    await Detalles.ObtenerPorVentaAsync(idVenta)),
                idempotente: false);
        }

        public async Task<ComprobanteVenta?> ObtenerComprobanteAsync(int idVenta)
        {
            Venta? venta = await Ventas.ObtenerPorIdAsync(idVenta);

            if (venta is null)
            {
                return null;
            }

            Cliente? cliente = await Clientes.ObtenerPorIdAsync(venta.IdCliente);

            string nombreCliente = cliente is null
                ? "Cliente eliminado"
                : $"{cliente.Nombre} {cliente.Apellido}".Trim();

            List<Empleado> todos = await Empleados.ListarAsync();

            return new ComprobanteVenta
            {
                Venta = venta,
                NombreCliente = nombreCliente,
                NombreEmpleadoResponsable = NombreDe(todos, venta.IdEmpleadoResponsable),
                NombreEmpleadoAnulacion = venta.IdEmpleadoAnulacion is not null
                    ? NombreDe(todos, venta.IdEmpleadoAnulacion.Value)
                    : null,
                Lineas = await MapearDetallesAsync(
                    await Detalles.ObtenerPorVentaAsync(idVenta))
            };
        }

        private static string NombreDe(List<Empleado> empleados, int idEmpleado)
        {
            Empleado? empleado = empleados
                .FirstOrDefault(e => e.IdEmpleado == idEmpleado);

            return empleado is null
                ? $"Empleado #{idEmpleado}"
                : $"{empleado.Nombre} {empleado.Apellido}".Trim();
        }

        public async Task<ResultadoVenta> AnularVentaAsync(int idVenta, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                return ResultadoVenta.Fallido(
                    "Debe indicar el motivo de la anulación.");
            }

            int idEmpleado = _contextoUsuario.IdEmpleado;

            if (idEmpleado <= 0)
            {
                return ResultadoVenta.Fallido(
                    "Debe iniciar sesión para anular la venta.");
            }

            await _unidadDeTrabajo.IniciarAsync();

            try
            {
                Venta? venta = await Ventas.ObtenerPorIdAsync(idVenta);

                if (venta is null)
                {
                    await _unidadDeTrabajo.RevertirAsync();
                    return ResultadoVenta.Fallido("La venta no existe.");
                }

                if (!string.Equals(
                        venta.EstadoVenta,
                        EstadoVenta.Registrada,
                        StringComparison.Ordinal))
                {
                    await _unidadDeTrabajo.RevertirAsync();
                    return ResultadoVenta.Fallido(
                        "Solo se pueden anular ventas en estado 'Registrada'.");
                }

                List<DetalleVenta> detalles =
                    await Detalles.ObtenerPorVentaAsync(idVenta);

                if (!await Ventas.MarcarAnuladaAsync(idVenta, idEmpleado, motivo.Trim()))
                {
                    await _unidadDeTrabajo.RevertirAsync();
                    return ResultadoVenta.Fallido(
                        "No se pudo anular la venta; pudo haber sido " +
                        "anulada por otra sesión.");
                }

                foreach (DetalleVenta detalle in detalles)
                {
                    await Inventario.ReponerAsync(detalle.IdProducto, detalle.Cantidad);
                }

                await _unidadDeTrabajo.ConfirmarAsync();

                Venta actualizada =
                    await Ventas.ObtenerPorIdAsync(idVenta) ?? venta;

                return ArmarResultado(
                    actualizada,
                    await MapearDetallesAsync(detalles),
                    idempotente: false);
            }
            catch (Exception)
            {
                await _unidadDeTrabajo.RevertirAsync();
                return ResultadoVenta.Fallido(
                    "No se pudo anular la venta. Intente nuevamente.");
            }
        }

        private async Task<List<LineaVentaDetalle>> MapearDetallesAsync(
            IEnumerable<DetalleVenta> detalles)
        {
            List<DetalleVenta> lista = detalles.ToList();

            List<Producto> productos = await Inventario.ObtenerPorIdsAsync(
                lista.Select(d => d.IdProducto).Distinct().ToList());

            Dictionary<int, Producto> productoPorId =
                productos.ToDictionary(p => p.IdProducto);

            return lista
                .Select(d =>
                {
                    Producto? producto = productoPorId.GetValueOrDefault(
                        d.IdProducto);

                    return new LineaVentaDetalle
                    {
                        IdProducto = d.IdProducto,
                        Codigo = producto?.Codigo ?? string.Empty,
                        Nombre = producto?.Nombre
                            ?? $"Producto #{d.IdProducto}",
                        UnidadMedida = producto?.UnidadMedida ?? string.Empty,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        TotalProducto = d.TotalProducto
                    };
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
