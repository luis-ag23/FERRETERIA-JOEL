using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    // Registro de empleados: valida credenciales, verifica el correo único,
    // hashea la contraseña y persiste el empleado con acceso al panel interno.
    public class ServicioRegistroEmpleado
    {
        // Los empleados autoregistrados tienen acceso a todas las funciones.
        private const string RolPorDefecto = "Administrador";

        private readonly RepositoryCreator<IEmpleadoRepository> _creador;
        private readonly IHasheadorContrasena _hasheador;
        private readonly RegistroCredencialesValidador _validador = new();
        private IEmpleadoRepository? _repositorio;

        public ServicioRegistroEmpleado(
            RepositoryCreator<IEmpleadoRepository> creador,
            IHasheadorContrasena hasheador)
        {
            _creador = creador;
            _hasheador = hasheador;
        }

        private IEmpleadoRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public ResultadoRegistro Registrar(RegistroEmpleadoSolicitud solicitud)
        {
            Dictionary<string, string> erroresCampo =
                _validador.Validar(solicitud);

            if (!erroresCampo.ContainsKey(
                    nameof(RegistroEmpleadoSolicitud.CorreoElectronico))
                && Repositorio.ObtenerPorCorreo(
                    solicitud.CorreoElectronico.Trim()) is not null)
            {
                erroresCampo[
                    nameof(RegistroEmpleadoSolicitud.CorreoElectronico)] =
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

            Empleado empleado = new()
            {
                Nombre = solicitud.Nombre.Trim(),
                Apellido = solicitud.Apellido.Trim(),
                Rol = RolPorDefecto,
                Telefono = solicitud.Telefono.Trim(),
                CorreoElectronico = solicitud.CorreoElectronico.Trim(),
                Contrasena = _hasheador.Hashear(solicitud.Contrasena),
                Estado = 1,
                FechaRegistro = DateTime.Now,
                IdEmpleadoResponsable = null
            };

            int id = Repositorio.Insertar(empleado);

            return new ResultadoRegistro
            {
                Exitoso = true,
                Id = id
            };
        }
    }
}
