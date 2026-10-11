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

        public Task IniciarAsync()
        {
            return _conexionTransaccional.IniciarTransaccionAsync();
        }

        public Task ConfirmarAsync()
        {
            return _conexionTransaccional.ConfirmarAsync();
        }

        public Task RevertirAsync()
        {
            return _conexionTransaccional.RevertirAsync();
        }
    }
}
