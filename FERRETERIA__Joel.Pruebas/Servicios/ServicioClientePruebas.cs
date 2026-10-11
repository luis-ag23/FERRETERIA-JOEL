using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Pruebas.Compartido;
using Xunit;

namespace FERRETERIA__Joel.Pruebas.Servicios
{
    public class ServicioClientePruebas
    {
        private readonly ClienteRepositorioFake _repositorio;
        private readonly ServicioCliente _servicio;

        public ServicioClientePruebas()
        {
            _repositorio = new ClienteRepositorioFake();
            _servicio = new ServicioCliente(
                new CreadorFake<Dominio.Puertos.IClienteRepository>(_repositorio),
                new ContextoUsuarioFake(idEmpleado: 42));
        }

        [Fact]
        public async Task Crear_ConDatosValidos_DevuelveClienteAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "Ana",
                "Mamani",
                "7111-2222",
                "ana@correo.com");

            Assert.True(resultado.Exito);
            Assert.NotNull(resultado.Cliente);
            Assert.Equal("Ana", resultado.Cliente.Nombre);
            Assert.Equal("Mamani", resultado.Cliente.Apellido);
            Assert.Equal("7111-2222", resultado.Cliente.Telefono);
            Assert.Equal("ana@correo.com", resultado.Cliente.CorreoElectronico);
            Assert.Equal(42, resultado.Cliente.IdEmpleadoResponsable);
            Assert.Single(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinNombre_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "   ",
                "Mamani",
                "7111-2222",
                null);

            Assert.False(resultado.Exito);
            Assert.Null(resultado.Cliente);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinApellido_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "Ana",
                " ",
                "7111-2222",
                null);

            Assert.False(resultado.Exito);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinTelefono_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "Ana",
                "Mamani",
                "",
                null);

            Assert.False(resultado.Exito);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Buscar_SoloDevuelveActivosAsync()
        {
            _repositorio.Agregar(new Cliente
            {
                Nombre = "Juan",
                Apellido = "Perez",
                Telefono = "7777-0000",
                Estado = 1
            });
            _repositorio.Agregar(new Cliente
            {
                Nombre = "Inactivo",
                Apellido = "Perez",
                Telefono = "7777-9999",
                Estado = 0
            });

            var encontrados = await _servicio.BuscarAsync("perez");

            Assert.Single(encontrados);
            Assert.Equal("Juan", encontrados[0].Nombre);
        }
    }
}