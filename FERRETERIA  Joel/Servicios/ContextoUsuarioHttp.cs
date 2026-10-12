using System.Security.Claims;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Servicios
{
    /// <summary>
    /// Resuelve la identidad del usuario autenticado a partir de las
    /// claims de la cookie. Permite que las páginas y servicios
    /// conozcan quién ejecuta la acción sin depender de datos del
    /// formulario.
    /// </summary>
    public class ContextoUsuarioHttp : IContextoUsuario
    {
        public const string ClaimNombreCompleto = "NombreCompleto";

        public const string ClaimIdSesion = "IdSesion";

        private readonly IHttpContextAccessor _accesor;

        public ContextoUsuarioHttp(IHttpContextAccessor accesor)
        {
            _accesor = accesor;
        }

        private ClaimsPrincipal? Usuario => _accesor.HttpContext?.User;

        public int IdEmpleado
        {
            get
            {
                string rol = Rol;

                bool esPersonal = string.Equals(
                    rol,
                    RolesSistema.Empleado,
                    StringComparison.Ordinal) ||
                    string.Equals(
                        rol,
                        RolesSistema.Administrador,
                        StringComparison.Ordinal);

                if (!esPersonal)
                {
                    return 0;
                }

                string? valor = Usuario?.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                return int.TryParse(valor, out int id) ? id : 0;
            }
        }

        public string NombreCompleto =>
            Usuario?.FindFirstValue(ClaimNombreCompleto)
            ?? Usuario?.Identity?.Name
            ?? string.Empty;

        public string Rol =>
            Usuario?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        public int IdSesion
        {
            get
            {
                string? valor = Usuario?.FindFirstValue(ClaimIdSesion);

                return int.TryParse(valor, out int id) ? id : 0;
            }
        }
    }
}
