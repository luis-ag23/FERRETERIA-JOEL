using MySql.Data.MySqlClient;

namespace FERRETERIA__Joel.Infraestructura.Conexion
{
    public sealed class MySqlConexionTransaccional : IConexionTransaccional
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private MySqlConnection? _conexion;
        private MySqlTransaction? _transaccion;
        private bool _disposed;

        public MySqlConexionTransaccional(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public MySqlConnection Conexion
        {
            get
            {
                if (_conexion is null)
                {
                    _conexion = _connectionFactory.CreateConnection();
                    _conexion.Open();
                }

                return _conexion;
            }
        }

        public MySqlTransaction? Transaccion => _transaccion;

        public void IniciarTransaccion()
        {
            if (_transaccion is not null)
            {
                throw new InvalidOperationException(
                    "Ya existe una transacción activa en esta conexión.");
            }

            _transaccion = Conexion.BeginTransaction();
        }

        public void Commit()
        {
            if (_transaccion is null)
            {
                return;
            }

            _transaccion.Commit();
            _transaccion.Dispose();
            _transaccion = null;
        }

        public void Rollback()
        {
            if (_transaccion is null)
            {
                return;
            }

            _transaccion.Rollback();
            _transaccion.Dispose();
            _transaccion = null;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _transaccion?.Dispose();
            _conexion?.Dispose();
            _transaccion = null;
            _conexion = null;
            _disposed = true;
        }
    }
}
