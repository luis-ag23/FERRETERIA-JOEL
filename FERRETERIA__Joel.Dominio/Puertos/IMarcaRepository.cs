using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    public interface IMarcaRepository
    {
        List<Marca> ObtenerTodas();

        List<Marca> ObtenerActivas();
    }
}
