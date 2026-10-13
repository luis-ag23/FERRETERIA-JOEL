using FERRETERIA__Joel.Dominio.ConfiguracionValidacion;
using FERRETERIA__Joel.Dominio.Validaciones;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Valida los datos del cliente que se registra desde la pantalla de
    /// venta (nombres, apellidos y teléfono) y arma un mensaje por campo.
    /// Reutiliza las reglas de <see cref="CredencialesValidaciones"/> para que
    /// empleados y clientes se validen con el mismo criterio.
    /// </summary>
    public class ClienteValidador
    {
        public const string CampoNombre = "Nombre";
        public const string CampoApellido = "Apellido";
        public const string CampoTelefono = "Telefono";

        private static readonly string MensajeNombre =
            $"Los nombres solo admiten letras y espacios (máximo {ConfiguracionCredenciales.NombreMaxLength} caracteres).";

        private static readonly string MensajeApellido =
            $"Los apellidos solo admiten letras y espacios (máximo {ConfiguracionCredenciales.ApellidoMaxLength} caracteres).";

        private const string MensajeTelefono =
            "El teléfono debe tener 8 dígitos y comenzar con 5, 6, 7 u 8.";

        private readonly CredencialesValidaciones _credenciales = new();

        public Dictionary<string, string> Validar(
            string nombre,
            string apellido,
            string telefono)
        {
            Dictionary<string, string> errores = new();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                errores[CampoNombre] = "Los nombres del cliente son obligatorios.";
            }
            else if (!_credenciales.EsNombreValido(nombre))
            {
                errores[CampoNombre] = MensajeNombre;
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                errores[CampoApellido] = "Los apellidos del cliente son obligatorios.";
            }
            else if (!_credenciales.EsApellidoValido(apellido))
            {
                errores[CampoApellido] = MensajeApellido;
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                errores[CampoTelefono] = "El teléfono del cliente es obligatorio.";
            }
            else if (!_credenciales.EsTelefonoValido(telefono))
            {
                errores[CampoTelefono] = MensajeTelefono;
            }

            return errores;
        }
    }
}