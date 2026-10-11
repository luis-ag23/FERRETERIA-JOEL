namespace FERRETERIA__Joel.Dominio.Puertos
{
    public abstract class RepositoryCreator<TContrato>
    {
        public abstract TContrato CreateRepository();
    }
}
