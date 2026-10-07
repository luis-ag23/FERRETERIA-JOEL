using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Validaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    public class ProductoNuevoModel : PageModel
    {
        private readonly ServicioProducto _servicio;
        private readonly ServicioCategoria _servicioCategoria;
        private readonly ServicioEmpleado _servicioEmpleado;
        private readonly ServicioMarca _servicioMarca;
        private readonly ILogger<ProductoNuevoModel> _logger;

        private readonly ProductoValidaciones _validacion = new();

        [BindProperty]
        public Producto Producto { get; set; } = new();

        public List<Categoria> Categorias { get; set; } = new();
        public List<Marca> Marcas { get; set; } = new();
        public List<Empleado> Empleados { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public static readonly string[] UnidadesMedida =
            { "Caja", "Kilogramo", "Litro", "Metro", "Par", "Unidad" };

        public ProductoNuevoModel(
            ServicioProducto servicio,
            ServicioCategoria servicioCategoria,
            ServicioEmpleado servicioEmpleado,
            ServicioMarca servicioMarca,
            ILogger<ProductoNuevoModel> logger)
        {
            _servicio = servicio;
            _servicioCategoria = servicioCategoria;
            _servicioEmpleado = servicioEmpleado;
            _servicioMarca = servicioMarca;
            _logger = logger;
        }

        public void OnGet()
        {
            Producto.Codigo = _servicio.SiguienteCodigo();

            CargarCatalogos();
        }

        public IActionResult OnPost()
        {
            Producto.Codigo = _servicio.SiguienteCodigo();
            NormalizarPrecio();
            NormalizarDatos();
            Validar();

            if (Errores.Any() || ErroresCampo.Any())
            {
                CargarCatalogos();
                return Page();
            }

            try
            {
                Insertar();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AgregarErrorCampo(
                    nameof(Producto.Codigo),
                    "Ya existe un producto con ese código.");

                CargarCatalogos();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar el producto.");

                Errores.Add(
                    "No se pudo registrar el producto. Inténtalo nuevamente.");

                CargarCatalogos();
                return Page();
            }

            TempData["Mensaje"] =
                "Producto creado correctamente.";

            return RedirectToPage("Productos");
        }

        private void NormalizarDatos()
        {
            Producto.Codigo =
                NormalizarTexto(Producto.Codigo).ToUpper();

            Producto.Nombre =
                NormalizarTexto(Producto.Nombre);

            Producto.Descripcion =
                string.IsNullOrWhiteSpace(Producto.Descripcion)
                    ? null
                    : NormalizarTexto(Producto.Descripcion);

            Producto.UnidadMedida =
                NormalizarTexto(Producto.UnidadMedida);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }

        private void NormalizarPrecio()
        {
            string precioTexto = Request.Form["Producto.PrecioVenta"].ToString();
            if (decimal.TryParse(
                precioTexto.Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal precio))
            {
                Producto.PrecioVenta = precio;
                ModelState.Remove("Producto.PrecioVenta");
            }
        }

        private void Validar()
        {
            if (!_validacion.EsCodigoValido(Producto.Codigo))
            {
                AgregarErrorCampo(
                    nameof(Producto.Codigo),
                    "El código es obligatorio y debe tener máximo 30 caracteres.");
            }

            if (!_validacion.EsNombreValido(Producto.Nombre))
            {
                AgregarErrorCampo(
                    nameof(Producto.Nombre),
                    "El nombre es obligatorio, debe tener máximo 150 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validacion.EsMarcaValida(Producto.IdMarca))
            {
                AgregarErrorCampo(
                    nameof(Producto.IdMarca),
                    "Debe seleccionar una marca.");
            }
            else if (!_servicioMarca
                .ObtenerActivas()
                .Any(m => m.IdMarca == Producto.IdMarca))
            {
                AgregarErrorCampo(
                    nameof(Producto.IdMarca),
                    "La marca seleccionada no existe o está inactiva.");
            }

            if (!_validacion.EsDescripcionValida(Producto.Descripcion))
            {
                AgregarErrorCampo(
                    nameof(Producto.Descripcion),
                    "La descripción no debe superar los 500 caracteres.");
            }

            if (!_validacion.EsUnidadMedidaValida(Producto.UnidadMedida))
            {
                AgregarErrorCampo(
                    nameof(Producto.UnidadMedida),
                    "Debe indicar la unidad de medida.");
            }

            if (!_validacion.EsPrecioValido(Producto.PrecioVenta))
            {
                AgregarErrorCampo(
                    nameof(Producto.PrecioVenta),
                    "El precio debe ser mayor a 0.");
            }

            if (!_validacion.EsCategoriaValida(Producto.IdCategoria))
            {
                AgregarErrorCampo(
                    nameof(Producto.IdCategoria),
                    "Debe seleccionar una categoría.");
            }
            else if (!_servicioCategoria
                .ObtenerActivas()
                .Any(c => c.IdCategoria == Producto.IdCategoria))
            {
                AgregarErrorCampo(
                    nameof(Producto.IdCategoria),
                    "La categoría seleccionada no existe o está inactiva.");
            }

            if (!_validacion.EsEmpleadoValido(
                Producto.IdEmpleadoResponsable))
            {
                AgregarErrorCampo(
                    nameof(Producto.IdEmpleadoResponsable),
                    "Debe seleccionar un empleado responsable.");
            }
        }

        private void AgregarErrorCampo(string campo, string mensaje)
        {
            ErroresCampo[campo] = mensaje;
        }

        private void CargarCatalogos()
        {
            Categorias =
                _servicioCategoria.ObtenerActivas();

            Marcas =
                _servicioMarca.ObtenerActivas();

            Empleados =
                _servicioEmpleado.ObtenerTodos();
        }

        private void Insertar()
        {
            _servicio.Crear(Producto);
        }
    }
}
