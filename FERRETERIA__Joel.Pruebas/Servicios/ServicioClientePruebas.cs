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
                _repositorio,
                new ContextoUsuarioFake(idEmpleado: 42));
        }

        [Fact]
        public async Task Crear_ConDatosValidos_DevuelveClienteAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "Ana",
                "Mamani",
                "7111-2222");

            Assert.True(resultado.Exito);
            Assert.NotNull(resultado.Cliente);
            Assert.Equal("Ana", resultado.Cliente.Nombre);
            Assert.Equal("Mamani", resultado.Cliente.Apellido);
            Assert.Equal("71112222", resultado.Cliente.Telefono);
            Assert.Equal(42, resultado.Cliente.IdEmpleadoResponsable);
            Assert.Single(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinNombre_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "   ",
                "Mamani",
                "7111-2222");

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
                "7111-2222");

            Assert.False(resultado.Exito);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinTelefono_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "Ana",
                "Mamani",
                "");

            Assert.False(resultado.Exito);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

                [Theory]
        [InlineData("Ana3", "Mamani", "71112222", "Nombre")]
        [InlineData("Ana", "Mamani*", "71112222", "Apellido")]
        [InlineData("Ana", "Mamani", "1234567", "Telefono")]
        [InlineData("Ana", "Mamani", "91112222", "Telefono")]
        [InlineData("Ana", "Mamani", "7111222A", "Telefono")]
        public async Task Crear_ConFormatoInvalido_FallaConErrorPorCampoAsync(
            string nombre,
            string apellido,
            string telefono,
            string campoConError)
        {
            var resultado = await _servicio.CrearAsync(nombre, apellido, telefono);

            Assert.False(resultado.Exito);
            Assert.Contains(campoConError, resultado.Errores.Keys);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_ConNombreMuyLargo_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                new string('A', 101),
                "Mamani",
                "71112222");

            Assert.False(resultado.Exito);
            Assert.Contains("Nombre", resultado.Errores.Keys);
        }

        [Fact]
        public async Task Crear_ConEspaciosSobrantes_NormalizaLosDatosAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "  Ana   María ",
                " Mamani  Quispe",
                "7111 2222");

            Assert.True(resultado.Exito);
            Assert.Equal("Ana María", resultado.Cliente!.Nombre);
            Assert.Equal("Mamani Quispe", resultado.Cliente.Apellido);
            Assert.Equal("71112222", resultado.Cliente.Telefono);
        }

        [Fact]
        public async Task Crear_ConClienteDuplicado_FallaAsync()
        {
            await _servicio.CrearAsync("Ana", "Mamani", "71112222");

            var resultado = await _servicio.CrearAsync("ana", "MAMANI", "7111-2222");

            Assert.False(resultado.Exito);
            Assert.Contains("Telefono", resultado.Errores.Keys);
            Assert.Single(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_MismoTelefonoConOtroNombre_EsValidoAsync()
        {
            await _servicio.CrearAsync("Ana", "Mamani", "71112222");

            var resultado = await _servicio.CrearAsync("Luis", "Mamani", "71112222");

            Assert.True(resultado.Exito);
            Assert.Equal(2, (await _repositorio.ObtenerTodosAsync()).Count);
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
