using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Infraestructura.Seguridad;
using FERRETERIA__Joel.Pruebas.Compartido;
using Xunit;

namespace FERRETERIA__Joel.Pruebas.Servicios
{
    public class ServicioRegistroEmpleadoPruebas
    {
        private readonly EmpleadoRepositorioFake _repositorio = new();
        private readonly EnviadorCorreoFake _correo = new();
        private readonly ConstructorEnlaceFake _enlace = new();
        private readonly ServicioRegistroEmpleado _servicio;

        public ServicioRegistroEmpleadoPruebas()
        {
            _servicio = new ServicioRegistroEmpleado(
                new CreadorFake<FERRETERIA__Joel.Dominio.Puertos.IRepository<
                    FERRETERIA__Joel.Dominio.Entidades.Empleado>>(_repositorio),
                new RegistroCredencialesValidador(),
                _correo,
                _enlace,
                new ContextoUsuarioFake(idEmpleado: 1));
        }

        private static RegistroSolicitud SolicitudValida(
            string rol = RolesSistema.EmpleadoVenta)
        {
            return new RegistroSolicitud
            {
                Nombre = "Ana",
                Apellido = "Mamani",
                Telefono = "71234567",
                CorreoElectronico = "ana.mamani@ferreteria.com",
                Rol = rol
            };
        }

        [Fact]
        public async Task Registrar_EmpleadoVenta_CreaCuentaPendienteYEnviaCorreoAsync()
        {
            ResultadoRegistro resultado =
                await _servicio.RegistrarAsync(SolicitudValida());

            Assert.True(resultado.Exitoso);

            var empleados = await _repositorio.ObtenerTodosAsync();
            Assert.Single(empleados);

            var empleado = empleados[0];
            Assert.Equal(RolesSistema.EmpleadoVenta, empleado.Rol);
            Assert.Equal(EstadoEmpleado.PendienteActivacion, empleado.Estado);
            Assert.Null(empleado.Contrasena);
            Assert.False(empleado.TokenUsado);
            Assert.NotNull(empleado.TokenActivacion);
            Assert.NotNull(empleado.TokenExpira);
            Assert.Equal(1, empleado.IdEmpleadoResponsable);

            Assert.Single(_correo.Envios);
            Assert.Equal("ana.mamani@ferreteria.com", _correo.Envios[0].Destinatario);
        }

        [Fact]
        public async Task Registrar_Administrador_AsignaRolAdministradorAsync()
        {
            ResultadoRegistro resultado = await _servicio.RegistrarAsync(
                SolicitudValida(RolesSistema.Administrador));

            Assert.True(resultado.Exitoso);

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            Assert.Equal(RolesSistema.Administrador, empleado.Rol);
            Assert.Equal(EstadoEmpleado.PendienteActivacion, empleado.Estado);
        }

        [Fact]
        public async Task Registrar_CorreoDuplicado_FallaAsync()
        {
            await _servicio.RegistrarAsync(SolicitudValida());

            ResultadoRegistro resultado =
                await _servicio.RegistrarAsync(SolicitudValida());

            Assert.False(resultado.Exitoso);
            Assert.Contains(
                nameof(RegistroSolicitud.CorreoElectronico),
                resultado.ErroresCampo.Keys);
            Assert.Single(_correo.Envios);
        }

        [Fact]
        public async Task Registrar_RolInvalido_FallaAsync()
        {
            ResultadoRegistro resultado =
                await _servicio.RegistrarAsync(SolicitudValida("Supervisor"));

            Assert.False(resultado.Exitoso);
            Assert.Contains(
                nameof(RegistroSolicitud.Rol),
                resultado.ErroresCampo.Keys);
        }

        [Fact]
        public async Task Registrar_ExponeEnlaceSoloEnDesarrolloAsync()
        {
            ResultadoRegistro resultado =
                await _servicio.RegistrarAsync(SolicitudValida());

            Assert.True(resultado.Exitoso);
            Assert.NotNull(resultado.EnlaceActivacion);
        }

        [Fact]
        public async Task Registrar_NoExponeEnlaceConSmtpRealAsync()
        {
            var servicio = new ServicioRegistroEmpleado(
                new CreadorFake<FERRETERIA__Joel.Dominio.Puertos.IRepository<
                    FERRETERIA__Joel.Dominio.Entidades.Empleado>>(_repositorio),
                new RegistroCredencialesValidador(),
                _correo,
                new ConstructorEnlaceFake(esDesarrollo: false),
                new ContextoUsuarioFake(idEmpleado: 1));

            ResultadoRegistro resultado =
                await servicio.RegistrarAsync(SolicitudValida());

            Assert.True(resultado.Exitoso);
            Assert.Null(resultado.EnlaceActivacion);
        }

        [Fact]
        public async Task Reenviar_Pendiente_GeneraTokenNuevoYEnviaCorreoAsync()
        {
            await _servicio.RegistrarAsync(SolicitudValida());

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            string? tokenAnterior = empleado.TokenActivacion;

            // Simula que el enlace anterior venció.
            empleado.TokenExpira = DateTime.Now.AddMinutes(-5);

            ResultadoRegistro resultado =
                await _servicio.ReenviarActivacionAsync(empleado.IdEmpleado);

            Assert.True(resultado.Exitoso);
            Assert.Equal(empleado.IdEmpleado, resultado.Id);

            var actualizado = (await _repositorio.ObtenerTodosAsync())[0];
            Assert.NotEqual(tokenAnterior, actualizado.TokenActivacion);
            Assert.NotNull(actualizado.TokenExpira);
            Assert.True(actualizado.TokenExpira > DateTime.Now);
            Assert.False(actualizado.TokenUsado);
            Assert.Equal(
                EstadoEmpleado.PendienteActivacion,
                actualizado.Estado);

            Assert.Equal(2, _correo.Envios.Count);
            Assert.Equal(
                "ana.mamani@ferreteria.com",
                _correo.Envios[1].Destinatario);
        }

        [Fact]
        public async Task Reenviar_EmpleadoInexistente_FallaAsync()
        {
            ResultadoRegistro resultado =
                await _servicio.ReenviarActivacionAsync(999);

            Assert.False(resultado.Exitoso);
            Assert.Empty(_correo.Envios);
        }

        [Fact]
        public async Task Reenviar_CuentaActiva_FallaAsync()
        {
            await _servicio.RegistrarAsync(SolicitudValida());

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            empleado.Estado = EstadoEmpleado.Activo;

            ResultadoRegistro resultado =
                await _servicio.ReenviarActivacionAsync(empleado.IdEmpleado);

            Assert.False(resultado.Exitoso);
            Assert.Single(_correo.Envios);
        }

        [Fact]
        public async Task ObtenerPendientes_SoloDevuelveEstadoPendienteAsync()
        {
            await _servicio.RegistrarAsync(SolicitudValida());

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            Assert.Single(await _servicio.ObtenerPendientesAsync());

            empleado.Estado = EstadoEmpleado.Activo;
            Assert.Empty(await _servicio.ObtenerPendientesAsync());
        }

        [Fact]
        public async Task ReenviarPorCorreo_Pendiente_ReenviaEnlaceAsync()
        {
            await _servicio.RegistrarAsync(SolicitudValida());

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            string? tokenAnterior = empleado.TokenActivacion;
            empleado.TokenExpira = DateTime.Now.AddMinutes(-5);

            ResultadoRegistro resultado = await _servicio
                .ReenviarPorCorreoAsync("ana.mamani@ferreteria.com");

            Assert.True(resultado.Exitoso);
            Assert.Equal(empleado.IdEmpleado, resultado.Id);

            var actualizado = (await _repositorio.ObtenerTodosAsync())[0];
            Assert.NotEqual(tokenAnterior, actualizado.TokenActivacion);
            Assert.True(actualizado.TokenExpira > DateTime.Now);

            Assert.Equal(2, _correo.Envios.Count);
        }

        [Fact]
        public async Task ReenviarPorCorreo_NoExiste_FallaAsync()
        {
            ResultadoRegistro resultado =
                await _servicio.ReenviarPorCorreoAsync("nadie@ferreteria.com");

            Assert.False(resultado.Exitoso);
            Assert.Contains(
                "No existe una cuenta con ese correo.",
                resultado.Errores);
            Assert.Empty(_correo.Envios);
        }

        [Fact]
        public async Task ReenviarPorCorreo_CuentaActiva_FallaAsync()
        {
            await _servicio.RegistrarAsync(SolicitudValida());

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            empleado.Estado = EstadoEmpleado.Activo;

            ResultadoRegistro resultado = await _servicio
                .ReenviarPorCorreoAsync("ana.mamani@ferreteria.com");

            Assert.False(resultado.Exitoso);
            Assert.Contains(
                "La cuenta ya está activa; no requiere activación.",
                resultado.Errores);
            Assert.Single(_correo.Envios);
        }

        [Fact]
        public async Task EsCuentaPendiente_DetectaEstadoAsync()
        {
            await _servicio.RegistrarAsync(SolicitudValida());

            Assert.True(await _servicio.EsCuentaPendienteAsync(
                "ana.mamani@ferreteria.com"));
            Assert.False(await _servicio.EsCuentaPendienteAsync(
                "nadie@ferreteria.com"));

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            empleado.Estado = EstadoEmpleado.Activo;

            Assert.False(await _servicio.EsCuentaPendienteAsync(
                "ana.mamani@ferreteria.com"));
        }
    }

    public class ServicioActivacionCuentaPruebas
    {
        private readonly EmpleadoRepositorioFake _repositorio = new();
        private readonly ServicioActivacionCuenta _servicio;
        private readonly Argon2HasheadorContrasena _hasheador = new();

        public ServicioActivacionCuentaPruebas()
        {
            _servicio = new ServicioActivacionCuenta(
                _repositorio,
                _hasheador,
                new RegistroCredencialesValidador());
        }

        private void AgregarPendiente(
            string token = "token-valido",
            DateTime? expira = null,
            bool usado = false,
            sbyte estado = EstadoEmpleado.PendienteActivacion)
        {
            _repositorio.Agregar(new FERRETERIA__Joel.Dominio.Entidades.Empleado
            {
                Nombre = "Ana",
                Apellido = "Mamani",
                Rol = RolesSistema.EmpleadoVenta,
                Telefono = "71234567",
                CorreoElectronico = "ana@ferreteria.com",
                Contrasena = null,
                TokenActivacion = token,
                TokenExpira = expira ?? DateTime.Now.AddMinutes(10),
                TokenUsado = usado,
                Estado = estado,
                FechaRegistro = DateTime.Now
            });
        }

        [Fact]
        public async Task ValidarToken_Vigente_EsValidoAsync()
        {
            AgregarPendiente();

            ResultadoActivacion resultado =
                await _servicio.ValidarTokenAsync("token-valido");

            Assert.True(resultado.Exitoso);
            Assert.Equal("Ana Mamani", resultado.NombreEmpleado);
        }

        [Fact]
        public async Task ValidarToken_Vencido_EsInvalidoAsync()
        {
            AgregarPendiente(expira: DateTime.Now.AddMinutes(-1));

            ResultadoActivacion resultado =
                await _servicio.ValidarTokenAsync("token-valido");

            Assert.False(resultado.Exitoso);
        }

        [Fact]
        public async Task ValidarToken_Usado_EsInvalidoAsync()
        {
            AgregarPendiente(usado: true);

            ResultadoActivacion resultado =
                await _servicio.ValidarTokenAsync("token-valido");

            Assert.False(resultado.Exitoso);
        }

        [Fact]
        public async Task ValidarToken_Desconocido_EsInvalidoAsync()
        {
            ResultadoActivacion resultado =
                await _servicio.ValidarTokenAsync("no-existe");

            Assert.False(resultado.Exitoso);
        }

        [Fact]
        public async Task Activar_ConContrasenaValida_ActivaLaCuentaAsync()
        {
            AgregarPendiente();

            ResultadoActivacion resultado = await _servicio.ActivarAsync(
                new ActivacionSolicitud
                {
                    Token = "token-valido",
                    Contrasena = "Clave123!",
                    ConfirmarContrasena = "Clave123!"
                });

            Assert.True(resultado.Exitoso);
            Assert.Empty(resultado.ErroresCampo);

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            Assert.Equal(EstadoEmpleado.Activo, empleado.Estado);
            Assert.True(empleado.TokenUsado);
            Assert.NotNull(empleado.Contrasena);
            Assert.True(_hasheador.Verificar("Clave123!", empleado.Contrasena!));
        }

        [Fact]
        public async Task Activar_ConContrasenaDebil_NoActivaAsync()
        {
            AgregarPendiente();

            ResultadoActivacion resultado = await _servicio.ActivarAsync(
                new ActivacionSolicitud
                {
                    Token = "token-valido",
                    Contrasena = "debil",
                    ConfirmarContrasena = "debil"
                });

            Assert.True(resultado.Exitoso);
            Assert.Contains(
                nameof(ActivacionSolicitud.Contrasena),
                resultado.ErroresCampo.Keys);

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            Assert.Equal(EstadoEmpleado.PendienteActivacion, empleado.Estado);
            Assert.Null(empleado.Contrasena);
        }

        [Fact]
        public async Task Activar_ConfirmacionDistinta_NoActivaAsync()
        {
            AgregarPendiente();

            ResultadoActivacion resultado = await _servicio.ActivarAsync(
                new ActivacionSolicitud
                {
                    Token = "token-valido",
                    Contrasena = "Clave123!",
                    ConfirmarContrasena = "Clave124!"
                });

            Assert.Contains(
                nameof(ActivacionSolicitud.ConfirmarContrasena),
                resultado.ErroresCampo.Keys);
        }

        [Fact]
        public async Task Activar_ConTokenVencido_FallaAsync()
        {
            AgregarPendiente(expira: DateTime.Now.AddMinutes(-1));

            ResultadoActivacion resultado = await _servicio.ActivarAsync(
                new ActivacionSolicitud
                {
                    Token = "token-valido",
                    Contrasena = "Clave123!",
                    ConfirmarContrasena = "Clave123!"
                });

            Assert.False(resultado.Exitoso);

            var empleado = (await _repositorio.ObtenerTodosAsync())[0];
            Assert.Equal(EstadoEmpleado.PendienteActivacion, empleado.Estado);
        }
    }
}
