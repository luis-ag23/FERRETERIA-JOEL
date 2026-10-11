using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Dominio.Validaciones;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    // Inicio de sesión unificado: busca primero en empleados y luego en
    // clientes, verifica la contraseña con el hasheador y comprueba que la
    // cuenta esté activa. Distingue el tipo de acceso por el rol resultante.
    public class ServicioInicioSesion
    {
        private const string MensajeCredenciales =
            "Correo o contraseña incorrectos.";

        private const string MensajeFormato =
            "Ingrese un correo y una contraseña válidos.";

        private const string MensajeInactivo =
            "La cuenta está inactiva. Contacte al administrador.";

        private readonly RepositoryCreator<IEmpleadoRepository> _creadorEmpleado;
        private readonly RepositoryCreator<IClienteRepository> _creadorCliente;
        private readonly IHasheadorContrasena _hasheador;
        private readonly CredencialesValidaciones _credenciales = new();
        private IEmpleadoRepository? _repositorioEmpleado;
        private IClienteRepository? _repositorioCliente;

        public ServicioInicioSesion(
            RepositoryCreator<IEmpleadoRepository> creadorEmpleado,
            RepositoryCreator<IClienteRepository> creadorCliente,
            IHasheadorContrasena hasheador)
        {
            _creadorEmpleado = creadorEmpleado;
            _creadorCliente = creadorCliente;
            _hasheador = hasheador;
        }

        private IEmpleadoRepository RepositorioEmpleado =>
            _repositorioEmpleado ??= _creadorEmpleado.CreateRepository();

        private IClienteRepository RepositorioCliente =>
            _repositorioCliente ??= _creadorCliente.CreateRepository();

        public ResultadoInicioSesion Iniciar(InicioSesionSolicitud solicitud)
        {
            string correo = solicitud.CorreoElectronico?.Trim() ?? string.Empty;
            string contrasena = solicitud.Contrasena ?? string.Empty;

            if (!_credenciales.EsCorreoValido(correo)
                || string.IsNullOrEmpty(contrasena))
            {
                return new ResultadoInicioSesion
                {
                    Exitoso = false,
                    Mensaje = MensajeFormato
                };
            }

            Empleado? empleado = RepositorioEmpleado.ObtenerPorCorreo(correo);

            if (empleado is not null)
            {
                return AutenticarEmpleado(empleado, contrasena);
            }

            Cliente? cliente = RepositorioCliente.ObtenerPorCorreo(correo);

            if (cliente is not null)
            {
                return AutenticarCliente(cliente, contrasena);
            }

            return new ResultadoInicioSesion
            {
                Exitoso = false,
                Mensaje = MensajeCredenciales
            };
        }

        private ResultadoInicioSesion AutenticarEmpleado(
            Empleado empleado,
            string contrasena)
        {
            if (!Coincide(contrasena, empleado.Contrasena, out bool rehash))
            {
                return CredencialesInvalidas();
            }

            if (empleado.Estado != 1)
            {
                return new ResultadoInicioSesion
                {
                    Exitoso = false,
                    Mensaje = MensajeInactivo
                };
            }

            if (rehash)
            {
                RepositorioEmpleado.ActualizarContrasena(
                    empleado.IdEmpleado,
                    _hasheador.Hashear(contrasena));
            }

            return new ResultadoInicioSesion
            {
                Exitoso = true,
                Usuario = new SesionUsuario
                {
                    Id = empleado.IdEmpleado,
                    NombreCompleto =
                        $"{empleado.Nombre} {empleado.Apellido}".Trim(),
                    CorreoElectronico =
                        empleado.CorreoElectronico ?? string.Empty,
                    Rol = RolesSistema.Empleado
                }
            };
        }

        private ResultadoInicioSesion AutenticarCliente(
            Cliente cliente,
            string contrasena)
        {
            if (!Coincide(contrasena, cliente.Contrasena, out bool rehash))
            {
                return CredencialesInvalidas();
            }

            if (cliente.Estado != 1)
            {
                return new ResultadoInicioSesion
                {
                    Exitoso = false,
                    Mensaje = MensajeInactivo
                };
            }

            if (rehash)
            {
                RepositorioCliente.ActualizarContrasena(
                    cliente.IdCliente,
                    _hasheador.Hashear(contrasena));
            }

            return new ResultadoInicioSesion
            {
                Exitoso = true,
                Usuario = new SesionUsuario
                {
                    Id = cliente.IdCliente,
                    NombreCompleto =
                        $"{cliente.Nombre} {cliente.Apellido}".Trim(),
                    CorreoElectronico =
                        cliente.CorreoElectronico ?? string.Empty,
                    Rol = RolesSistema.Usuario
                }
            };
        }

        private static ResultadoInicioSesion CredencialesInvalidas()
        {
            return new ResultadoInicioSesion
            {
                Exitoso = false,
                Mensaje = MensajeCredenciales
            };
        }

        // Verifica la contraseña. Detecta contraseñas heredadas guardadas en
        // texto plano (semillas) y marca que deben re-hashearse.
        private bool Coincide(
            string contrasena,
            string almacenada,
            out bool rehash)
        {
            rehash = false;

            if (string.IsNullOrEmpty(almacenada))
            {
                return false;
            }

            if (EsHashArgon2(almacenada))
            {
                try
                {
                    return _hasheador.Verificar(contrasena, almacenada);
                }
                catch
                {
                    return false;
                }
            }

            if (string.Equals(almacenada, contrasena, StringComparison.Ordinal))
            {
                rehash = true;
                return true;
            }

            return false;
        }

        private static bool EsHashArgon2(string valor)
        {
            return valor.StartsWith("$argon2", StringComparison.Ordinal);
        }
    }
}
