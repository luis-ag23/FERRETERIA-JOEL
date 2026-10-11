using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Politicas;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Pruebas.Compartido;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Pruebas.Integracion
{
    /// <summary>
    /// Pruebas de integración reales contra MySQL. Se ejecutan solo si
    /// la variable de entorno FERRETERIA_TEST_CONNECTION apunta a una
    /// base de desarrollo; en caso contrario se omiten sin fallar.
    /// </summary>
    public class FachadaVentaIntegracionPruebas
    {
        private const string VariableConexion = "FERRETERIA_TEST_CONNECTION";

        private static string? CadenaConexion =>
            Environment.GetEnvironmentVariable(VariableConexion);

        [Fact]
        public async Task RegistrarVenta_Real_DescuentaStockYAnulacionRestauraAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            int idCategoria = ObtenerPrimerId(cadena, "categoria", "IdCategoria");
            int idMarca = ObtenerPrimerId(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = CrearProducto(
                cadena, idCategoria, idMarca, precio: 100m, stock: 10);
            int idCliente = CrearCliente(cadena);

            try
            {
                (FachadaVenta fachada, MySqlConexionTransaccional conexion) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexion)
                {
                    ResultadoVenta venta = await fachada.RegistrarVentaAsync(
                        new SolicitudVenta
                        {
                            IdCliente = idCliente,
                            MetodoPago = "Efectivo",
                            EstadoPago = "Pagado",
                            Lineas = new List<LineaVentaSolicitada>
                            {
                                new() { IdProducto = idProducto, Cantidad = 3 }
                            }
                        });

                    Assert.True(venta.Exito);
                    Assert.NotNull(venta.CodigoVenta);
                    Assert.Equal(300m, venta.Subtotal);
                    Assert.Equal(300m, venta.MontoTotal);
                    Assert.Equal(7, LeerStock(cadena, idProducto));
                    Assert.Equal(
                        EstadoVenta.Registrada,
                        LeerEstadoVenta(cadena, venta.IdVenta));

                    ResultadoVenta anulada = await fachada.AnularVentaAsync(
                        venta.IdVenta,
                        "Prueba de integración");

                    Assert.True(anulada.Exito);
                    Assert.Equal(10, LeerStock(cadena, idProducto));
                    Assert.Equal(
                        EstadoVenta.Anulada,
                        LeerEstadoVenta(cadena, venta.IdVenta));
                }
            }
            finally
            {
                Limpiar(cadena, idProducto, idCliente);
            }
        }

        [Fact]
        public async Task RegistrarVenta_Real_StockInsuficiente_NoPersisteAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            int idCategoria = ObtenerPrimerId(cadena, "categoria", "IdCategoria");
            int idMarca = ObtenerPrimerId(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = CrearProducto(
                cadena, idCategoria, idMarca, precio: 100m, stock: 2);
            int idCliente = CrearCliente(cadena);

            try
            {
                (FachadaVenta fachada, MySqlConexionTransaccional conexion) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexion)
                {
                    ResultadoVenta venta = await fachada.RegistrarVentaAsync(
                        new SolicitudVenta
                        {
                            IdCliente = idCliente,
                            MetodoPago = "Efectivo",
                            Lineas = new List<LineaVentaSolicitada>
                            {
                                new() { IdProducto = idProducto, Cantidad = 5 }
                            }
                        });

                    Assert.False(venta.Exito);
                    Assert.Equal(2, LeerStock(cadena, idProducto));
                    Assert.Equal(0, ContarVentasDeCliente(cadena, idCliente));
                }
            }
            finally
            {
                Limpiar(cadena, idProducto, idCliente);
            }
        }

        [Fact]
        public async Task RegistrarVenta_Real_Idempotente_NoDuplicaAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            int idCategoria = ObtenerPrimerId(cadena, "categoria", "IdCategoria");
            int idMarca = ObtenerPrimerId(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = CrearProducto(
                cadena, idCategoria, idMarca, precio: 100m, stock: 10);
            int idCliente = CrearCliente(cadena);

            try
            {
                (FachadaVenta fachada, MySqlConexionTransaccional conexion) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexion)
                {
                    SolicitudVenta solicitud = new()
                    {
                        IdCliente = idCliente,
                        MetodoPago = "Efectivo",
                        ClaveIdempotencia = Guid.NewGuid().ToString("N"),
                        Lineas = new List<LineaVentaSolicitada>
                        {
                            new() { IdProducto = idProducto, Cantidad = 2 }
                        }
                    };

                    ResultadoVenta primero = await fachada.RegistrarVentaAsync(solicitud);
                    ResultadoVenta segundo = await fachada.RegistrarVentaAsync(solicitud);

                    Assert.True(primero.Exito);
                    Assert.True(segundo.Exito);
                    Assert.True(segundo.Idempotente);
                    Assert.Equal(primero.IdVenta, segundo.IdVenta);
                    Assert.Equal(1, ContarVentasDeCliente(cadena, idCliente));
                    Assert.Equal(8, LeerStock(cadena, idProducto));
                }
            }
            finally
            {
                Limpiar(cadena, idProducto, idCliente);
            }
        }

        [Fact]
        public async Task RegistrarVenta_Real_Concurrente_SoloUnaGana()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            int idCategoria = ObtenerPrimerId(cadena, "categoria", "IdCategoria");
            int idMarca = ObtenerPrimerId(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = CrearProducto(
                cadena, idCategoria, idMarca, precio: 100m, stock: 1);
            int idCliente = CrearCliente(cadena);

            try
            {
                (FachadaVenta fachadaA, MySqlConexionTransaccional conexionA) =
                    CrearFachada(cadena, idEmpleado: 1);
                (FachadaVenta fachadaB, MySqlConexionTransaccional conexionB) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexionA)
                using (conexionB)
                {
                    SolicitudVenta solicitud = new()
                    {
                        IdCliente = idCliente,
                        MetodoPago = "Efectivo",
                        Lineas = new List<LineaVentaSolicitada>
                        {
                            new() { IdProducto = idProducto, Cantidad = 1 }
                        }
                    };

                    Task<ResultadoVenta> tareaA = fachadaA.RegistrarVentaAsync(solicitud);
                    Task<ResultadoVenta> tareaB = fachadaB.RegistrarVentaAsync(solicitud);

                    ResultadoVenta[] resultados =
                        await Task.WhenAll(tareaA, tareaB);

                    int exitos = resultados.Count(r => r.Exito);

                    Assert.Equal(1, exitos);
                    Assert.Equal(0, LeerStock(cadena, idProducto));
                    Assert.Equal(1, ContarVentasDeCliente(cadena, idCliente));
                }
            }
            finally
            {
                Limpiar(cadena, idProducto, idCliente);
            }
        }

        [Fact]
        public async Task RegistrarVenta_Real_Concurrente_CodigosUnicosAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            int idCategoria = ObtenerPrimerId(cadena, "categoria", "IdCategoria");
            int idMarca = ObtenerPrimerId(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = CrearProducto(
                cadena, idCategoria, idMarca, precio: 100m, stock: 2);
            int idCliente = CrearCliente(cadena);

            try
            {
                (FachadaVenta fachadaA, MySqlConexionTransaccional conexionA) =
                    CrearFachada(cadena, idEmpleado: 1);
                (FachadaVenta fachadaB, MySqlConexionTransaccional conexionB) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexionA)
                using (conexionB)
                {
                    SolicitudVenta solicitudA = new()
                    {
                        IdCliente = idCliente,
                        MetodoPago = "Efectivo",
                        Lineas = new List<LineaVentaSolicitada>
                        {
                            new() { IdProducto = idProducto, Cantidad = 1 }
                        }
                    };

                    SolicitudVenta solicitudB = new()
                    {
                        IdCliente = idCliente,
                        MetodoPago = "Efectivo",
                        Lineas = new List<LineaVentaSolicitada>
                        {
                            new() { IdProducto = idProducto, Cantidad = 1 }
                        }
                    };

                    Task<ResultadoVenta> tareaA = fachadaA.RegistrarVentaAsync(solicitudA);
                    Task<ResultadoVenta> tareaB = fachadaB.RegistrarVentaAsync(solicitudB);

                    ResultadoVenta[] resultados =
                        await Task.WhenAll(tareaA, tareaB);

                    Assert.Equal(2, resultados.Count(r => r.Exito));
                    Assert.Equal(0, LeerStock(cadena, idProducto));

                    List<int> ids = resultados.Select(r => r.IdVenta).ToList();
                    List<string?> codigos = resultados
                        .Select(r => r.CodigoVenta)
                        .ToList();

                    Assert.Equal(2, ids.Distinct().Count());
                    Assert.All(codigos, c => Assert.False(string.IsNullOrWhiteSpace(c)));
                    Assert.Equal(2, codigos.Distinct().Count());
                }
            }
            finally
            {
                Limpiar(cadena, idProducto, idCliente);
            }
        }

        [Fact]
        public async Task AnularVenta_Real_Concurrente_RestauraStockUnaVezAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            int idCategoria = ObtenerPrimerId(cadena, "categoria", "IdCategoria");
            int idMarca = ObtenerPrimerId(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = CrearProducto(
                cadena, idCategoria, idMarca, precio: 100m, stock: 10);
            int idCliente = CrearCliente(cadena);

            try
            {
                int idVenta;

                (FachadaVenta fachadaRegistro, MySqlConexionTransaccional conexionRegistro) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexionRegistro)
                {
                    ResultadoVenta venta = await fachadaRegistro.RegistrarVentaAsync(
                        new SolicitudVenta
                        {
                            IdCliente = idCliente,
                            MetodoPago = "Efectivo",
                            Lineas = new List<LineaVentaSolicitada>
                            {
                                new() { IdProducto = idProducto, Cantidad = 4 }
                            }
                        });

                    Assert.True(venta.Exito);
                    idVenta = venta.IdVenta;
                    Assert.Equal(6, LeerStock(cadena, idProducto));
                }

                (FachadaVenta fachadaA, MySqlConexionTransaccional conexionA) =
                    CrearFachada(cadena, idEmpleado: 1);
                (FachadaVenta fachadaB, MySqlConexionTransaccional conexionB) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexionA)
                using (conexionB)
                {
                    Task<ResultadoVenta> tareaA =
                        fachadaA.AnularVentaAsync(idVenta, "Anulación concurrente A");
                    Task<ResultadoVenta> tareaB =
                        fachadaB.AnularVentaAsync(idVenta, "Anulación concurrente B");

                    ResultadoVenta[] resultados =
                        await Task.WhenAll(tareaA, tareaB);

                    Assert.Equal(1, resultados.Count(r => r.Exito));
                    Assert.Equal(10, LeerStock(cadena, idProducto));
                    Assert.Equal(
                        EstadoVenta.Anulada,
                        LeerEstadoVenta(cadena, idVenta));
                }
            }
            finally
            {
                Limpiar(cadena, idProducto, idCliente);
            }
        }

        private static (FachadaVenta, MySqlConexionTransaccional) CrearFachada(
            string cadena,
            int idEmpleado)
        {
            FabricaConexionPruebas fabrica = new(cadena);
            MySqlConexionTransaccional conexion = new(fabrica);

            SelectorDescuento selector = new(new IPoliticaDescuento[]
            {
                new DescuentoPorMonto(montoMinimo: 1000m, porcentaje: 5m),
                new DescuentoPorCantidad(cantidadMinima: 12, porcentaje: 3m)
            });

            FachadaVenta fachada = new(
                new ClienteRepositoryCreator(conexion),
                new VentaRepositoryCreator(conexion),
                new DetalleVentaRepositoryCreator(conexion),
                new InventarioRepositoryCreator(conexion),
                new EmpleadoVentaRepositoryCreator(conexion),
                new MySqlUnidadDeTrabajo(conexion),
                selector,
                new ContextoUsuarioFake(idEmpleado));

            return (fachada, conexion);
        }

        private static MySqlConnection Abrir(string cadena)
        {
            MySqlConnection conexion = new(cadena);
            conexion.Open();
            return conexion;
        }

        private static int EjecutarEscalar(string cadena, string sql, params (string, object)[] parametros)
        {
            using MySqlConnection conexion = Abrir(cadena);
            using MySqlCommand comando = new(sql, conexion);

            foreach ((string nombre, object valor) in parametros)
            {
                comando.Parameters.AddWithValue(nombre, valor);
            }

            object? resultado = comando.ExecuteScalar();

            return resultado is null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }

        private static void Ejecutar(string cadena, string sql, params (string, object)[] parametros)
        {
            using MySqlConnection conexion = Abrir(cadena);
            using MySqlCommand comando = new(sql, conexion);

            foreach ((string nombre, object valor) in parametros)
            {
                comando.Parameters.AddWithValue(nombre, valor);
            }

            comando.ExecuteNonQuery();
        }

        private static int ObtenerPrimerId(string cadena, string tabla, string columna)
        {
            return EjecutarEscalar(
                cadena,
                $"SELECT {columna} FROM {tabla} WHERE Estado = 1 ORDER BY {columna} LIMIT 1");
        }

        private static int CrearProducto(
            string cadena,
            int idCategoria,
            int idMarca,
            decimal precio,
            int stock)
        {
            using MySqlConnection conexion = Abrir(cadena);

            using MySqlCommand comando = new(@"
                INSERT INTO producto
                (
                    IdCategoria, Codigo, Nombre, Marca, UnidadMedida,
                    PrecioVenta, CantidadStock, Estado, IdEmpleadoResponsable
                )
                VALUES
                (
                    @categoria, @codigo, @nombre, @marca, 'Unidad',
                    @precio, @stock, 1, 1
                )", conexion);

            comando.Parameters.AddWithValue("@categoria", idCategoria);
            comando.Parameters.AddWithValue("@codigo", $"T{Guid.NewGuid():N}"[..12]);
            comando.Parameters.AddWithValue("@nombre", $"Producto prueba {Guid.NewGuid():N}"[..24]);
            comando.Parameters.AddWithValue("@marca", idMarca);
            comando.Parameters.AddWithValue("@precio", precio);
            comando.Parameters.AddWithValue("@stock", stock);

            comando.ExecuteNonQuery();

            return Convert.ToInt32(comando.LastInsertedId);
        }

        private static int CrearCliente(string cadena)
        {
            using MySqlConnection conexion = Abrir(cadena);

            using MySqlCommand comando = new(@"
                INSERT INTO cliente
                (
                    Nombre, Apellido, Telefono, Estado, IdEmpleadoResponsable
                )
                VALUES
                (
                    @nombre, 'Prueba', '0000-0000', 1, 1
                )", conexion);

            comando.Parameters.AddWithValue("@nombre", $"Cliente {Guid.NewGuid():N}"[..20]);

            comando.ExecuteNonQuery();

            return Convert.ToInt32(comando.LastInsertedId);
        }

        private static int LeerStock(string cadena, int idProducto)
        {
            return EjecutarEscalar(
                cadena,
                "SELECT CantidadStock FROM producto WHERE IdProducto = @id",
                ("@id", idProducto));
        }

        private static string LeerEstadoVenta(string cadena, int idVenta)
        {
            using MySqlConnection conexion = Abrir(cadena);
            using MySqlCommand comando = new(
                "SELECT EstadoVenta FROM venta WHERE IdVenta = @id", conexion);

            comando.Parameters.AddWithValue("@id", idVenta);

            return comando.ExecuteScalar()?.ToString() ?? string.Empty;
        }

        private static int ContarVentasDeCliente(string cadena, int idCliente)
        {
            return EjecutarEscalar(
                cadena,
                "SELECT COUNT(*) FROM venta WHERE IdCliente = @id",
                ("@id", idCliente));
        }

        private static void Limpiar(string cadena, int idProducto, int idCliente)
        {
            using MySqlConnection conexion = Abrir(cadena);

            using (MySqlCommand borrarDetalle = new(
                "DELETE FROM detalle_venta WHERE IdProducto = @id", conexion))
            {
                borrarDetalle.Parameters.AddWithValue("@id", idProducto);
                borrarDetalle.ExecuteNonQuery();
            }

            using (MySqlCommand borrarVentas = new(
                "DELETE FROM venta WHERE IdCliente = @id", conexion))
            {
                borrarVentas.Parameters.AddWithValue("@id", idCliente);
                borrarVentas.ExecuteNonQuery();
            }

            using (MySqlCommand borrarProducto = new(
                "DELETE FROM producto WHERE IdProducto = @id", conexion))
            {
                borrarProducto.Parameters.AddWithValue("@id", idProducto);
                borrarProducto.ExecuteNonQuery();
            }

            using (MySqlCommand borrarCliente = new(
                "DELETE FROM cliente WHERE IdCliente = @id", conexion))
            {
                borrarCliente.Parameters.AddWithValue("@id", idCliente);
                borrarCliente.ExecuteNonQuery();
            }
        }
    }
}
