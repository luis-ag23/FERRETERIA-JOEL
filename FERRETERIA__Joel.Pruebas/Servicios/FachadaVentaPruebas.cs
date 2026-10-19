using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Pruebas.Compartido;

namespace FERRETERIA__Joel.Pruebas.Servicios
{
    public class FachadaVentaPruebas
    {
        private sealed class Escenario
        {
            public required FachadaVenta Fachada { get; init; }
            public required FachadaAnulacion Anulacion { get; init; }
            public required InventarioRepositorioFake Inventario { get; init; }
            public required VentaRepositorioFake Ventas { get; init; }
            public required DetalleVentaRepositorioFake Detalles { get; init; }
            public required UnidadDeTrabajoFake UnidadDeTrabajo { get; init; }
            public required ClienteRepositorioFake Clientes { get; init; }
        }

        private static Escenario CrearEscenario(params Producto[] productos)
        {
            return CrearEscenarioCon(new ContextoUsuarioFake(), productos);
        }

        private static Escenario CrearEscenarioCon(
            IContextoUsuario contexto,
            params Producto[] productos)
        {
            ClienteRepositorioFake clientesRepo = new();
            VentaRepositorioFake ventasRepo = new();
            DetalleVentaRepositorioFake detallesRepo = new();
            InventarioRepositorioFake inventarioRepo = new();
            EmpleadoVentaRepositorioFake empleadosRepo = new();
            UnidadDeTrabajoFake unidadDeTrabajo = new();

            clientesRepo.Agregar(new Cliente
            {
                Nombre = "Juan",
                Apellido = "Perez",
                Telefono = "7777-0000",
                Estado = 1
            });

            empleadosRepo.Agregar(new Empleado
            {
                IdEmpleado = 42,
                Nombre = "Usuario",
                Apellido = "De Prueba",
                Rol = RolesSistema.EmpleadoVenta
            });

            foreach (Producto producto in productos)
            {
                inventarioRepo.Agregar(producto);
            }

            CreadorFake<IRepository<Empleado>> creadorEmpleados = new(empleadosRepo);

            FachadaActualizacionStock stock = new(inventarioRepo);

            ServicioCliente servicioCliente = new(clientesRepo, contexto);

            FachadaVenta fachada = new(
                ventasRepo,
                detallesRepo,
                creadorEmpleados,
                unidadDeTrabajo,
                contexto,
                stock,
                servicioCliente);

            FachadaAnulacion anulacion = new(
                ventasRepo,
                detallesRepo,
                servicioCliente,
                unidadDeTrabajo,
                contexto,
                stock);

            return new Escenario
            {
                Fachada = fachada,
                Anulacion = anulacion,
                Inventario = inventarioRepo,
                Ventas = ventasRepo,
                Detalles = detallesRepo,
                UnidadDeTrabajo = unidadDeTrabajo,
                Clientes = clientesRepo
            };
        }

        private static Producto CrearProducto(int id, decimal precio, int stock, string nombre = "Producto")
        {
            return new Producto
            {
                IdProducto = id,
                IdCategoria = 1,
                IdMarca = 1,
                Codigo = $"COD-{id:D3}",
                Nombre = nombre,
                UnidadMedida = "Unidad",
                PrecioVenta = precio,
                CantidadStock = stock,
                Estado = 1,
                IdEmpleadoResponsable = 1
            };
        }

        [Fact]
        public async Task RegistrarVenta_SinCliente_FallaAsync()
        {
            Escenario escenario = CrearEscenario();

            ResultadoVenta resultado = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 0,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 1 }
                    }
                });

            Assert.False(resultado.Exito);
            Assert.Equal(0, escenario.Ventas.VecesInsertar);
        }

        [Fact]
        public async Task RegistrarVenta_SinLineas_FallaAsync()
        {
            Escenario escenario = CrearEscenario();

            ResultadoVenta resultado = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo"
                });

            Assert.False(resultado.Exito);
            Assert.Equal(0, escenario.Ventas.VecesInsertar);
        }

        [Fact]
        public async Task RegistrarVenta_CantidadInvalida_FallaAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta resultado = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 0 }
                    }
                });

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task RegistrarVenta_Exitosa_DescuentaStockYRegistraAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta resultado = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    EstadoPago = "Pagado",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 2 }
                    }
                });

            Assert.True(resultado.Exito);
            Assert.False(resultado.Idempotente);
            Assert.Equal(200m, resultado.Subtotal);
            Assert.Equal(200m, resultado.MontoTotal);
            Assert.Equal(8, escenario.Inventario.Stock(1));
            Assert.Equal(1, escenario.Ventas.VecesInsertar);
            Assert.Equal(1, escenario.UnidadDeTrabajo.VecesConfirmar);
            Assert.Equal(0, escenario.UnidadDeTrabajo.VecesRevertir);
            Assert.Single(resultado.Detalles);
            Assert.Equal(2, resultado.Detalles[0].Cantidad);
        }

        [Fact]
        public async Task RegistrarVenta_StockInsuficiente_RevierteYNoRegistraAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 3));

            ResultadoVenta resultado = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 5 }
                    }
                });

            Assert.False(resultado.Exito);
            Assert.Equal(3, escenario.Inventario.Stock(1));
            Assert.Equal(0, escenario.Ventas.VecesInsertar);
            Assert.Equal(1, escenario.UnidadDeTrabajo.VecesRevertir);
            Assert.Equal(0, escenario.UnidadDeTrabajo.VecesConfirmar);
        }

        [Fact]
        public async Task RegistrarVenta_Idempotente_NoDuplicaAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            SolicitudVenta solicitud = new()
            {
                IdCliente = 1,
                MetodoPago = "Efectivo",
                ClaveIdempotencia = "clave-123",
                Lineas = new List<LineaVentaSolicitada>
                {
                    new() { IdProducto = 1, Cantidad = 2 }
                }
            };

            ResultadoVenta primero = await escenario.Fachada.RegistrarVentaAsync(solicitud);
            ResultadoVenta segundo = await escenario.Fachada.RegistrarVentaAsync(solicitud);

            Assert.True(primero.Exito);
            Assert.True(segundo.Exito);
            Assert.True(segundo.Idempotente);
            Assert.Equal(primero.IdVenta, segundo.IdVenta);
            Assert.Equal(1, escenario.Ventas.VecesInsertar);
            Assert.Equal(8, escenario.Inventario.Stock(1));
        }

        [Fact]
        public async Task RegistrarVenta_AgrupaLineasDuplicadasAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta resultado = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 1 },
                        new() { IdProducto = 1, Cantidad = 3 }
                    }
                });

            Assert.True(resultado.Exito);
            Assert.Single(resultado.Detalles);
            Assert.Equal(4, resultado.Detalles[0].Cantidad);
            Assert.Equal(400m, resultado.Subtotal);
            Assert.Equal(6, escenario.Inventario.Stock(1));
        }

        [Fact]
        public async Task RegistrarVenta_EmpleadoInvalido_FallaAsync()
        {
            Escenario escenario = CrearEscenarioCon(
                new ContextoUsuarioFake(idEmpleado: 0),
                CrearProducto(1, 100m, 10));

            ResultadoVenta resultado = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 1 }
                    }
                });

            Assert.False(resultado.Exito);
            Assert.Equal(0, escenario.Ventas.VecesInsertar);
            Assert.Equal(10, escenario.Inventario.Stock(1));
        }

        [Fact]
        public async Task AnularVenta_RestauraStockAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta venta = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 4 }
                    }
                });

            Assert.Equal(6, escenario.Inventario.Stock(1));

            ResultadoVenta anulada = await escenario.Anulacion.AnularVentaAsync(
                venta.IdVenta,
                "Producto defectuoso");

            Assert.True(anulada.Exito);
            Assert.Equal(EstadoVenta.Anulada, anulada.EstadoVenta);
            Assert.Equal(10, escenario.Inventario.Stock(1));
            Assert.Equal(2, escenario.UnidadDeTrabajo.VecesConfirmar);
        }

        [Fact]
        public async Task AnularVenta_ConEstadoNoRegistrada_FallaAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta venta = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 1 }
                    }
                });

            await escenario.Anulacion.AnularVentaAsync(venta.IdVenta, "Primera anulación");
            ResultadoVenta segunda = await escenario.Anulacion.AnularVentaAsync(
                venta.IdVenta,
                "Segunda anulación");

            Assert.False(segunda.Exito);
        }

        [Fact]
        public async Task AnularVenta_EmpleadoInvalido_FallaAsync()
        {
            Escenario escenario = CrearEscenarioCon(
                new ContextoUsuarioFake(idEmpleado: 0),
                CrearProducto(1, 100m, 10));

            ResultadoVenta resultado =
                await escenario.Anulacion.AnularVentaAsync(1, "Intento anónimo");

            Assert.False(resultado.Exito);
        }

        [Fact]
        public async Task CalcularLinea_StockInsuficiente_DevuelveErrorAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 2));

            var (linea, error) = await escenario.Fachada.CalcularLineaAsync(1, 5);

            Assert.Null(linea);
            Assert.NotNull(error);
        }

        [Fact]
        public void CalcularResumen_CalculaSubtotalYTotal()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 600m, 10));

            ResumenVenta resumen = escenario.Fachada.CalcularResumen(
                new List<LineaVentaDetalle>
                {
                    new()
                    {
                        IdProducto = 1,
                        Cantidad = 2,
                        PrecioUnitario = 600m,
                        TotalProducto = 1200m
                    }
                });

            Assert.Equal(1200m, resumen.Subtotal);
            Assert.Equal(1200m, resumen.MontoTotal);
        }

        [Fact]
        public async Task ListarVentasListado_IncluyeNombreClienteAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 2 }
                    }
                });

            List<VentaListado> lista = await escenario.Anulacion.ListarVentasListadoAsync();

            Assert.Single(lista);
            Assert.Equal("Juan Perez", lista[0].NombreCliente);
            Assert.Equal("V-TEST-000001", lista[0].Venta.CodigoVenta);
        }

        [Fact]
        public async Task ListarVentasListado_ClienteEliminado_CaeEnResguardoAsync()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            await escenario.Ventas.InsertarAsync(new Venta
            {
                EstadoVenta = EstadoVenta.Registrada,
                IdCliente = 999,
                MontoTotal = 50m
            });

            List<VentaListado> lista = await escenario.Anulacion.ListarVentasListadoAsync();

            Assert.Single(lista);
            Assert.Equal("Cliente eliminado", lista[0].NombreCliente);
        }

        [Fact]
        public async Task ObtenerComprobante_DevuelveLineasConCatalogoAsync()
        {
            Escenario escenario = CrearEscenario(
                CrearProducto(1, 150m, 10, "Taladro X"));

            ResultadoVenta venta = await escenario.Fachada.RegistrarVentaAsync(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Tarjeta",
                    EstadoPago = "Pagado",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 3 }
                    }
                });

            ComprobanteVenta? comprobante =
                await escenario.Fachada.ObtenerComprobanteAsync(venta.IdVenta);

            Assert.NotNull(comprobante);
            Assert.Equal("Juan Perez", comprobante.NombreCliente);
            Assert.Equal("Usuario De Prueba", comprobante.NombreEmpleadoResponsable);
            Assert.Equal("Tarjeta", comprobante.Venta.MetodoPago);
            Assert.Equal(450m, comprobante.Venta.MontoTotal);
            Assert.Single(comprobante.Lineas);
            Assert.Equal("Taladro X", comprobante.Lineas[0].Nombre);
            Assert.Equal("COD-001", comprobante.Lineas[0].Codigo);
            Assert.Equal(3, comprobante.Lineas[0].Cantidad);
        }

        [Fact]
        public async Task ObtenerComprobante_NoExistente_DevuelveNuloAsync()
        {
            Escenario escenario = CrearEscenario();

            Assert.Null(await escenario.Fachada.ObtenerComprobanteAsync(123));
        }
    }
}
