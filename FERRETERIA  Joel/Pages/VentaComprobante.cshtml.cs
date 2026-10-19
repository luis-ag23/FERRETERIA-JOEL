using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Comprobante/recibo de una venta. El identificador viaja cifrado
    /// en la ruta (token) para no exponer el id; el descifrado está
    /// delegado en <see cref="FERRETERIA__Joel.Helpers.UrlProtector"/>.
    /// </summary>
    [Authorize]
    public class VentaComprobanteModel : PageModel
    {
        private readonly FachadaVenta _fachada;
        private readonly IGeneradorComprobantePdf _generadorPdf;

        public VentaComprobanteModel(
            FachadaVenta fachada,
            IGeneradorComprobantePdf generadorPdf)
        {
            _fachada = fachada;
            _generadorPdf = generadorPdf;
        }

        public string Token { get; private set; } = string.Empty;

        public ComprobanteVenta? Comprobante { get; private set; }

        public bool NoEncontrado { get; private set; }

        public async Task OnGetAsync(string token)
        {
            await CargarAsync(token);
        }

        /// <summary>
        /// Descarga el comprobante como PDF (?handler=Pdf). Reutiliza la misma
        /// resolución del token y la misma fachada que la vista en pantalla.
        /// </summary>
        public async Task<IActionResult> OnGetPdfAsync(string token)
        {
            await CargarAsync(token);

            if (Comprobante is null)
            {
                return NotFound();
            }

            byte[] pdf = _generadorPdf.Generar(Comprobante);
            string codigo = Comprobante.Venta.CodigoVenta ?? Comprobante.Venta.IdVenta.ToString();

            Response.Headers["Content-Disposition"] = $"inline; filename=\"Comprobante-{codigo}.pdf\"";
            return File(pdf, "application/pdf");
        }

        private async Task CargarAsync(string token)
        {
            Token = token;

            string? texto = UrlProtector.Descifrar(token);

            if (texto is null
                || !int.TryParse(texto, out int idVenta)
                || idVenta <= 0)
            {
                NoEncontrado = true;
                return;
            }

            Comprobante = await _fachada.ObtenerComprobanteAsync(idVenta);

            if (Comprobante is null)
            {
                NoEncontrado = true;
            }
        }

        public static string ImprimirEstado(string estado)
        {
            return estado == EstadoVenta.Registrada
                ? "Registrada"
                : "Anulada";
        }
    }
}