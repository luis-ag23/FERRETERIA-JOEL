using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Gestión del personal: consulta del listado y del ciclo de vida de las
    /// cuentas (baja lógica y reactivación).
    ///
    /// <para>
    /// La baja es <em>lógica</em>: la fila no se borra nunca, se cambia su
    /// estado a <see cref="EstadoEmpleado.Inactivo"/>. De ese modo el
    /// histórico de ventas, los comprobantes y la auditoría siguen siendo
    /// trazables, y la cuenta deja de poder iniciar sesión porque el proceso
    /// de autenticación exige el estado activo.
    /// </para>
    /// </summary>
    public class ServicioPersonal
    {
        private readonly IRepositoryEmpleado _repositorio;
        private readonly IContextoUsuario _contextoUsuario;

        public ServicioPersonal(
            IRepositoryEmpleado repositorio,
            IContextoUsuario contextoUsuario)
        {
            _repositorio = repositorio;
            _contextoUsuario = contextoUsuario;
        }

        /// <summary>
        /// Lista el personal con la proyección sin credenciales: la contraseña
        /// y el token de activación nunca llegan a la interfaz.
        /// </summary>
        public async Task<List<Empleado>> ListarAsync()
        {
            return await _repositorio.ObtenerTodosSinCredencialesAsync();
        }

        /// <summary>
        /// Da de baja a un empleado: la cuenta pasa a estado inactivo sin
        /// borrado físico y deja de poder iniciar sesión.
        /// </summary>
        public async Task<ResultadoPersonal> DarDeBajaAsync(int idEmpleado)
        {
            return await CambiarEstadoAsync(
                idEmpleado,
                EstadoEmpleado.Inactivo,
                "La cuenta fue dada de baja.");
        }

        /// <summary>
        /// Reactiva una cuenta dada de baja. La cuenta vuelve a estado
        /// pendiente de activación: el empleado debe definir de nuevo su
        /// contraseña mediante el enlace de activación.
        /// </summary>
        public async Task<ResultadoPersonal> ReactivarAsync(int idEmpleado)
        {
            return await CambiarEstadoAsync(
                idEmpleado,
                EstadoEmpleado.PendienteActivacion,
                "La cuenta fue reactivada y quedó pendiente de activación.");
        }

        private async Task<ResultadoPersonal> CambiarEstadoAsync(
            int idEmpleado,
            sbyte nuevoEstado,
            string mensajeExito)
        {
            if (idEmpleado <= 0)
            {
                return new ResultadoPersonal
                {
                    Exitoso = false,
                    Mensaje = "El empleado indicado no es válido."
                };
            }

            if (idEmpleado == _contextoUsuario.IdEmpleado)
            {
                return new ResultadoPersonal
                {
                    Exitoso = false,
                    Mensaje =
                        "No puede cambiar el estado de su propia cuenta."
                };
            }

            bool aplicado = await _repositorio.ActualizarEstadoAsync(
                idEmpleado,
                nuevoEstado,
                _contextoUsuario.IdEmpleado);

            if (!aplicado)
            {
                return new ResultadoPersonal
                {
                    Exitoso = false,
                    IdEmpleado = idEmpleado,
                    Mensaje =
                        "El empleado no existe o ya se encuentra en ese estado."
                };
            }

            return new ResultadoPersonal
            {
                Exitoso = true,
                IdEmpleado = idEmpleado,
                Estado = nuevoEstado,
                Mensaje = mensajeExito
            };
        }
    }
}
