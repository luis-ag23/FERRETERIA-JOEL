namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Puerto para coordinar una transacción de base de datos.
    /// La aplicación abre, confirma o revierte; la infraestructura
    /// provee la conexión y transacción reales.
    /// </summary>
    public interface IUnidadDeTrabajo
    {
        void Iniciar();

        void Confirmar();

        void Revertir();
    }
}
