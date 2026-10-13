using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    // Bitácora de sesiones: registra los eventos de inicio y cierre de sesión
    // con el identificador del usuario, su nombre, su tipo y la fecha/hora.
    public class ServicioBitacora
    {
        private const string AccionLogin = "login";

        private const string AccionLogout = "logout";

        private readonly MySqlBitacoraRepository _repositorio;

        public ServicioBitacora(MySqlBitacoraRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task RegistrarLoginAsync(
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario)
        {
            await RegistrarAsync(
                AccionLogin,
                idUsuario,
                nombreUsuario,
                tipoUsuario);
        }

        public async Task RegistrarLogoutAsync(
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario)
        {
            await RegistrarAsync(
                AccionLogout,
                idUsuario,
                nombreUsuario,
                tipoUsuario);
        }

        private async Task RegistrarAsync(
            string accion,
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario)
        {
            await _repositorio.RegistrarAsync(
                accion,
                idUsuario,
                nombreUsuario,
                string.IsNullOrWhiteSpace(tipoUsuario)
                    ? RolesSistema.EmpleadoVenta
                    : tipoUsuario);
        }
    }
}

