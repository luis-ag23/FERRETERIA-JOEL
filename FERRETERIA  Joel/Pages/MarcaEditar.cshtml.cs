using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Validaciones;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    public class MarcaEditarModel : PageModel
    {
        private readonly ServicioMarca _servicio;
        private readonly ServicioEmpleado _servicioEmpleado;
        private readonly ILogger<MarcaEditarModel> _logger;

        private readonly MarcaValidaciones _validador = new();

        [BindProperty]
        public Marca MarcaEdit { get; set; } = new();

        public List<Empleado> Empleados { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public MarcaEditarModel(
            ServicioMarca servicio,
            ServicioEmpleado servicioEmpleado,
            ILogger<MarcaEditarModel> logger)
        {
            _servicio = servicio;
            _servicioEmpleado = servicioEmpleado;
            _logger = logger;
        }

        public IActionResult OnGet(string token)
        {
            CargarEmpleados();

            string? texto = UrlProtector.Descifrar(token);

            if (!int.TryParse(texto, out int id))
            {
                TempData["MensajeError"] = "La marca solicitada no existe.";
                return RedirectToPage("Marcas");
            }

            var marca = _servicio.ObtenerPorId(id);
            if (marca == null)
            {
                TempData["MensajeError"] = "La marca solicitada no existe.";
                return RedirectToPage("Marcas");
            }

            MarcaEdit = marca;
            return Page();
        }

        public IActionResult OnPost()
        {
            Marca? marcaActual = _servicio.ObtenerPorId(MarcaEdit.IdMarca);
            if (marcaActual is null)
            {
                TempData["MensajeError"] = "La marca solicitada no existe.";
                return RedirectToPage("Marcas");
            }

            NormalizarDatos();
            Validar();

            if (Errores.Any() || ErroresCampo.Any())
            {
                CargarEmpleados();
                return Page();
            }

            try
            {
                _servicio.Actualizar(MarcaEdit);
                TempData["Mensaje"] = "Marca actualizada con éxito.";
                return RedirectToPage("Marcas");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AgregarErrorCampo(
                    nameof(Marca.Nombre),
                    $"Ya existe otra marca con el nombre '{MarcaEdit.Nombre}'.");
                CargarEmpleados();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la marca {Id}.", MarcaEdit.IdMarca);
                Errores.Add("No se pudo actualizar la marca. Inténtalo nuevamente.");
                CargarEmpleados();
                return Page();
            }
        }

        private void NormalizarDatos()
        {
            MarcaEdit.Nombre =
                NormalizarTexto(MarcaEdit.Nombre);

            MarcaEdit.PaisOrigen =
                NormalizarTexto(MarcaEdit.PaisOrigen);

            MarcaEdit.Calidad =
                NormalizarTexto(MarcaEdit.Calidad);

            MarcaEdit.Descripcion =
                string.IsNullOrWhiteSpace(MarcaEdit.Descripcion)
                    ? null
                    : NormalizarTexto(MarcaEdit.Descripcion);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }

        private void Validar()
        {
            if (!_validador.EsNombreValido(MarcaEdit.Nombre))
            {
                AgregarErrorCampo(
                    nameof(Marca.Nombre),
                    "El nombre es obligatorio, debe tener máximo 100 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validador.EsPaisOrigenValido(MarcaEdit.PaisOrigen))
            {
                AgregarErrorCampo(
                    nameof(Marca.PaisOrigen),
                    "El país de origen es obligatorio y debe tener máximo 100 caracteres.");
            }

            if (!_validador.EsCalidadValida(MarcaEdit.Calidad))
            {
                AgregarErrorCampo(
                    nameof(Marca.Calidad),
                    "Debe seleccionar la calidad de la marca.");
            }

            if (!_validador.EsDescripcionValida(MarcaEdit.Descripcion))
            {
                AgregarErrorCampo(
                    nameof(Marca.Descripcion),
                    "La descripción no debe superar los 255 caracteres.");
            }

            if (!_validador.EsEmpleadoValido(
                MarcaEdit.IdEmpleadoResponsable))
            {
                AgregarErrorCampo(
                    nameof(Marca.IdEmpleadoResponsable),
                    "Debe seleccionar un empleado responsable.");
            }
        }

        private void AgregarErrorCampo(string campo, string mensaje)
        {
            ErroresCampo[campo] = mensaje;
        }

        private void CargarEmpleados()
        {
            Empleados = _servicioEmpleado.ObtenerTodos();
        }
    }
}