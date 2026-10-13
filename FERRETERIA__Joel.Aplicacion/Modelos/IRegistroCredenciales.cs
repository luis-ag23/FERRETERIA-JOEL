namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Campos de credenciales comunes al alta de personal. Permite validar una
    /// sola vez sin depender de la entidad concreta.
    /// </summary>
    public interface IRegistroCredenciales
    {
        string Nombre { get; }

        string Apellido { get; }

        string Telefono { get; }

        string CorreoElectronico { get; }

        string Contrasena { get; }

        string ConfirmarContrasena { get; }
    }
}
