using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Dominio.Validaciones;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    // Inicio de sesión de empleado: valida el correo, recupera el empleado,
    // verifica la contraseña con el hasheador y comprueba que la cuenta esté activa.
    public class ServicioInicioSesionEmpleado
    {
        private const string MensajeCredenciales =
            "Correo o contraseña incorrectos.";

        private const string MensajeFormato =
            "Ingrese un correo y una contraseña válidos.";

        private const string MensajeInactivo =
            "La cuenta está inactiva. Contacte al administrador.";

        private readonly RepositoryCreator<IEmpleadoRepository> _creador;
        private readonly IHasheadorContrasena _hasheador;
        private readonly CredencialesValidaciones _credenciales = new();
        private IEmpleadoRepository? _repositorio;

        public ServicioInicioSesionEmpleado(
            RepositoryCreator<IEmpleadoRepository> creador,
            IHasheadorContrasena hasheador)
        {
            _creador = creador;
            _hasheador = hasheador;
        }

        private IEmpleadoRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

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

            Empleado? empleado = Repositorio.ObtenerPorCorreo(correo);

            if (empleado is null
                || string.IsNullOrEmpty(empleado.Contrasena)
                || !_hasheador.Verificar(contrasena, empleado.Contrasena))
            {
                return new ResultadoInicioSesion
                {
                    Exitoso = false,
                    Mensaje = MensajeCredenciales
                };
            }

            if (empleado.Estado != 1)
            {
                return new ResultadoInicioSesion
                {
                    Exitoso = false,
                    Mensaje = MensajeInactivo
                };
            }

            return new ResultadoInicioSesion
            {
                Exitoso = true,
                Empleado = empleado
            };
        }
    }
}
