using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Servicio de bitácora de sesiones (inicio y cierre de sesión).
    /// </summary>
    public class ServicioSesion
    {
        private readonly RepositoryCreator<ISesionRepository> _creador;
        private ISesionRepository? _repositorio;

        public ServicioSesion(
            RepositoryCreator<ISesionRepository> creador)
        {
            _creador = creador;
        }

        private ISesionRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public int Registrar(int idEmpleado, string accion)
        {
            return Repositorio.Registrar(idEmpleado, accion);
        }

        public void Cerrar(int idSesion)
        {
            Repositorio.Cerrar(idSesion);
        }
    }
}
