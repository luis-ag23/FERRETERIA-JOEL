namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Puerto de la bitácora de sesiones. Registra cada inicio y cierre de
    /// sesión con el usuario afectado, su tipo y la fecha/hora del evento.
    /// </summary>
    public interface IBitacoraRepository
    {
        int Registrar(
            string accion,
            int? idUsuario,
            string? nombreUsuario,
            string tipoUsuario);
    }
}
