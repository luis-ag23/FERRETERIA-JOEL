using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        bool ExisteCorreo(string correo);
    }
}
