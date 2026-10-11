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
        public void Crear_ConDatosValidos_RegistraCliente()
        {
            var resultado = _servicio.Crear(
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
            Assert.Single(_repositorio.ObtenerTodos());
        }

        [Fact]
        public void Crear_SinNombre_Falla()
        {
            var resultado = _servicio.Crear(
                "   ",
                "Mamani",
                "7111-2222",
                null);

            Assert.False(resultado.Exito);
            Assert.Null(resultado.Cliente);
            Assert.Empty(_repositorio.ObtenerTodos());
        }

        [Fact]
        public void Crear_SinApellido_Falla()
        {
            var resultado = _servicio.Crear(
                "Ana",
                " ",
                "7111-2222",
                null);

            Assert.False(resultado.Exito);
            Assert.Empty(_repositorio.ObtenerTodos());
        }

        [Fact]
        public void Crear_SinTelefono_Falla()
        {
            var resultado = _servicio.Crear(
                "Ana",
                "Mamani",
                "",
                null);

            Assert.False(resultado.Exito);
            Assert.Empty(_repositorio.ObtenerTodos());
        }

        [Fact]
        public void Buscar_SoloDevuelveActivos()
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

            var encontrados = _servicio.Buscar("perez");

            Assert.Single(encontrados);
            Assert.Equal("Juan", encontrados[0].Nombre);
        }
    }
}