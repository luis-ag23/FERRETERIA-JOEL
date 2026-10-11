using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Politicas;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Pruebas.Compartido;

namespace FERRETERIA__Joel.Pruebas.Servicios
{
    public class FachadaVentaPruebas
    {
        private sealed class Escenario
        {
            public required FachadaVenta Fachada { get; init; }

            public required InventarioRepositorioFake Inventario { get; init; }

            public required VentaRepositorioFake Ventas { get; init; }

            public required DetalleVentaRepositorioFake Detalles { get; init; }

            public required UnidadDeTrabajoFake UnidadDeTrabajo { get; init; }

            public required ClienteRepositorioFake Clientes { get; init; }

            public required ProductoRepositorioFake Productos { get; init; }
        }

        private static Escenario CrearEscenario(params Producto[] productos)
        {
            ProductoRepositorioFake productosRepo = new();
            ClienteRepositorioFake clientesRepo = new();
            VentaRepositorioFake ventasRepo = new();
            DetalleVentaRepositorioFake detallesRepo = new();
            InventarioRepositorioFake inventarioRepo = new();
            UnidadDeTrabajoFake unidadDeTrabajo = new();

            clientesRepo.Agregar(new Cliente
            {
                Nombre = "Juan",
                Apellido = "Perez",
                Telefono = "7777-0000",
                Estado = 1
            });

            foreach (Producto producto in productos)
            {
                productosRepo.Agregar(producto);
                inventarioRepo.Agregar(producto);
            }

            SelectorDescuento selector = new(new IPoliticaDescuento[]
            {
                new DescuentoPorMonto(montoMinimo: 1000m, porcentaje: 5m),
                new DescuentoPorCantidad(cantidadMinima: 12, porcentaje: 3m)
            });

            FachadaVenta fachada = new(
                new CreadorFake<Dominio.Puertos.IGestionarRepository<Producto>>(productosRepo),
                new CreadorFake<Dominio.Puertos.IClienteRepository>(clientesRepo),
                new CreadorFake<Dominio.Puertos.IVentaRepository>(ventasRepo),
                new CreadorFake<Dominio.Puertos.IDetalleVentaRepository>(detallesRepo),
                new CreadorFake<Dominio.Puertos.IInventarioRepository>(inventarioRepo),
                unidadDeTrabajo,
                selector,
                new ContextoUsuarioFake());

            return new Escenario
            {
                Fachada = fachada,
                Inventario = inventarioRepo,
                Ventas = ventasRepo,
                Detalles = detallesRepo,
                UnidadDeTrabajo = unidadDeTrabajo,
                Clientes = clientesRepo,
                Productos = productosRepo
            };
        }

        private static Producto CrearProducto(
            int id,
            decimal precio,
            int stock,
            string nombre = "Producto")
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
        public void RegistrarVenta_SinCliente_Falla()
        {
            Escenario escenario = CrearEscenario();

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
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
        public void RegistrarVenta_SinLineas_Falla()
        {
            Escenario escenario = CrearEscenario();

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo"
                });

            Assert.False(resultado.Exito);
            Assert.Equal(0, escenario.Ventas.VecesInsertar);
        }

        [Fact]
        public void RegistrarVenta_CantidadInvalida_Falla()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
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
        public void RegistrarVenta_Exitosa_DescuentaStockYRegistra()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
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
        public void RegistrarVenta_AplicaDescuentoPorMonto()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 600m, 10));

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 2 }
                    }
                });

            Assert.True(resultado.Exito);
            Assert.Equal(1200m, resultado.Subtotal);
            Assert.Equal(60m, resultado.MontoDescuento);
            Assert.Equal(5m, resultado.PorcentajeDescuento);
            Assert.Equal(1140m, resultado.MontoTotal);
        }

        [Fact]
        public void RegistrarVenta_AplicaDescuentoPorCantidad()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 50m, 50));

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 12 }
                    }
                });

            Assert.True(resultado.Exito);
            Assert.Equal(600m, resultado.Subtotal);
            Assert.Equal(18m, resultado.MontoDescuento);
            Assert.Equal(582m, resultado.MontoTotal);
        }

        [Fact]
        public void RegistrarVenta_StockInsuficiente_RevierteYNoRegistra()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 3));

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
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
        public void RegistrarVenta_Idempotente_NoDuplica()
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

            ResultadoVenta primero = escenario.Fachada.RegistrarVenta(solicitud);
            ResultadoVenta segundo = escenario.Fachada.RegistrarVenta(solicitud);

            Assert.True(primero.Exito);
            Assert.True(segundo.Exito);
            Assert.True(segundo.Idempotente);
            Assert.Equal(primero.IdVenta, segundo.IdVenta);
            Assert.Equal(1, escenario.Ventas.VecesInsertar);
            Assert.Equal(8, escenario.Inventario.Stock(1));
        }

        [Fact]
        public void RegistrarVenta_AgrupaLineasDuplicadas()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta resultado = escenario.Fachada.RegistrarVenta(
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
        public void AnularVenta_RestauraStock()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta venta = escenario.Fachada.RegistrarVenta(
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

            ResultadoVenta anulada = escenario.Fachada.AnularVenta(
                venta.IdVenta,
                "Producto defectuoso");

            Assert.True(anulada.Exito);
            Assert.Equal(EstadoVenta.Anulada, anulada.EstadoVenta);
            Assert.Equal(10, escenario.Inventario.Stock(1));
            Assert.Equal(2, escenario.UnidadDeTrabajo.VecesConfirmar);
        }

        [Fact]
        public void AnularVenta_ConEstadoNoRegistrada_Falla()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 10));

            ResultadoVenta venta = escenario.Fachada.RegistrarVenta(
                new SolicitudVenta
                {
                    IdCliente = 1,
                    MetodoPago = "Efectivo",
                    Lineas = new List<LineaVentaSolicitada>
                    {
                        new() { IdProducto = 1, Cantidad = 1 }
                    }
                });

            escenario.Fachada.AnularVenta(venta.IdVenta, "Primera anulación");
            ResultadoVenta segunda = escenario.Fachada.AnularVenta(
                venta.IdVenta,
                "Segunda anulación");

            Assert.False(segunda.Exito);
        }

        [Fact]
        public void CalcularLinea_StockInsuficiente_DevuelveError()
        {
            Escenario escenario = CrearEscenario(CrearProducto(1, 100m, 2));

            LineaVentaDetalle? linea =
                escenario.Fachada.CalcularLinea(1, 5, out string? error);

            Assert.Null(linea);
            Assert.NotNull(error);
        }

        [Fact]
        public void CalcularResumen_AplicaDescuento()
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
            Assert.Equal(60m, resumen.MontoDescuento);
            Assert.Equal(1140m, resumen.MontoTotal);
        }
    }
}
