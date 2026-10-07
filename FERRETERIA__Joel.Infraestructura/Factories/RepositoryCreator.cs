namespace FERRETERIA__Joel.Infraestructura.Factories
{
    public abstract class RepositoryCreator<TContrato>
    {
        public abstract TContrato CreateRepository();
    }
}