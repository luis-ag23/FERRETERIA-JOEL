using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Validaciones;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = FERRETERIA__Joel.Dominio.Constantes.RolesSistema.Administrador)]
    public class ProductoNuevoModel : PageModel
    {
        private readonly ServicioProducto _servicio;
        private readonly ServicioCategoria _servicioCategoria;
        private readonly ServicioMarca _servicioMarca;
        private readonly ILogger<ProductoNuevoModel> _logger;

        private readonly ProductoValidaciones _validacion = new();

        [BindProperty]
        public Producto Producto { get; set; } = new();

        public List<Categoria> Categorias { get; set; } = new();
        public List<Marca> Marcas { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public static readonly string[] UnidadesMedida =
            { "Caja", "Kilogramo", "Litro", "Metro", "Par", "Unidad" };

        public ProductoNuevoModel(
            ServicioProducto servicio,
            ServicioCategoria servicioCategoria,
            ServicioMarca servicioMarca,
            ILogger<ProductoNuevoModel> logger)
        {
            _servicio = servicio;
            _servicioCategoria = servicioCategoria;
            _servicioMarca = servicioMarca;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Producto.Codigo = await _servicio.SiguienteCodigoAsync();

            await CargarCatalogosAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Producto.Codigo = await _servicio.SiguienteCodigoAsync();
            Producto.IdEmpleadoResponsable =
                EmpleadoSesion.ObtenerIdEmpleado(User);
            NormalizarPrecio();
            NormalizarDatos();
            await ValidarAsync();

            if (Errores.Any() || ErroresCampo.Any())
            {
                await CargarCatalogosAsync();
                return Page();
            }

            try
            {
                await InsertarAsync();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AgregarErrorCampo(
                    nameof(Producto.Codigo),
                    "Ya existe un producto con ese código.");

                await CargarCatalogosAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar el producto.");

                Errores.Add(
                    "No se pudo registrar el producto. Inténtalo nuevamente.");

                await CargarCatalogosAsync();
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

        private async Task ValidarAsync()
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
            else if (!(await _servicioMarca
                .ObtenerActivasAsync())
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

            if (!_validacion.EsCantidadStockValido(Producto.CantidadStock))
            {
                AgregarErrorCampo(
                    nameof(Producto.CantidadStock),
                    "La cantidad en stock no puede ser negativa.");
            }

            if (!_validacion.EsCategoriaValida(Producto.IdCategoria))
            {
                AgregarErrorCampo(
                    nameof(Producto.IdCategoria),
                    "Debe seleccionar una categoría.");
            }
            else if (!(await _servicioCategoria
                .ObtenerActivasAsync())
                .Any(c => c.IdCategoria == Producto.IdCategoria))
            {
                AgregarErrorCampo(
                    nameof(Producto.IdCategoria),
                    "La categoría seleccionada no existe o está inactiva.");
            }
        }

        private void AgregarErrorCampo(string campo, string mensaje)
        {
            ErroresCampo[campo] = mensaje;
        }

        private async Task CargarCatalogosAsync()
        {
            Categorias =
                await _servicioCategoria.ObtenerActivasAsync();

            Marcas =
                await _servicioMarca.ObtenerActivasAsync();
        }

        private async Task InsertarAsync()
        {
            await _servicio.CrearAsync(Producto);
        }
    }
}
