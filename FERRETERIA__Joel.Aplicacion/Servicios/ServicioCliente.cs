using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Repositorios;

namespace FERRETERIA__Joel.Aplicacion.Servicios
{
    /// <summary>
    /// Caso de uso de clientes. Permite dar de alta un cliente desde el
    /// propio flujo de venta (sin abandonar la pantalla) y consultar el
    /// catálogo de clientes activos. El cliente no tiene credenciales: el
    /// empleado en sesión queda registrado como responsable del alta.
    /// </summary>
    public class ServicioCliente
    {
        private readonly MySqlClienteRepository _repositorio;
        private readonly IContextoUsuario _contextoUsuario;

        public ServicioCliente(
            MySqlClienteRepository repositorio,
            IContextoUsuario contextoUsuario)
        {
            _repositorio = repositorio;
            _contextoUsuario = contextoUsuario;
        }

        public async Task<List<Cliente>> ListarAsync()
        {
            return await _repositorio.ObtenerTodosAsync();
        }

        public async Task<List<Cliente>> BuscarAsync(string termino)
        {
            return await _repositorio.BuscarAsync(termino);
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int idCliente)
        {
            return await _repositorio.ObtenerPorIdAsync(idCliente);
        }

        public async Task<ResultadoCliente> CrearAsync(
            string nombre,
            string apellido,
            string telefono)
        {
            int idEmpleado = _contextoUsuario.IdEmpleado;
            if (idEmpleado <= 0)
            {
                return new ResultadoCliente
                {
                    Exito = false,
                    Mensaje = "Debe iniciar sesión con un empleado válido para registrar clientes."
                };
            }

            nombre = nombre?.Trim() ?? string.Empty;
            apellido = apellido?.Trim() ?? string.Empty;
            telefono = telefono?.Trim() ?? string.Empty;

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
                Estado = 1,
                FechaRegistro = DateTime.Now,
                IdEmpleadoResponsable = idEmpleado
            };

            int idCliente = await _repositorio.InsertarAsync(cliente);
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
