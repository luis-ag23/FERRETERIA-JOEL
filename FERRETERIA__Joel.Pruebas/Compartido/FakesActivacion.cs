using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Pruebas.Compartido
{
    /// <summary>
    /// Repositorio falso de empleados. Hereda de la implementación real y
    /// sustituye (métodos virtuales) el acceso a la base de datos, incluidas
    /// las operaciones del flujo de activación por correo.
    /// </summary>
    public sealed class EmpleadoRepositorioFake : MySqlEmpleadoRepository
    {
        private readonly List<Empleado> _empleados = new();

        public EmpleadoRepositorioFake() : base(null!)
        {
        }

        public void Agregar(Empleado empleado)
        {
            empleado.IdEmpleado = _empleados.Count + 1;
            _empleados.Add(empleado);
        }

        public override Task<List<Empleado>> ObtenerTodosAsync()
        {
            return Task.FromResult(_empleados.ToList());
        }

        public override Task<List<Empleado>> ObtenerTodosSinCredencialesAsync()
        {
            return Task.FromResult(_empleados.ToList());
        }

        public override Task<int> InsertarAsync(Empleado empleado)
        {
            empleado.IdEmpleado = _empleados.Count + 1;
            _empleados.Add(empleado);
            return Task.FromResult(empleado.IdEmpleado);
        }

        public override Task<Empleado?> ObtenerPorCorreoAsync(string correo)
        {
            return Task.FromResult(
                _empleados.FirstOrDefault(e => e.CorreoElectronico == correo));
        }

        public override Task<Empleado?> ObtenerPorCorreoConCredencialesAsync(
            string correo)
        {
            return Task.FromResult(
                _empleados.FirstOrDefault(e => e.CorreoElectronico == correo));
        }

        public override Task<Empleado?> ObtenerPorCorreoSinCredencialesAsync(
            string correo)
        {
            return Task.FromResult(
                _empleados.FirstOrDefault(e => e.CorreoElectronico == correo));
        }

        public override Task<bool> ExistePorCorreoAsync(string correo)
        {
            return Task.FromResult(
                _empleados.Any(e => e.CorreoElectronico == correo));
        }

        public override Task<bool> ActualizarEstadoAsync(
            int idEmpleado,
            sbyte nuevoEstado,
            int idEmpleadoResponsable)
        {
            Empleado? empleado =
                _empleados.FirstOrDefault(e => e.IdEmpleado == idEmpleado);

            if (empleado is null || empleado.Estado == nuevoEstado)
            {
                return Task.FromResult(false);
            }

            empleado.Estado = nuevoEstado;
            empleado.IdEmpleadoResponsable = idEmpleadoResponsable;

            return Task.FromResult(true);
        }

        public override Task<Empleado?> ObtenerPorTokenActivacionAsync(
            string token)
        {
            return Task.FromResult(
                _empleados.FirstOrDefault(e => e.TokenActivacion == token));
        }

        public override Task<List<Empleado>> ObtenerPendientesActivacionAsync()
        {
            return Task.FromResult(_empleados
                .Where(e => e.Estado
                    == FERRETERIA__Joel.Dominio.Constantes.EstadoEmpleado
                        .PendienteActivacion)
                .ToList());
        }

        public override Task ReasignarTokenActivacionAsync(
            int id,
            string token,
            DateTime expira)
        {
            Empleado? empleado =
                _empleados.FirstOrDefault(e => e.IdEmpleado == id);

            if (empleado is not null)
            {
                empleado.TokenActivacion = token;
                empleado.TokenExpira = expira;
                empleado.TokenUsado = false;
            }

            return Task.CompletedTask;
        }

        public override Task ActualizarContrasenaAsync(
            int id,
            string contrasena)
        {
            Empleado? empleado =
                _empleados.FirstOrDefault(e => e.IdEmpleado == id);

            if (empleado is not null)
            {
                empleado.Contrasena = contrasena;
            }

            return Task.CompletedTask;
        }

        public override Task ActivarCuentaAsync(
            int id,
            string contrasena,
            sbyte estadoActivo)
        {
            Empleado? empleado =
                _empleados.FirstOrDefault(e => e.IdEmpleado == id);

            if (empleado is not null)
            {
                empleado.Contrasena = contrasena;
                empleado.TokenUsado = true;
                empleado.Estado = estadoActivo;
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Enviador de correo falso: registra los envíos en memoria para poder
    /// verificar que el alta disparó el correo de activación.
    /// </summary>
    public sealed class EnviadorCorreoFake : IEnviadorCorreo
    {
        public List<(string Destinatario, string Enlace)> Envios { get; } = new();

        public Task EnviarActivacionAsync(
            string destinatario,
            string nombreEmpleado,
            string enlaceActivacion)
        {
            Envios.Add((destinatario, enlaceActivacion));
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Constructor de enlace falso: devuelve una URL predecible y permite
    /// simular el modo desarrollo.
    /// </summary>
    public sealed class ConstructorEnlaceFake : IConstructorEnlaceActivacion
    {
        public ConstructorEnlaceFake(bool esDesarrollo = true)
        {
            EsEnlaceDeDesarrollo = esDesarrollo;
        }

        public bool EsEnlaceDeDesarrollo { get; }

        public string Construir(string token)
        {
            return $"https://localhost/ActivarCuenta?token={token}";
        }
    }
}
