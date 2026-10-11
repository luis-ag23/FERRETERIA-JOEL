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
    public class CategoriaEditarModel : PageModel
    {
        private readonly ServicioCategoria _servicio;
        private readonly ServicioEmpleado _servicioEmpleado;
        private readonly ILogger<CategoriaEditarModel> _logger;
        private readonly CategoriaValidaciones _validador = new();

        [BindProperty]
        public Categoria CategoriaEdit { get; set; } = new();

        public List<Empleado> Empleados { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public CategoriaEditarModel(
            ServicioCategoria servicio,
            ServicioEmpleado servicioEmpleado,
            ILogger<CategoriaEditarModel> logger)
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
                TempData["MensajeError"] = "La categoría solicitada no existe.";
                return RedirectToPage("Categorias");
            }

            var categoria = _servicio.ObtenerPorId(id);
            if (categoria == null)
            {
                TempData["MensajeError"] = "La categoría solicitada no existe.";
                return RedirectToPage("Categorias");
            }

            CategoriaEdit = categoria;
            return Page();
        }

        public IActionResult OnPost()
        {
            Categoria? categoriaActual = _servicio.ObtenerPorId(CategoriaEdit.IdCategoria);
            if (categoriaActual is null)
            {
                TempData["MensajeError"] = "La categoría solicitada no existe.";
                return RedirectToPage("Categorias");
            }

            CategoriaEdit.Codigo = categoriaActual.Codigo;
            CategoriaEdit.PorcentajeGanancia = 0;
            NormalizarDatos();
            Validar();

            if (Errores.Any() || ErroresCampo.Any())
            {
                CargarEmpleados();
                return Page();
            }

            try
            {
                _servicio.Actualizar(CategoriaEdit);
                TempData["Mensaje"] = "Categoría actualizada con éxito.";
                return RedirectToPage("Categorias");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AgregarErrorCampo(
                    nameof(Categoria.Codigo),
                    $"El código '{CategoriaEdit.Codigo}' ya pertenece a otra categoría.");
                CargarEmpleados();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la categoría {Id}.", CategoriaEdit.IdCategoria);
                Errores.Add("No se pudo actualizar la categoría. Inténtalo nuevamente.");
                CargarEmpleados();
                return Page();
            }
        }

        private void NormalizarDatos()
        {
            CategoriaEdit.Codigo =
                NormalizarTexto(CategoriaEdit.Codigo).ToUpper();

            CategoriaEdit.Nombre =
                NormalizarTexto(CategoriaEdit.Nombre);

            CategoriaEdit.Descripcion =
                string.IsNullOrWhiteSpace(CategoriaEdit.Descripcion)
                    ? null
                    : NormalizarTexto(CategoriaEdit.Descripcion);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }

        private void Validar()
        {
            if (!_validador.EsCodigoValido(CategoriaEdit.Codigo))
            {
                AgregarErrorCampo(
                    nameof(Categoria.Codigo),
                    "El código es obligatorio y debe tener máximo 20 caracteres.");
            }

            if (!_validador.EsNombreValido(CategoriaEdit.Nombre))
            {
                AgregarErrorCampo(
                    nameof(Categoria.Nombre),
                    "El nombre es obligatorio, debe tener máximo 100 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validador.EsDescripcionValida(CategoriaEdit.Descripcion))
            {
                AgregarErrorCampo(
                    nameof(Categoria.Descripcion),
                    "La descripción no debe superar los 255 caracteres.");
            }

            if (!_validador.EsPorcentajeGananciaValido(
                CategoriaEdit.PorcentajeGanancia))
            {
                AgregarErrorCampo(
                    nameof(Categoria.PorcentajeGanancia),
                    "El porcentaje de ganancia debe estar entre 0 y 100.");
            }

            if (!_validador.EsEmpleadoValido(
                CategoriaEdit.IdEmpleadoResponsable))
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
