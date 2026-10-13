namespace FERRETERIA__Joel.Infraestructura.Factories
{
    /// <summary>
    /// Base del patrón Factory Method para repositorios. Cada creador concreto
    /// produce un repositorio sobre la conexión/transacción del request. El
    /// producto es siempre uno de los contratos genéricos del dominio
    /// (<c>IRepository&lt;T&gt;</c> o <c>IGestionarRepository&lt;T&gt;</c>).
    /// </summary>
    public abstract class RepositoryCreator<TContrato>
    {
        public abstract TContrato CreateRepository();
    }
}
