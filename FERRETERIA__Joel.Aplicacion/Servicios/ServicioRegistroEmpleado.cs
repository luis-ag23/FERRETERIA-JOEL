using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Factories;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Registro de empleados: valida credenciales, verifica que el correo sea
    /// único, hashea la contraseña con Argon2 y persiste el empleado con rol
    /// <c>EmpleadoVenta</c>.
    /// </summary>
    public class ServicioRegistroEmpleado
    {
        private const string RolPorDefecto = RolesSistema.EmpleadoVenta;

        private readonly RepositoryCreator<IRepository<Empleado>> _creador;
        private readonly IHasheadorContrasena _hasheador;
        private readonly RegistroCredencialesValidador _validador = new();
        private IRepository<Empleado>? _repositorio;

        public ServicioRegistroEmpleado(
            RepositoryCreator<IRepository<Empleado>> creador,
            IHasheadorContrasena hasheador)
        {
            _creador = creador;
            _hasheador = hasheador;
        }

        private IRepository<Empleado> Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public async Task<ResultadoRegistro> RegistrarAsync(
            IRegistroCredenciales solicitud)
        {
            Dictionary<string, string> erroresCampo =
                _validador.Validar(solicitud);

            if (!erroresCampo.ContainsKey(
                    nameof(IRegistroCredenciales.CorreoElectronico))
                && (await ((MySqlEmpleadoRepository)Repositorio)
                    .ObtenerPorCorreoAsync(solicitud.CorreoElectronico.Trim()))
                is not null)
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

            int id = await Repositorio.InsertarAsync(empleado);

            return new ResultadoRegistro
            {
                Exitoso = true,
                Id = id
            };
        }
    }
}
