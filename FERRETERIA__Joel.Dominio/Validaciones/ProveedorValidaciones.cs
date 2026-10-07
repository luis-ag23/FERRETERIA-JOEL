using System.Text.RegularExpressions;
using FERRETERIA__Joel.Dominio.ConfiguracionValidacion;

namespace FERRETERIA__Joel.Dominio.Validaciones
{
    public class ProveedorValidaciones
    {
        private static readonly Regex FormatoTelefono =
            new(@"^[5-8][0-9]{7}$", RegexOptions.Compiled);
        private static readonly Regex FormatoCorreo =
            new(@"^(?=.{1,254}$)(?=.{6,64}@)[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?(?:\.[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*\.[A-Za-z]{2,24}$", RegexOptions.Compiled);
        public bool EsRazonSocialValida(string? razonSocial)
        {
            return ValidacionesTexto.EsNombreValido(
                razonSocial,
                ConfiguracionProveedor.RazonSocialMaxLength);
        }

        public bool EsNombreComercialValido(string? nombreComercial)
        {
            return ValidacionesTexto.EsNombreValido(
                nombreComercial,
                ConfiguracionProveedor.NombreComercialMaxLength);
        }

        public bool EsNombreContactoValido(string? nombreContacto)
        {
            return ValidacionesTexto.EsNombreValido(
                nombreContacto,
                ConfiguracionProveedor.NombreContactoMaxLength);
        }

        public bool EsTelefonoValido(string? telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
            {
                return false;
            }

            string limpio = telefono.Trim();

            return FormatoTelefono.IsMatch(limpio);
        }

        public bool EsCorreoValido(string? correoElectronico)
        {
            if (string.IsNullOrWhiteSpace(correoElectronico))
            {
                return true;
            }

            return FormatoCorreo.IsMatch(correoElectronico.Trim());
        }

        public bool EsDireccionValida(string? direccion)
        {
            return string.IsNullOrWhiteSpace(direccion)
                || direccion.Trim().Length <= ConfiguracionProveedor.DireccionMaxLength;
        }

        public bool EsEmpleadoValido(int idEmpleadoResponsable)
        {
            return idEmpleadoResponsable >= ConfiguracionProveedor.EmpleadoMinimo;
        }
    }

}