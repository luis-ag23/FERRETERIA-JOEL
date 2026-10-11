using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Caso de uso de clientes. Permite dar de alta un cliente desde el
    /// propio flujo de venta (sin abandonar la pantalla) y consultar el
    /// catálogo de clientes activos. Todas las operaciones son asíncronas.
    /// </summary>
    public class ServicioCliente
    {
        private readonly RepositoryCreator<IClienteRepository> _creador;
        private readonly IContextoUsuario _contextoUsuario;
        private IClienteRepository? _repositorio;

        public ServicioCliente(
            RepositoryCreator<IClienteRepository> creador,
            IContextoUsuario contextoUsuario)
        {
            _creador = creador;
            _contextoUsuario = contextoUsuario;
        }

        private IClienteRepository Repositorio =>
            _repositorio ??= _creador.CreateRepository();

        public Task<List<Cliente>> ListarAsync()
        {
            return Repositorio.ObtenerTodosAsync();
        }

        public Task<List<Cliente>> BuscarAsync(string termino)
        {
            return Repositorio.BuscarAsync(termino);
        }

        public async Task<ResultadoCliente> CrearAsync(
            string nombre,
            string apellido,
            string telefono,
            string? correo)
        {
            nombre = nombre?.Trim() ?? string.Empty;
            apellido = apellido?.Trim() ?? string.Empty;
            telefono = telefono?.Trim() ?? string.Empty;
            correo = string.IsNullOrWhiteSpace(correo)
                ? null
                : correo.Trim();

            if (string.IsNullOrEmpty(nombre))
            {
                return new ResultadoCliente
                {
                    Exito = false,
                    Mensaje = "El nombre del cliente es obligatorio."
                };
            }

            if (string.IsNullOrEmpty(apellido))
            {
                return new ResultadoCliente
                {
                    Exito = false,
                    Mensaje = "El apellido del cliente es obligatorio."
                };
            }

            if (string.IsNullOrEmpty(telefono))
            {
                return new ResultadoCliente
                {
                    Exito = false,
                    Mensaje = "El teléfono del cliente es obligatorio."
                };
            }

            Cliente cliente = new()
            {
                Nombre = nombre,
                Apellido = apellido,
                Telefono = telefono,
                CorreoElectronico = correo,
                Estado = 1,
                FechaRegistro = DateTime.Now,
                IdEmpleadoResponsable = _contextoUsuario.IdEmpleado
            };

            int idCliente = await Repositorio.InsertarAsync(cliente);
            cliente.IdCliente = idCliente;

            return new ResultadoCliente
            {
                Exito = true,
                Mensaje = "Cliente registrado correctamente.",
                Cliente = cliente
            };
        }
    }
}
