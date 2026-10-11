using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    // Bitácora de sesiones: registra los eventos de inicio y cierre de sesión
    // con el identificador del usuario, su nombre, su tipo y la fecha/hora.
    public class ServicioBitacora
    {
        private const string AccionLogin = "login";

        private const string AccionLogout = "logout";

        private readonly RepositoryCreator<IBitacoraRepository> _creador;
        private IBitacoraRepository? _repositorio;

        public ServicioBitacora(
            RepositoryCreator<IBitacoraRepository> creador)
        {
            _creador = creador;
        }

        private IBitacoraRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public void RegistrarLogin(
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario)
        {
            Registrar(AccionLogin, idUsuario, nombreUsuario, tipoUsuario);
        }

        public void RegistrarLogout(
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario)
        {
            Registrar(AccionLogout, idUsuario, nombreUsuario, tipoUsuario);
        }

        private void Registrar(
            string accion,
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario)
        {
            Repositorio.Registrar(
                accion,
                idUsuario,
                nombreUsuario,
                string.IsNullOrWhiteSpace(tipoUsuario)
                    ? RolesSistema.Usuario
                    : tipoUsuario);
        }
    }
}
