namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IModificacionRepository<T>
    {
        void Actualizar(T entidad);

        void CambiarEstado(int id);

        int Count();
    }
}
