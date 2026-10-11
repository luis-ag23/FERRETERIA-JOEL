using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Servicio de autenticación. Verifica credenciales contra el
    /// repositorio dedicado y, si detecta contraseñas heredadas en
    /// texto plano, las migra a PBKDF2 en el primer inicio de sesión.
    /// Un empleado inactivo nunca se autentica porque el repositorio
    /// filtra Estado = 1.
    /// </summary>
    public class ServicioAutenticacion
    {
        private const string MensajeCredencialesInvalidas =
            "Correo o contraseña incorrectos.";

        private readonly RepositoryCreator<IAutenticacionRepository> _creador;
        private readonly IPasswordHasher _hasher;
        private IAutenticacionRepository? _repositorio;

        public ServicioAutenticacion(
            RepositoryCreator<IAutenticacionRepository> creador,
            IPasswordHasher hasher)
        {
            _creador = creador;
            _hasher = hasher;
        }

        private IAutenticacionRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public ResultadoAutenticacion Autenticar(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                return ResultadoAutenticacion.Fallido(
                    MensajeCredencialesInvalidas);
            }

            Empleado? empleado = Repositorio.ObtenerPorCorreo(correo.Trim());

            if (empleado is null)
            {
                return ResultadoAutenticacion.Fallido(
                    MensajeCredencialesInvalidas);
            }

            if (!CoincideContrasena(empleado, contrasena))
            {
                return ResultadoAutenticacion.Fallido(
                    MensajeCredencialesInvalidas);
            }

            return ResultadoAutenticacion.Correcto(empleado);
        }

        private bool CoincideContrasena(Empleado empleado, string contrasena)
        {
            string almacenada = empleado.Contrasena ?? string.Empty;

            if (_hasher.EsHashValido(almacenada))
            {
                return _hasher.Verificar(contrasena, almacenada);
            }

            bool coincide = string.Equals(
                almacenada,
                contrasena,
                StringComparison.Ordinal);

            if (coincide)
            {
                Repositorio.ActualizarContrasena(
                    empleado.IdEmpleado,
                    _hasher.Hashear(contrasena));
            }

            return coincide;
        }
    }
}
