using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Conexion;

namespace FERRETERIA__Joel.Infraestructura
{
    public sealed class MySqlUnidadDeTrabajo : IUnidadDeTrabajo
    {
        private readonly IConexionTransaccional _conexionTransaccional;

        public MySqlUnidadDeTrabajo(IConexionTransaccional conexionTransaccional)
        {
            _conexionTransaccional = conexionTransaccional;
        }

        public void Iniciar()
        {
            _conexionTransaccional.IniciarTransaccion();
        }

        public void Confirmar()
        {
            _conexionTransaccional.Commit();
        }

        public void Revertir()
        {
            _conexionTransaccional.Rollback();
        }
    }
}
