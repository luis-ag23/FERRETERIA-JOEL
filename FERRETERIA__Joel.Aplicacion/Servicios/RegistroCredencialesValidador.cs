using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.ConfiguracionValidacion;
using FERRETERIA__Joel.Dominio.Validaciones;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Valida las credenciales comunes al registro (nombres, teléfono, correo,
    /// contraseña y confirmación) y arma los mensajes por campo.
    /// </summary>
    public class RegistroCredencialesValidador
    {
        private const string MensajeNombre =
            "El nombre es obligatorio, debe tener máximo 100 caracteres y solo admite letras y espacios.";

        private const string MensajeApellido =
            "El apellido es obligatorio, debe tener máximo 100 caracteres y solo admite letras y espacios.";

        private const string MensajeTelefono =
            "El teléfono debe tener 8 dígitos y comenzar con 5, 6, 7 u 8.";

        private const string MensajeCorreo =
            "El correo debe tener un formato válido, por ejemplo correo@ejemplo.com.";

        private const string MensajeConfirmacion =
            "La confirmación de la contraseña no coincide.";

        private static readonly string MensajeContrasena =
            $"La contraseña debe tener entre {ConfiguracionCredenciales.ContrasenaLongitudMinima} y "
            + $"{ConfiguracionCredenciales.ContrasenaLongitudMaxima} caracteres e incluir al menos una "
            + "mayúscula, una minúscula, un número y un símbolo.";

        private readonly CredencialesValidaciones _credenciales = new();

        public Dictionary<string, string> Validar(IRegistroCredenciales solicitud)
        {
            Dictionary<string, string> errores = new();

            if (!_credenciales.EsNombreValido(solicitud.Nombre))
            {
                errores[nameof(IRegistroCredenciales.Nombre)] = MensajeNombre;
            }

            if (!_credenciales.EsApellidoValido(solicitud.Apellido))
            {
                errores[nameof(IRegistroCredenciales.Apellido)] = MensajeApellido;
            }

            if (!_credenciales.EsTelefonoValido(solicitud.Telefono))
            {
                errores[nameof(IRegistroCredenciales.Telefono)] = MensajeTelefono;
            }

            if (!_credenciales.EsCorreoValido(solicitud.CorreoElectronico))
            {
                errores[nameof(IRegistroCredenciales.CorreoElectronico)] = MensajeCorreo;
            }

            if (!_credenciales.EsContrasenaValida(solicitud.Contrasena))
            {
                errores[nameof(IRegistroCredenciales.Contrasena)] = MensajeContrasena;
            }
            else if (!_credenciales.EsConfirmacionValida(
                solicitud.Contrasena,
                solicitud.ConfirmarContrasena))
            {
                errores[nameof(IRegistroCredenciales.ConfirmarContrasena)] = MensajeConfirmacion;
            }

            return errores;
        }
    }
}
