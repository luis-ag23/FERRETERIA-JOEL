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
                "1234567", "Ana", "Mamani", "7111-2222");

            Assert.True(resultado.Exito);
            Assert.NotNull(resultado.Cliente);
            Assert.Equal("1234567", resultado.Cliente.Ci);
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
                "1234567", "   ", "Mamani", "7111-2222");

            Assert.False(resultado.Exito);
            Assert.Null(resultado.Cliente);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinApellido_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "1234567", "Ana", " ", "7111-2222");

            Assert.False(resultado.Exito);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinTelefono_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "1234567", "Ana", "Mamani", "");

            Assert.False(resultado.Exito);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_SinCi_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "  ", "Ana", "Mamani", "71112222");

            Assert.False(resultado.Exito);
            Assert.Contains("Ci", resultado.Errores.Keys);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Theory]
        [InlineData("1234567", "Ana3", "Mamani", "71112222", "Nombre")]
        [InlineData("1234567", "Ana", "Mamani*", "71112222", "Apellido")]
        [InlineData("1234567", "Ana", "Mamani", "1234567", "Telefono")]
        [InlineData("1234567", "Ana", "Mamani", "91112222", "Telefono")]
        [InlineData("1234567", "Ana", "Mamani", "7111222A", "Telefono")]
        [InlineData("12345", "Ana", "Mamani", "71112222", "Ci")]
        [InlineData("123456789", "Ana", "Mamani", "71112222", "Ci")]
        [InlineData("12A4567", "Ana", "Mamani", "71112222", "Ci")]
        public async Task Crear_ConFormatoInvalido_FallaConErrorPorCampoAsync(
            string ci,
            string nombre,
            string apellido,
            string telefono,
            string campoConError)
        {
            var resultado = await _servicio.CrearAsync(ci, nombre, apellido, telefono);

            Assert.False(resultado.Exito);
            Assert.Contains(campoConError, resultado.Errores.Keys);
            Assert.Empty(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_ConNombreMuyLargo_FallaAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "1234567", new string('A', 101), "Mamani", "71112222");

            Assert.False(resultado.Exito);
            Assert.Contains("Nombre", resultado.Errores.Keys);
        }

        [Fact]
        public async Task Crear_ConEspaciosSobrantes_NormalizaLosDatosAsync()
        {
            var resultado = await _servicio.CrearAsync(
                "1234 567", "  Ana   María ", " Mamani  Quispe", "7111 2222");

            Assert.True(resultado.Exito);
            Assert.Equal("1234567", resultado.Cliente!.Ci);
            Assert.Equal("Ana María", resultado.Cliente.Nombre);
            Assert.Equal("Mamani Quispe", resultado.Cliente.Apellido);
            Assert.Equal("71112222", resultado.Cliente.Telefono);
        }

        [Fact]
        public async Task Crear_ConCiDuplicado_FallaAsync()
        {
            await _servicio.CrearAsync("1234567", "Ana", "Mamani", "71112222");

            var resultado = await _servicio.CrearAsync(
                "1234567", "Luis", "Quispe", "72223333");

            Assert.False(resultado.Exito);
            Assert.Contains("Ci", resultado.Errores.Keys);
            Assert.Single(await _repositorio.ObtenerTodosAsync());
        }

        [Fact]
        public async Task Crear_MismoTelefonoConOtroCi_EsValidoAsync()
        {
            await _servicio.CrearAsync("1234567", "Ana", "Mamani", "71112222");

            var resultado = await _servicio.CrearAsync(
                "7654321", "Luis", "Mamani", "71112222");

            Assert.True(resultado.Exito);
            Assert.Equal(2, (await _repositorio.ObtenerTodosAsync()).Count);
        }

        [Fact]
        public async Task Buscar_PorCi_SoloDevuelveActivosAsync()
        {
            _repositorio.Agregar(new Cliente
            {
                Ci = "1234567", Nombre = "Juan", Apellido = "Perez",
                Telefono = "7777-0000", Estado = 1
            });
            _repositorio.Agregar(new Cliente
            {
                Ci = "1234568", Nombre = "Inactivo", Apellido = "Perez",
                Telefono = "7777-9999", Estado = 0
            });

            var encontrados = await _servicio.BuscarAsync("12345");

            Assert.Single(encontrados);
            Assert.Equal("Juan", encontrados[0].Nombre);
        }

        [Fact]
        public async Task Buscar_PorNombre_SoloDevuelveActivosAsync()
        {
            _repositorio.Agregar(new Cliente
            {
                Ci = "1234567", Nombre = "Juan", Apellido = "Perez",
                Telefono = "7777-0000", Estado = 1
            });
            _repositorio.Agregar(new Cliente
            {
                Ci = "1234568", Nombre = "Inactivo", Apellido = "Perez",
                Telefono = "7777-9999", Estado = 0
            });

            var encontrados = await _servicio.BuscarAsync("perez");

            Assert.Single(encontrados);
            Assert.Equal("Juan", encontrados[0].Nombre);
        }

        [Fact]
        public async Task Buscar_SinTexto_DevuelveListaVaciaAsync()
        {
            _repositorio.Agregar(new Cliente
            {
                Ci = "1234567", Nombre = "Juan", Apellido = "Perez",
                Telefono = "7777-0000", Estado = 1
            });

            Assert.Empty(await _servicio.BuscarAsync("   "));
        }
    }
}