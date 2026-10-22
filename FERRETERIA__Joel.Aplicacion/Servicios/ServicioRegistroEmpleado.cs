using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Alta de personal autorizada por un administrador. Valida los datos,
    /// verifica que el correo sea único, asigna el rol elegido
    /// (<c>Administrador</c> o <c>EmpleadoVenta</c>) y crea la cuenta en estado
    /// <c>PendienteActivacion</c> con un token de activación. La contraseña
    /// queda nula y el enlace de activación se envía por correo (válido 10
    /// minutos).
    /// </summary>
    public class ServicioRegistroEmpleado
    {
        /// <summary>Vigencia del token de activación.</summary>
        public static readonly TimeSpan VigenciaToken =
            TimeSpan.FromMinutes(10);

        private readonly RepositoryCreator<IRepository<Empleado>> _creador;
        private readonly RegistroCredencialesValidador _validador;
        private readonly IEnviadorCorreo _enviadorCorreo;
        private readonly IConstructorEnlaceActivacion _constructorEnlace;
        private readonly IContextoUsuario _contextoUsuario;
        private IRepository<Empleado>? _repositorio;

        public ServicioRegistroEmpleado(
            RepositoryCreator<IRepository<Empleado>> creador,
            RegistroCredencialesValidador validador,
            IEnviadorCorreo enviadorCorreo,
            IConstructorEnlaceActivacion constructorEnlace,
            IContextoUsuario contextoUsuario)
        {
            _creador = creador;
            _validador = validador;
            _enviadorCorreo = enviadorCorreo;
            _constructorEnlace = constructorEnlace;
            _contextoUsuario = contextoUsuario;
        }

        private IRepository<Empleado> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        /// <summary>
        /// Acceso al repositorio con su contrato de personal: permite usar la
        /// proyección sin credenciales y las operaciones del flujo de
        /// activación.
        /// </summary>
        private MySqlEmpleadoRepository RepositorioEmpleado =>
            (MySqlEmpleadoRepository)Repositorio;

        public async Task<ResultadoRegistro> RegistrarAsync(
            IRegistroCredenciales solicitud)
        {
            Dictionary<string, string> erroresCampo =
                _validador.Validar(solicitud);

            if (!erroresCampo.ContainsKey(
                    nameof(IRegistroCredenciales.CorreoElectronico))
                && await RepositorioEmpleado.ExistePorCorreoAsync(
                    solicitud.CorreoElectronico.Trim()))
            {
                erroresCampo[
                    nameof(IRegistroCredenciales.CorreoElectronico)] =
                    "El correo ya está registrado.";
            }

            if (erroresCampo.Any())
            {
                return new ResultadoRegistro
                {
                    Exitoso = false,
                    ErroresCampo = erroresCampo
                };
            }

            string token = GeneradorTokenActivacion.Generar();

            Empleado empleado = new()
            {
                Nombre = solicitud.Nombre.Trim(),
                Apellido = solicitud.Apellido.Trim(),
                Rol = RolesSistema.Normalizar(solicitud.Rol),
                Telefono = solicitud.Telefono.Trim(),
                CorreoElectronico = solicitud.CorreoElectronico.Trim(),
                Contrasena = null,
                TokenActivacion = token,
                TokenExpira = DateTime.Now.Add(VigenciaToken),
                TokenUsado = false,
                Estado = EstadoEmpleado.PendienteActivacion,
                FechaRegistro = DateTime.Now,
                IdEmpleadoResponsable = _contextoUsuario.IdEmpleado > 0
                    ? _contextoUsuario.IdEmpleado
                    : null
            };

            int id = await Repositorio.InsertarAsync(empleado);

            string enlace = _constructorEnlace.Construir(token);

            await _enviadorCorreo.EnviarActivacionAsync(
                empleado.CorreoElectronico!,
                $"{empleado.Nombre} {empleado.Apellido}".Trim(),
                enlace);

            return new ResultadoRegistro
            {
                Exitoso = true,
                Id = id,
                EnlaceActivacion = _constructorEnlace
                    .EsEnlaceDeDesarrollo
                    ? enlace
                    : null
            };
        }

        /// <summary>
        /// Reenvía el enlace de activación a un empleado pendiente. Genera un
        /// token nuevo (invalida el anterior), reinicia su vigencia y vuelve a
        /// enviar el correo. Se usa cuando el enlace anterior venció o se
        /// perdió.
        /// </summary>
        public async Task<ResultadoRegistro> ReenviarActivacionAsync(
            int idEmpleado)
        {
            MySqlEmpleadoRepository repositorio = RepositorioEmpleado;

            Empleado? empleado = (await repositorio.ObtenerPendientesActivacionAsync())
                .FirstOrDefault(e => e.IdEmpleado == idEmpleado);

            if (empleado is null)
            {
                Empleado? existente = (await repositorio.ObtenerTodosAsync())
                    .FirstOrDefault(e => e.IdEmpleado == idEmpleado);

                return new ResultadoRegistro
                {
                    Exitoso = false,
                    Errores = new List<string>
                    {
                        existente is null
                            ? "El empleado no existe."
                            : "La cuenta ya está activa; no requiere activación."
                    }
                };
            }

            if (string.IsNullOrWhiteSpace(empleado.CorreoElectronico))
            {
                return new ResultadoRegistro
                {
                    Exitoso = false,
                    Errores = new List<string>
                    {
                        "El empleado no tiene correo registrado."
                    }
                };
            }

            string token = GeneradorTokenActivacion.Generar();

            await repositorio.ReasignarTokenActivacionAsync(
                empleado.IdEmpleado,
                token,
                DateTime.Now.Add(VigenciaToken));

            string enlace = _constructorEnlace.Construir(token);

            await _enviadorCorreo.EnviarActivacionAsync(
                empleado.CorreoElectronico,
                $"{empleado.Nombre} {empleado.Apellido}".Trim(),
                enlace);

            return new ResultadoRegistro
            {
                Exitoso = true,
                Id = empleado.IdEmpleado,
                EnlaceActivacion = _constructorEnlace
                    .EsEnlaceDeDesarrollo
                    ? enlace
                    : null
            };
        }

        /// <summary>
        /// Reenvía el enlace de activación a una cuenta pendiente identificada
        /// por su correo. Se usa cuando el administrador intenta registrar un
        /// correo ya existente: en lugar de solo rechazarlo, se le ofrece
        /// reenviar el enlace. Si el correo no existe o la cuenta ya está
        /// activa, devuelve el error correspondiente.
        /// </summary>
        public async Task<ResultadoRegistro> ReenviarPorCorreoAsync(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return new ResultadoRegistro
                {
                    Exitoso = false,
                    Errores = new List<string>
                    {
                        "Debe indicar un correo electrónico."
                    }
                };
            }

            Empleado? empleado =
                await RepositorioEmpleado
                    .ObtenerPorCorreoSinCredencialesAsync(correo.Trim());

            if (empleado is null)
            {
                return new ResultadoRegistro
                {
                    Exitoso = false,
                    Errores = new List<string>
                    {
                        "No existe una cuenta con ese correo."
                    }
                };
            }

            if (empleado.Estado != EstadoEmpleado.PendienteActivacion)
            {
                return new ResultadoRegistro
                {
                    Exitoso = false,
                    Errores = new List<string>
                    {
                        "La cuenta ya está activa; no requiere activación."
                    }
                };
            }

            return await ReenviarActivacionAsync(empleado.IdEmpleado);
        }

        /// <summary>
        /// Indica si existe una cuenta con ese correo y está pendiente de
        /// activación. Sirve para decidir si se ofrece el reenvío del enlace
        /// tras un intento de registro con correo duplicado.
        /// </summary>
        public async Task<bool> EsCuentaPendienteAsync(string? correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return false;
            }

            Empleado? empleado =
                await RepositorioEmpleado
                    .ObtenerPorCorreoSinCredencialesAsync(correo.Trim());

            return empleado is not null
                && empleado.Estado == EstadoEmpleado.PendienteActivacion;
        }

        /// <summary>
        /// Lista los empleados pendientes de activación para mostrarlos al
        /// administrador con la opción de reenviar el enlace.
        /// </summary>
        public async Task<List<Empleado>> ObtenerPendientesAsync()
        {
            return await RepositorioEmpleado.ObtenerPendientesActivacionAsync();
        }
    }
}
