using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Puertos;
using FERRETERIA__Joel.Infraestructura.Repositorios;
using System.Text.RegularExpressions;

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
        private const string MensajeRevisarDatos =
            "Revise los datos señalados e inténtelo nuevamente.";

        private readonly ClienteValidador _validador = new();
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

            nombre = NormalizarTexto(nombre);
            apellido = NormalizarTexto(apellido);
            telefono = NormalizarTelefono(telefono);

            Dictionary<string, string> errores =
                _validador.Validar(nombre, apellido, telefono);

            if (errores.Count == 0
                && await _repositorio.ExisteAsync(nombre, apellido, telefono))
            {
                errores[ClienteValidador.CampoTelefono] =
                    "Ya existe un cliente registrado con los mismos nombres, apellidos y teléfono.";
            }

            if (errores.Count > 0)
            {
                return new ResultadoCliente
                {
                    Exito = false,
                    Mensaje = MensajeRevisarDatos,
                    Errores = errores
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

        // Quita espacios sobrantes: "  Ana   María " -> "Ana María".
        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? string.Empty, @"\s+", " ");
        }

        // Acepta "7111 2222" o "7111-2222" y guarda solo los dígitos.
        private static string NormalizarTelefono(string? telefono)
        {
            return Regex.Replace(telefono ?? string.Empty, @"[\s\-]", string.Empty);
        }
    }
}
