using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Conexion
{
    /// <summary>
    /// Contexto de conexión compartido por request. Permite que varios
    /// repositorios ejecuten sus comandos sobre la MISMA conexión y la
    /// MISMA transacción MySQL. La unidad de trabajo abre, confirma o
    /// revierte; los repositorios solo la consumen.
    /// </summary>
    public interface IConexionTransaccional : IDisposable
    {
        MySqlConnection Conexion { get; }

        MySqlTransaction? Transaccion { get; }

        void IniciarTransaccion();

        void Commit();

        void Rollback();
    }
}
