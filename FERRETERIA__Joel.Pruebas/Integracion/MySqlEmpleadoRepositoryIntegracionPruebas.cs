using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Infraestructura.Repositorios;
using FERRETERIA__Joel.Pruebas.Compartido;
using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Pruebas.Integracion
{
    /// <summary>
    /// Pruebas de integración reales del repositorio de personal. Se
    /// ejecutan solo si la variable de entorno FERRETERIA_TEST_CONNECTION
    /// apunta a una base de desarrollo; en caso contrario se omiten sin
    /// fallar.
    ///
    /// <para>
    /// Cubren las tres proyecciones del repositorio y la baja lógica: que el
    /// listado no devuelva credenciales, que la autenticación sí las obtenga,
    /// y que dar de baja una cuenta la desactive sin borrarla.
    /// </para>
    /// </summary>
    public class MySqlEmpleadoRepositoryIntegracionPruebas
    {
        private const string VariableConexion = "FERRETERIA_TEST_CONNECTION";

        private static string? CadenaConexion =>
            Environment.GetEnvironmentVariable(VariableConexion);

        [Fact]
        public async Task Listar_Publico_DevuelvePersonal_SinCredencialesAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            MySqlEmpleadoRepository repositorio = CrearRepositorio(cadena);

            List<Empleado> personal = await repositorio.ObtenerTodosSinCredencialesAsync();

            Assert.NotEmpty(personal);

            foreach (Empleado empleado in personal)
            {
                // La proyección pública no puede filtrar credenciales.
                Assert.Null(empleado.Contrasena);
                Assert.Null(empleado.TokenActivacion);
                Assert.Null(empleado.TokenExpira);
                Assert.False(empleado.TokenUsado);

                // Pero sí trae los datos que la interfaz necesita.
                Assert.NotEmpty(empleado.Nombre);
                Assert.NotEmpty(empleado.Rol);
            }
        }

        [Fact]
        public async Task Listar_Publico_NoDevuelveElHashDeLaContrasenaAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            MySqlEmpleadoRepository repositorio = CrearRepositorio(cadena);

            Empleado? conCredenciales =
                await repositorio.ObtenerPorCorreoConCredencialesAsync(
                    "joel.rios@ferreteria.com");

            if (conCredenciales is null)
            {
                return;
            }

            Assert.False(string.IsNullOrWhiteSpace(conCredenciales.Contrasena));

            Empleado? sinCredenciales =
                await repositorio.ObtenerPorCorreoSinCredencialesAsync(
                    "joel.rios@ferreteria.com");

            Assert.NotNull(sinCredenciales);
            Assert.Null(sinCredenciales!.Contrasena);
            Assert.Equal(conCredenciales.IdEmpleado, sinCredenciales.IdEmpleado);
            Assert.Equal(
                conCredenciales.CorreoElectronico,
                sinCredenciales.CorreoElectronico);
        }

        [Fact]
        public async Task ExistePorCorreo_UsaProyeccionSinCredencialesAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            MySqlEmpleadoRepository repositorio = CrearRepositorio(cadena);

            Assert.True(await repositorio.ExistePorCorreoAsync(
                "joel.rios@ferreteria.com"));

            Assert.False(await repositorio.ExistePorCorreoAsync(
                "no.existe.esta.cuenta@ferreteria.com"));
        }

        [Fact]
        public async Task BajaLogica_DesactivaLaCuenta_SinBorrarlaAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            MySqlEmpleadoRepository repositorio = CrearRepositorio(cadena);

            (int idEmpleado, string correo) = await CrearEmpleadoAsync(cadena);

            try
            {
                bool aplicada = await repositorio.ActualizarEstadoAsync(
                    idEmpleado,
                    EstadoEmpleado.Inactivo,
                    idEmpleadoResponsable: 1);

                Assert.True(aplicada);

                // La cuenta sigue existiendo: la baja es lógica.
                Empleado? dadoDeBaja =
                    await repositorio.ObtenerPorCorreoSinCredencialesAsync(correo);

                Assert.NotNull(dadoDeBaja);
                Assert.Equal(EstadoEmpleado.Inactivo, dadoDeBaja!.Estado);

                // Y una cuenta inactiva ya no puede volver a ese mismo estado.
                Assert.False(await repositorio.ActualizarEstadoAsync(
                    idEmpleado,
                    EstadoEmpleado.Inactivo,
                    idEmpleadoResponsable: 1));

                // La reactivación la deja pendiente de activación.
                bool reactivada = await repositorio.ActualizarEstadoAsync(
                    idEmpleado,
                    EstadoEmpleado.PendienteActivacion,
                    idEmpleadoResponsable: 1);

                Assert.True(reactivada);

                Empleado? reactivado =
                    await repositorio.ObtenerPorCorreoSinCredencialesAsync(correo);

                Assert.NotNull(reactivado);
                Assert.Equal(
                    EstadoEmpleado.PendienteActivacion,
                    reactivado!.Estado);
            }
            finally
            {
                await EliminarEmpleadoAsync(cadena, idEmpleado);
            }
        }

        [Fact]
        public async Task ServicioPersonal_NoPermiteDarDeBajaLaPropiaCuentaAsync()
        {
            string? cadena = CadenaConexion;

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return;
            }

            MySqlEmpleadoRepository repositorio = CrearRepositorio(cadena);

            ContextoUsuarioFake contexto = new(1);

            ServicioPersonal servicio = new(repositorio, contexto);

            ResultadoPersonal resultado = await servicio.DarDeBajaAsync(1);

            Assert.False(resultado.Exitoso);
            Assert.Contains("su propia cuenta", resultado.Mensaje);

            // La cuenta del administrador sigue activa.
            Empleado? administrador =
                await repositorio.ObtenerPorCorreoSinCredencialesAsync(
                    "joel.rios@ferreteria.com");

            Assert.NotNull(administrador);
            Assert.Equal(EstadoEmpleado.Activo, administrador!.Estado);
        }

        private static MySqlEmpleadoRepository CrearRepositorio(string cadena)
        {
            return new MySqlEmpleadoRepository(new FabricaConexionPruebas(cadena));
        }

        private static async Task<(int IdEmpleado, string Correo)> CrearEmpleadoAsync(
            string cadena)
        {
            await using MySqlConnection conexion = new(cadena);
            await conexion.OpenAsync();

            string correo =
                $"baja.{Guid.NewGuid():N}"[..24] + "@ferreteria.com";

            await using MySqlCommand comando = new(@"
                INSERT INTO empleado
                (
                    Nombre, Apellido, Rol, Telefono, CorreoElectronico,
                    Contrasena, Estado, IdEmpleadoResponsable
                )
                VALUES
                (
                    'Empleado', 'Prueba', 'EmpleadoVenta', '0000-0000', @correo,
                    NULL, 1, 1
                )", conexion);

            comando.Parameters.AddWithValue("@correo", correo);

            await comando.ExecuteNonQueryAsync();

            return (Convert.ToInt32(comando.LastInsertedId), correo);
        }

        private static async Task EliminarEmpleadoAsync(string cadena, int idEmpleado)
        {
            await using MySqlConnection conexion = new(cadena);
            await conexion.OpenAsync();

            await using MySqlCommand comando = new(
                "DELETE FROM empleado WHERE IdEmpleado = @id", conexion);

            comando.Parameters.AddWithValue("@id", idEmpleado);

            await comando.ExecuteNonQueryAsync();
        }
    }
}
