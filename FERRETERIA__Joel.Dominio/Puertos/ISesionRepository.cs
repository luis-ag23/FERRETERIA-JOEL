namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Bitácora de inicio y cierre de sesión (auditoría).
    /// </summary>
    public interface ISesionRepository
    {
        int Registrar(int idEmpleado, string accion);

        void Cerrar(int idSesion);
    }
}
