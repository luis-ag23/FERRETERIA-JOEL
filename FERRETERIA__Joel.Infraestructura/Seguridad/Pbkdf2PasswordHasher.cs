using System.Security.Cryptography;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Infraestructura.Seguridad
{
    /// <summary>
    /// Implementación PBKDF2-HMAC-SHA256 con sal aleatoria por usuario.
    /// Formato almacenado: pbkdf2$&lt;iteraciones&gt;$&lt;salBase64&gt;$&lt;hashBase64&gt;.
    /// No se guarda nunca la contraseña en texto plano.
    /// </summary>
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const string Prefijo = "pbkdf2";
        private const int Iteraciones = 100_000;
        private const int TamanoSal = 16;
        private const int TamanoHash = 32;

        public string Hashear(string contrasena)
        {
            if (contrasena is null)
            {
                throw new ArgumentNullException(nameof(contrasena));
            }

            byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password: contrasena,
                salt: sal,
                iterations: Iteraciones,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: TamanoHash);

            return string.Join(
                "$",
                Prefijo,
                Iteraciones.ToString(),
                Convert.ToBase64String(sal),
                Convert.ToBase64String(hash));
        }

        public bool Verificar(string contrasena, string hashAlmacenado)
        {
            if (string.IsNullOrWhiteSpace(contrasena) ||
                !IntentarDescomponer(
                    hashAlmacenado,
                    out int iteraciones,
                    out byte[] sal,
                    out byte[] hashEsperado))
            {
                return false;
            }

            byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                password: contrasena,
                salt: sal,
                iterations: iteraciones,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(
                hashCalculado,
                hashEsperado);
        }

        public bool EsHashValido(string hashAlmacenado)
        {
            return IntentarDescomponer(
                hashAlmacenado,
                out _,
                out _,
                out _);
        }

        private static bool IntentarDescomponer(
            string? hashAlmacenado,
            out int iteraciones,
            out byte[] sal,
            out byte[] hash)
        {
            iteraciones = 0;
            sal = Array.Empty<byte>();
            hash = Array.Empty<byte>();

            if (string.IsNullOrWhiteSpace(hashAlmacenado))
            {
                return false;
            }

            string[] partes = hashAlmacenado.Split('$');

            if (partes.Length != 4 || partes[0] != Prefijo)
            {
                return false;
            }

            if (!int.TryParse(partes[1], out iteraciones) ||
                iteraciones <= 0)
            {
                return false;
            }

            try
            {
                sal = Convert.FromBase64String(partes[2]);
                hash = Convert.FromBase64String(partes[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            return sal.Length > 0 && hash.Length > 0;
        }
    }
}
