namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IRepository<T>
    {
        List<T> ObtenerTodos();

        int Insertar(T entidad);
    }
}
