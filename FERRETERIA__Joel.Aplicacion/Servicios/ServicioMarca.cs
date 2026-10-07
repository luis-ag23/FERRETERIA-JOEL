using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    public class ServicioMarca
    {
        private readonly IMarcaRepository _repositorio;

        public ServicioMarca(IMarcaRepository repositorio)
        {
            _repositorio = repositorio;
        }

        public List<Marca> ObtenerTodas()
        {
            return _repositorio.ObtenerTodas();
        }

        public List<Marca> ObtenerActivas()
        {
            return _repositorio.ObtenerActivas();
        }
    }
}
