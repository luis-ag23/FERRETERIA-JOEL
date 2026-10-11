namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IGestionarRepository<T> : IRepository<T>
    {
        void Actualizar(T entidad);

        void CambiarEstado(int id, int idEmpleadoResponsable);

        List<T> MostrarActivos();
    }
}
