using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Caso de uso de clientes. Permite dar de alta un cliente desde el
    /// propio flujo de venta (sin abandonar la pantalla) y consultar el
    /// catálogo de clientes activos.
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

        public List<Cliente> Listar()
        {
            return Repositorio.ObtenerTodos();
        }

        public List<Cliente> Buscar(string termino)
        {
            return Repositorio.Buscar(termino);
        }

        public ResultadoCliente Crear(
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

            int idCliente = Repositorio.Insertar(cliente);
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