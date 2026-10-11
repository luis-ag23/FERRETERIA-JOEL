using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    // Registro de clientes: valida credenciales, verifica el correo único, hashea la contraseña y persiste. (extra: ExisteCorreo)
    public class ServicioRegistroCliente
    {
        private readonly RepositoryCreator<IClienteRepository> _creador;
        private readonly IHasheadorContrasena _hasheador;
        private readonly RegistroCredencialesValidador _validador = new();
        private IClienteRepository? _repositorio;

        public ServicioRegistroCliente(
            RepositoryCreator<IClienteRepository> creador,
            IHasheadorContrasena hasheador)
        {
            _creador = creador;
            _hasheador = hasheador;
        }

        private IClienteRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public ResultadoRegistro Registrar(RegistroClienteSolicitud solicitud)
        {
            Dictionary<string, string> erroresCampo =
                _validador.Validar(solicitud);

            if (!erroresCampo.ContainsKey(
                    nameof(RegistroClienteSolicitud.CorreoElectronico))
                && Repositorio.ExisteCorreo(solicitud.CorreoElectronico.Trim()))
            {
                erroresCampo[
                    nameof(RegistroClienteSolicitud.CorreoElectronico)] =
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
