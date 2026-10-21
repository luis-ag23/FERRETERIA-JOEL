using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura;
using FERRETERIA__Joel.Infraestructura.Conexion;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Repositorios;
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

            int idCategoria = await ObtenerPrimerIdAsync(cadena, "categoria", "IdCategoria");
            int idMarca = await ObtenerPrimerIdAsync(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = await CrearProductoAsync(
                cadena, idCategoria, idMarca, precio: 100m, stock: 10);
            int idCliente = await CrearClienteAsync(cadena);

            try
            {
                (FachadaVenta fachada, FachadaAnulacion anulacion, MySqlConexionTransaccional conexion) =
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
                    Assert.Equal(300m, venta.MontoTotal);
                    Assert.Equal(7, await LeerStockAsync(cadena, idProducto));
                    Assert.Equal(
                        EstadoVenta.Registrada,
                        await LeerEstadoVentaAsync(cadena, venta.IdVenta));

                    ResultadoVenta anulada = await anulacion.AnularVentaAsync(
                        venta.IdVenta,
                        "Prueba de integración");

                    Assert.True(anulada.Exito);
                    Assert.Equal(10, await LeerStockAsync(cadena, idProducto));
                    Assert.Equal(
                        EstadoVenta.Anulada,
                        await LeerEstadoVentaAsync(cadena, venta.IdVenta));
                }
            }
            finally
            {
                await LimpiarAsync(cadena, idProducto, idCliente);
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

            int idCategoria = await ObtenerPrimerIdAsync(cadena, "categoria", "IdCategoria");
            int idMarca = await ObtenerPrimerIdAsync(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = await CrearProductoAsync(
                cadena, idCategoria, idMarca, precio: 100m, stock: 2);
            int idCliente = await CrearClienteAsync(cadena);

            try
            {
                (FachadaVenta fachada, FachadaAnulacion anulacion, MySqlConexionTransaccional conexion) =
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
                    Assert.Equal(2, await LeerStockAsync(cadena, idProducto));
                    Assert.Equal(0, await ContarVentasDeClienteAsync(cadena, idCliente));
                }
            }
            finally
            {
                await LimpiarAsync(cadena, idProducto, idCliente);
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

            int idCategoria = await ObtenerPrimerIdAsync(cadena, "categoria", "IdCategoria");
            int idMarca = await ObtenerPrimerIdAsync(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = await CrearProductoAsync(
                cadena, idCategoria, idMarca, precio: 100m, stock: 1);
            int idCliente = await CrearClienteAsync(cadena);

            try
            {
                (FachadaVenta fachadaA, _, MySqlConexionTransaccional conexionA) =
                    CrearFachada(cadena, idEmpleado: 1);
                (FachadaVenta fachadaB, _, MySqlConexionTransaccional conexionB) =
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
                    Assert.Equal(0, await LeerStockAsync(cadena, idProducto));
                    Assert.Equal(1, await ContarVentasDeClienteAsync(cadena, idCliente));
                }
            }
            finally
            {
                await LimpiarAsync(cadena, idProducto, idCliente);
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

            int idCategoria = await ObtenerPrimerIdAsync(cadena, "categoria", "IdCategoria");
            int idMarca = await ObtenerPrimerIdAsync(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = await CrearProductoAsync(
                cadena, idCategoria, idMarca, precio: 100m, stock: 2);
            int idCliente = await CrearClienteAsync(cadena);

            try
            {
                (FachadaVenta fachadaA, _, MySqlConexionTransaccional conexionA) =
                    CrearFachada(cadena, idEmpleado: 1);
                (FachadaVenta fachadaB, _, MySqlConexionTransaccional conexionB) =
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
                    Assert.Equal(0, await LeerStockAsync(cadena, idProducto));

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
                await LimpiarAsync(cadena, idProducto, idCliente);
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

            int idCategoria = await ObtenerPrimerIdAsync(cadena, "categoria", "IdCategoria");
            int idMarca = await ObtenerPrimerIdAsync(cadena, "marca", "IdMarca");

            if (idCategoria == 0 || idMarca == 0)
            {
                return;
            }

            int idProducto = await CrearProductoAsync(
                cadena, idCategoria, idMarca, precio: 100m, stock: 10);
            int idCliente = await CrearClienteAsync(cadena);

            try
            {
                int idVenta;

                (FachadaVenta fachadaRegistro, _, MySqlConexionTransaccional conexionRegistro) =
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
                    Assert.Equal(6, await LeerStockAsync(cadena, idProducto));
                }

                (_, FachadaAnulacion anulacionA, MySqlConexionTransaccional conexionA) =
                    CrearFachada(cadena, idEmpleado: 1);
                (_, FachadaAnulacion anulacionB, MySqlConexionTransaccional conexionB) =
                    CrearFachada(cadena, idEmpleado: 1);

                using (conexionA)
                using (conexionB)
                {
                    Task<ResultadoVenta> tareaA =
                        anulacionA.AnularVentaAsync(idVenta, "Anulación concurrente A");
                    Task<ResultadoVenta> tareaB =
                        anulacionB.AnularVentaAsync(idVenta, "Anulación concurrente B");

                    ResultadoVenta[] resultados =
                        await Task.WhenAll(tareaA, tareaB);

                    Assert.Equal(1, resultados.Count(r => r.Exito));
                    Assert.Equal(10, await LeerStockAsync(cadena, idProducto));
                    Assert.Equal(
                        EstadoVenta.Anulada,
                        await LeerEstadoVentaAsync(cadena, idVenta));
                }
            }
            finally
            {
                await LimpiarAsync(cadena, idProducto, idCliente);
            }
        }

        private static (
            FachadaVenta Venta,
            FachadaAnulacion Anulacion,
            MySqlConexionTransaccional Conexion) CrearFachada(
            string cadena,
            int idEmpleado)
        {
            FabricaConexionPruebas fabrica = new(cadena);
            MySqlConexionTransaccional conexion = new(fabrica);

            IContextoUsuario contexto = new ContextoUsuarioFake(idEmpleado);

            MySqlVentaRepository ventasRepositorio = new(conexion);
            MySqlDetalleVentaRepository detallesRepositorio = new(conexion);
            MySqlInventarioRepository inventarioRepositorio = new(conexion);
            MySqlClienteRepository clientesRepositorio = new(conexion);
            MySqlProductoRepository productosRepositorio = new(conexion);
            MySqlCategoriaRepository categoriasRepositorio = new(fabrica);
            RepositoryCreator<IRepository<Empleado>> creadorEmpleados = new EmpleadoRepositoryCreator(fabrica);
            MySqlUnidadDeTrabajo unidadDeTrabajo = new(conexion);

            FachadaActualizacionStock stock = new(inventarioRepositorio);

            ServicioCliente servicioCliente = new(clientesRepositorio, contexto);

            FachadaVenta venta = new(
                ventasRepositorio,
                detallesRepositorio,
                creadorEmpleados,
                unidadDeTrabajo,
                contexto,
                stock,
                servicioCliente,
                categoriasRepositorio);

            FachadaAnulacion anulacion = new(
                ventasRepositorio,
                detallesRepositorio,
                servicioCliente,
                unidadDeTrabajo,
                contexto,
                stock,
                productosRepositorio,
                categoriasRepositorio);

            return (venta, anulacion, conexion);
        }

        private static async Task<MySqlConnection> AbrirAsync(string cadena)
        {
            MySqlConnection conexion = new(cadena);
            await conexion.OpenAsync();
            return conexion;
        }

        private static async Task<int> EjecutarEscalarAsync(
            string cadena,
            string sql,
            params (string, object)[] parametros)
        {
            await using MySqlConnection conexion = await AbrirAsync(cadena);
            await using MySqlCommand comando = new(sql, conexion);

            foreach ((string nombre, object valor) in parametros)
            {
                comando.Parameters.AddWithValue(nombre, valor);
            }

            object? resultado = await comando.ExecuteScalarAsync();

            return resultado is null || resultado == DBNull.Value
                ? 0
                : Convert.ToInt32(resultado);
        }

        private static Task<int> ObtenerPrimerIdAsync(
            string cadena,
            string tabla,
            string columna)
        {
            return EjecutarEscalarAsync(
                cadena,
                $"SELECT {columna} FROM {tabla} WHERE Estado = 1 ORDER BY {columna} LIMIT 1");
        }

        private static async Task<int> CrearProductoAsync(
            string cadena,
            int idCategoria,
            int idMarca,
            decimal precio,
            int stock)
        {
            await using MySqlConnection conexion = await AbrirAsync(cadena);

            await using MySqlCommand comando = new(@"
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

            await comando.ExecuteNonQueryAsync();

            return Convert.ToInt32(comando.LastInsertedId);
        }

        private static async Task<int> CrearClienteAsync(string cadena)
        {
            await using MySqlConnection conexion = await AbrirAsync(cadena);

            await using MySqlCommand comando = new(@"
                INSERT INTO cliente
                (
                    Ci, Nombre, Apellido, Telefono, Estado, IdEmpleadoResponsable
                )
                VALUES
                (
                    @ci, @nombre, 'Prueba', '0000-0000', 1, 1
                )", conexion);

            comando.Parameters.AddWithValue("@ci", Random.Shared.Next(10000000, 99999999).ToString());
            comando.Parameters.AddWithValue("@nombre", $"Cliente {Guid.NewGuid():N}"[..20]);

            await comando.ExecuteNonQueryAsync();

            return Convert.ToInt32(comando.LastInsertedId);
        }

        private static Task<int> LeerStockAsync(string cadena, int idProducto)
        {
            return EjecutarEscalarAsync(
                cadena,
                "SELECT CantidadStock FROM producto WHERE IdProducto = @id",
                ("@id", idProducto));
        }

        private static async Task<string> LeerEstadoVentaAsync(string cadena, int idVenta)
        {
            await using MySqlConnection conexion = await AbrirAsync(cadena);
            await using MySqlCommand comando = new(
                "SELECT EstadoVenta FROM venta WHERE IdVenta = @id", conexion);

            comando.Parameters.AddWithValue("@id", idVenta);

            object? resultado = await comando.ExecuteScalarAsync();

            return resultado?.ToString() ?? string.Empty;
        }

        private static Task<int> ContarVentasDeClienteAsync(string cadena, int idCliente)
        {
            return EjecutarEscalarAsync(
                cadena,
                "SELECT COUNT(*) FROM venta WHERE IdCliente = @id",
                ("@id", idCliente));
        }

        private static async Task LimpiarAsync(string cadena, int idProducto, int idCliente)
        {
            await using MySqlConnection conexion = await AbrirAsync(cadena);

            await using (MySqlCommand borrarDetalle = new(
                "DELETE FROM detalle_venta WHERE IdProducto = @id", conexion))
            {
                borrarDetalle.Parameters.AddWithValue("@id", idProducto);
                await borrarDetalle.ExecuteNonQueryAsync();
            }

            await using (MySqlCommand borrarVentas = new(
                "DELETE FROM venta WHERE IdCliente = @id", conexion))
            {
                borrarVentas.Parameters.AddWithValue("@id", idCliente);
                await borrarVentas.ExecuteNonQueryAsync();
            }

            await using (MySqlCommand borrarProducto = new(
                "DELETE FROM producto WHERE IdProducto = @id", conexion))
            {
                borrarProducto.Parameters.AddWithValue("@id", idProducto);
                await borrarProducto.ExecuteNonQueryAsync();
            }

            await using (MySqlCommand borrarCliente = new(
                "DELETE FROM cliente WHERE IdCliente = @id", conexion))
            {
                borrarCliente.Parameters.AddWithValue("@id", idCliente);
                await borrarCliente.ExecuteNonQueryAsync();
            }
        }
    }
}