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
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = FERRETERIA__Joel.Dominio.Constantes.RolesSistema.Administrador)]
    public class CategoriaNuevaModel : PageModel
    {
        private readonly ServicioCategoria _servicio;
        private readonly ILogger<CategoriaNuevaModel> _logger;

        private readonly CategoriaValidaciones _validador = new();

        [BindProperty]
        public Categoria NuevaCategoria { get; set; } = new();

        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public CategoriaNuevaModel(
            ServicioCategoria servicio,
            ILogger<CategoriaNuevaModel> logger)
        {
            _servicio = servicio;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            NuevaCategoria.Estado = 1;
            NuevaCategoria.Codigo = await _servicio.SiguienteCodigoAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            NuevaCategoria.Codigo = await _servicio.SiguienteCodigoAsync();
            NuevaCategoria.PorcentajeGanancia = 0;
            NuevaCategoria.IdEmpleadoResponsable =
                EmpleadoSesion.ObtenerIdEmpleado(User);
            NormalizarDatos();
            Validar();

            if (Errores.Any() || ErroresCampo.Any())
            {
                return Page();
            }

            try
            {
                await _servicio.InsertarAsync(NuevaCategoria);

                TempData["Mensaje"] =
                    "Categoría registrada con éxito.";

                return RedirectToPage("Categorias");
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                AgregarErrorCampo(
                    nameof(Categoria.Codigo),
                    $"El código '{NuevaCategoria.Codigo}' ya existe en el sistema.");

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar la categoría.");

                Errores.Add(
                    "No se pudo registrar la categoría. Inténtalo nuevamente.");

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
        }

        private void AgregarErrorCampo(string campo, string mensaje)
        {
            ErroresCampo[campo] = mensaje;
        }
    }
}
