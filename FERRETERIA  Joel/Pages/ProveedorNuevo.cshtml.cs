using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Entidades;
using FERRETERIA__Joel.Dominio.Validaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace FERRETERIA__Joel.Pages
{
    public class ProveedorNuevoModel : PageModel
    {
        private readonly ServicioProveedor _servicio;
        private readonly ServicioEmpleado _servicioEmpleado;
        private readonly ILogger<ProveedorNuevoModel> _logger;

        private readonly ProveedorValidaciones _validacion = new();

        [BindProperty]
        public Proveedor Proveedor { get; set; } = new();

        public List<Empleado> Empleados { get; set; } = new();
        public List<string> Errores { get; set; } = new();
        public Dictionary<string, string> ErroresCampo { get; set; } = new();

        public ProveedorNuevoModel(
            ServicioProveedor servicio,
            ServicioEmpleado servicioEmpleado,
            ILogger<ProveedorNuevoModel> logger)
        {
            _servicio = servicio;
            _servicioEmpleado = servicioEmpleado;
            _logger = logger;
        }
        public void OnGet()
        {
            CargarEmpleados();
        }
        public IActionResult OnPost()
        {
            NormalizarDatos();
            Validar();

            if (Errores.Any() || ErroresCampo.Any())
            {
                CargarEmpleados();
                return Page();
            }

            try
            {
                _servicio.Insertar(Proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al registrar el proveedor.");

                Errores.Add(
                    "No se pudo registrar el proveedor. Inténtalo nuevamente.");

                CargarEmpleados();
                return Page();
            }

            TempData["Mensaje"] = "Proveedor creado correctamente.";

            return RedirectToPage("Proveedores");
        }
        private void NormalizarDatos()
        {
            Proveedor.RazonSocial =
                NormalizarTexto(Proveedor.RazonSocial);

            Proveedor.NombreComercial =
                NormalizarTexto(Proveedor.NombreComercial);

            Proveedor.NombreContacto =
                NormalizarTexto(Proveedor.NombreContacto);

            Proveedor.Telefono =
                NormalizarTexto(Proveedor.Telefono);

            Proveedor.CorreoElectronico =
                string.IsNullOrWhiteSpace(Proveedor.CorreoElectronico)
                    ? null
                    : NormalizarTexto(Proveedor.CorreoElectronico);

            Proveedor.Direccion =
                string.IsNullOrWhiteSpace(Proveedor.Direccion)
                    ? null
                    : NormalizarTexto(Proveedor.Direccion);
        }

        private static string NormalizarTexto(string? texto)
        {
            return Regex.Replace(texto?.Trim() ?? "", @"\s+", " ");
        }
        private void Validar()
        {
            if (!_validacion.EsRazonSocialValida(Proveedor.RazonSocial))
            {
                AgregarErrorCampo(
                    nameof(Proveedor.RazonSocial),
                    "La razón social es obligatoria, debe tener máximo 150 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validacion.EsNombreComercialValido(Proveedor.NombreComercial))
            {
                AgregarErrorCampo(
                    nameof(Proveedor.NombreComercial),
                    "El nombre comercial es obligatorio, debe tener máximo 150 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validacion.EsNombreContactoValido(Proveedor.NombreContacto))
            {
                AgregarErrorCampo(
                    nameof(Proveedor.NombreContacto),
                    "El nombre del contacto es obligatorio, debe tener máximo 150 caracteres y solo admite letras y espacios (sin números ni caracteres especiales).");
            }

            if (!_validacion.EsTelefonoValido(Proveedor.Telefono))
            {
                AgregarErrorCampo(
                    nameof(Proveedor.Telefono),
                    "El teléfono debe tener 8 dígitos y comenzar con 5, 6, 7 u 8.");
            }

            if (!_validacion.EsCorreoValido(Proveedor.CorreoElectronico))
            {
                AgregarErrorCampo(
                    nameof(Proveedor.CorreoElectronico),
                    "El correo debe tener un formato válido: mínimo 6 caracteres antes de la @ y un dominio como correo@ejemplo.com.");
            }

            if (!_validacion.EsEmpleadoValido(Proveedor.IdEmpleadoResponsable))
            {
                AgregarErrorCampo(
                    nameof(Proveedor.IdEmpleadoResponsable),
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
