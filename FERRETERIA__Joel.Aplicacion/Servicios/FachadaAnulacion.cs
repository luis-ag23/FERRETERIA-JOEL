using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Fachada (Facade) de las ventas YA REGISTRADAS: consulta del
    /// historial, detalle de una venta y anulación (borrado lógico).
    ///
    /// Al anular, la venta completa pasa a estado "Anulada" y se restauran
    /// las existencias consumidas: detalle_venta NO se elimina, se usa
    /// como fuente para reponer el stock producto a producto. La reposición
    /// se delega en <see cref="FachadaActualizacionStock"/> y todo ocurre
    /// dentro de una única transacción (commit/rollback). Es la fachada
    /// que consume la pantalla de Historial de ventas.
    /// </summary>
    public class FachadaAnulacion
    {
        private readonly MySqlVentaRepository _ventas;
        private readonly MySqlDetalleVentaRepository _detalles;
        private readonly ServicioCliente _servicioCliente;
        private readonly IUnidadDeTrabajo _unidadDeTrabajo;
        private readonly IContextoUsuario _contextoUsuario;
        private readonly FachadaActualizacionStock _stock;

        public FachadaAnulacion(
            MySqlVentaRepository ventas,
            MySqlDetalleVentaRepository detalles,
            ServicioCliente servicioCliente,
            IUnidadDeTrabajo unidadDeTrabajo,
            IContextoUsuario contextoUsuario,
            FachadaActualizacionStock stock)
        {
            _ventas = ventas;
            _detalles = detalles;
            _servicioCliente = servicioCliente;
            _unidadDeTrabajo = unidadDeTrabajo;
            _contextoUsuario = contextoUsuario;
            _stock = stock;
        }

        // ---------- Consulta (Select / historial) ----------

        public async Task<List<VentaListado>> ListarVentasListadoAsync()
        {
            List<Venta> ventas = await _ventas.ObtenerTodosAsync();

            Dictionary<int, string> nombresCliente = new();

            foreach (int idCliente in ventas.Select(v => v.IdCliente).Distinct())
            {
                Cliente? cliente =
                    await _servicioCliente.ObtenerPorIdAsync(idCliente);

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
            Venta? venta = await _ventas.ObtenerPorIdAsync(idVenta);

            if (venta is null)
            {
                return null;
            }

            return ResultadoVenta.Correcto(
                venta,
                await _stock.MapearLineasAsync(
                    await _detalles.ObtenerPorVentaAsync(idVenta)));
        }

        // ---------- Anulación (borrado lógico + reposición de stock) ----------

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
                Venta? venta = await _ventas.ObtenerPorIdAsync(idVenta);

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
                    await _detalles.ObtenerPorVentaAsync(idVenta);

                if (!await _ventas.MarcarAnuladaAsync(idVenta, idEmpleado, motivo.Trim()))
                {
                    await _unidadDeTrabajo.RevertirAsync();
                    return ResultadoVenta.Fallido(
                        "No se pudo anular la venta; pudo haber sido " +
                        "anulada por otra sesión.");
                }

                // detalle_venta NO se toca: se usa para restaurar el stock.
                foreach (DetalleVenta detalle in detalles)
                {
                    await _stock.ReponerAsync(detalle.IdProducto, detalle.Cantidad);
                }

                await _unidadDeTrabajo.ConfirmarAsync();

                Venta actualizada =
                    await _ventas.ObtenerPorIdAsync(idVenta) ?? venta;

                return ResultadoVenta.Correcto(
                    actualizada,
                    await _stock.MapearLineasAsync(detalles));
            }
            catch (Exception)
            {
                await _unidadDeTrabajo.RevertirAsync();
                return ResultadoVenta.Fallido(
                    "No se pudo anular la venta. Intente nuevamente.");
            }
        }
    }
}
