using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    // Registro de clientes (rol Usuario): valida las credenciales, verifica
    // que el correo no esté en uso, hashea la contraseña y persiste el
    // cliente con acceso de solo lectura al catálogo.
    public class ServicioRegistroUsuario
    {
        private readonly RepositoryCreator<IClienteRepository> _creador;
        private readonly IHasheadorContrasena _hasheador;
        private readonly RegistroCredencialesValidador _validador = new();
        private IClienteRepository? _repositorio;

        public ServicioRegistroUsuario(
            RepositoryCreator<IClienteRepository> creador,
            IHasheadorContrasena hasheador)
        {
            _creador = creador;
            _hasheador = hasheador;
        }

        private IClienteRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public ResultadoRegistro Registrar(IRegistroCredenciales solicitud)
        {
            Dictionary<string, string> erroresCampo =
                _validador.Validar(solicitud);

            if (!erroresCampo.ContainsKey(
                    nameof(IRegistroCredenciales.CorreoElectronico))
                && Repositorio.ExisteCorreo(
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

            Cliente cliente = new()
            {
                Nombre = solicitud.Nombre.Trim(),
                Apellido = solicitud.Apellido.Trim(),
                Telefono = solicitud.Telefono.Trim(),
                CorreoElectronico = solicitud.CorreoElectronico.Trim(),
                Contrasena = _hasheador.Hashear(solicitud.Contrasena),
                Estado = 1,
                FechaRegistro = DateTime.Now,
                IdClienteResponsable = null
            };

            int id = Repositorio.Insertar(cliente);

            return new ResultadoRegistro
            {
                Exitoso = true,
                Id = id
            };
        }
    }
}
