namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Puerto de cifrado de contraseñas. La aplicación depende de esta
    /// abstracción; la infraestructura decide el algoritmo (PBKDF2).
    /// </summary>
    public interface IPasswordHasher
    {
        string Hashear(string contrasena);

        bool Verificar(string contrasena, string hashAlmacenado);

        bool EsHashValido(string hashAlmacenado);
    }
}
