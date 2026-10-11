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
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = FERRETERIA__Joel.Dominio.Constantes.RolesSistema.Empleado)]
    public class MarcaNuevaModel : PageModel
    {
        private readonly ServicioMarca _servicio;
        private readonly ILogger<MarcaNuevaModel> _logger;

        private readonly MarcaValidaciones _validador = new();

        [BindProperty]
        public Marca NuevaMarca { get; set; } = new();

        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public static readonly string[] Calidades =
            { "Alta", "Media", "Baja" };

        public MarcaNuevaModel(
            ServicioMarca servicio,
            ILogger<MarcaNuevaModel> logger)
        {
            _servicio = servicio;
            _logger = logger;
        }

        public void OnGet()
        {
            NuevaMarca.Estado = 1;
        }

        public IActionResult OnPost()
        {
            NuevaMarca.IdEmpleadoResponsable =
                EmpleadoSesion.ObtenerIdEmpleado(User);
            NormalizarDatos();
            Validar();

            if (Errores.Any() || ErroresCampo.Any())
            {
                return Page();
            }

            try
            {
                _servicio.Insertar(NuevaMarca);

                TempData["Mensaje"] =
                    "Marca registrada con éxito.";

                return RedirectToPage("Marcas");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AgregarErrorCampo(
                    nameof(Marca.Nombre),
                    $"Ya existe una marca con el nombre '{NuevaMarca.Nombre}'.");

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar la marca.");

                Errores.Add(
                    "No se pudo registrar la marca. Inténtalo nuevamente.");

                return Page();
            }
        }

        private void NormalizarDatos()
        {
            NuevaMarca.Nombre =
                NormalizarTexto(NuevaMarca.Nombre);

            NuevaMarca.PaisOrigen =
                NormalizarTexto(NuevaMarca.PaisOrigen);

            NuevaMarca.Calidad =
                NormalizarTexto(NuevaMarca.Calidad);

            NuevaMarca.Descripcion =
                string.IsNullOrWhiteSpace(NuevaMarca.Descripcion)
                    ? null
                    : NormalizarTexto(NuevaMarca.Descripcion);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }

        private void Validar()
        {
            if (!_validador.EsNombreValido(NuevaMarca.Nombre))
            {
                AgregarErrorCampo(
                    nameof(Marca.Nombre),
                    "El nombre es obligatorio, debe tener máximo 100 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validador.EsPaisOrigenValido(NuevaMarca.PaisOrigen))
            {
                AgregarErrorCampo(
                    nameof(Marca.PaisOrigen),
                    "El país de origen es obligatorio y debe tener máximo 100 caracteres.");
            }

            if (!_validador.EsCalidadValida(NuevaMarca.Calidad))
            {
                AgregarErrorCampo(
                    nameof(Marca.Calidad),
                    "Debe seleccionar la calidad de la marca.");
            }

            if (!_validador.EsDescripcionValida(NuevaMarca.Descripcion))
            {
                AgregarErrorCampo(
                    nameof(Marca.Descripcion),
                    "La descripción no debe superar los 255 caracteres.");
            }
        }

        private void AgregarErrorCampo(string campo, string mensaje)
        {
            ErroresCampo[campo] = mensaje;
        }
    }
}