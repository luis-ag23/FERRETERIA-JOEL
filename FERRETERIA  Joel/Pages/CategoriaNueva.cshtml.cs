using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Validaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    public class CategoriaNuevaModel : PageModel
    {
        private readonly ServicioCategoria _servicio;
        private readonly ServicioEmpleado _servicioEmpleado;
        private readonly ILogger<CategoriaNuevaModel> _logger;

        private readonly CategoriaValidaciones _validador = new();

        [BindProperty]
        public Categoria NuevaCategoria { get; set; } = new();

        public List<Empleado> Empleados { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public CategoriaNuevaModel(
            ServicioCategoria servicio,
            ServicioEmpleado servicioEmpleado,
            ILogger<CategoriaNuevaModel> logger)
        {
            _servicio = servicio;
            _servicioEmpleado = servicioEmpleado;
            _logger = logger;
        }

        public void OnGet()
        {
            NuevaCategoria.Estado = 1;
            NuevaCategoria.Codigo = _servicio.SiguienteCodigo();
            CargarEmpleados();
        }

        public IActionResult OnPost()
        {
            NuevaCategoria.Codigo = _servicio.SiguienteCodigo();
            NuevaCategoria.PorcentajeGanancia = 0;
            NormalizarDatos();
            Validar();

            if (Errores.Any() || ErroresCampo.Any())
            {
                CargarEmpleados();
                return Page();
            }

            try
            {
                _servicio.Insertar(NuevaCategoria);

                TempData["Mensaje"] =
                    "Categoría registrada con éxito.";

                return RedirectToPage("Categorias");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AgregarErrorCampo(
                    nameof(Categoria.Codigo),
                    $"El código '{NuevaCategoria.Codigo}' ya existe en el sistema.");

                CargarEmpleados();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar la categoría.");

                Errores.Add(
                    "No se pudo registrar la categoría. Inténtalo nuevamente.");

                CargarEmpleados();
                return Page();
            }
        }

        private void NormalizarDatos()
        {
            NuevaCategoria.Codigo =
                NormalizarTexto(NuevaCategoria.Codigo).ToUpper();

            NuevaCategoria.Nombre =
                NormalizarTexto(NuevaCategoria.Nombre);

            NuevaCategoria.Descripcion =
                string.IsNullOrWhiteSpace(NuevaCategoria.Descripcion)
                    ? null
                    : NormalizarTexto(NuevaCategoria.Descripcion);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }

        private void Validar()
        {
            if (!_validador.EsCodigoValido(NuevaCategoria.Codigo))
            {
                AgregarErrorCampo(
                    nameof(Categoria.Codigo),
                    "El código es obligatorio y debe tener máximo 20 caracteres.");
            }

            if (!_validador.EsNombreValido(NuevaCategoria.Nombre))
            {
                AgregarErrorCampo(
                    nameof(Categoria.Nombre),
                    "El nombre es obligatorio, debe tener máximo 100 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validador.EsDescripcionValida(NuevaCategoria.Descripcion))
            {
                AgregarErrorCampo(
                    nameof(Categoria.Descripcion),
                    "La descripción no debe superar los 255 caracteres.");
            }

            if (!_validador.EsPorcentajeGananciaValido(
                NuevaCategoria.PorcentajeGanancia))
            {
                AgregarErrorCampo(
                    nameof(Categoria.PorcentajeGanancia),
                    "El porcentaje de ganancia debe estar entre 0 y 100.");
            }

            if (!_validador.EsEmpleadoValido(
                NuevaCategoria.IdEmpleadoResponsable))
            {
                AgregarErrorCampo(
                    nameof(Categoria.IdEmpleadoResponsable),
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
