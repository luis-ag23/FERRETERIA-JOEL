using FERRETERIA__Joel.Aplicacion.Modelos;
using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Helpers;
using Microsoft.AspNetCore.Authorization;
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

        public VentaComprobanteModel(FachadaVenta fachada)
        {
            _fachada = fachada;
        }

        public ComprobanteVenta? Comprobante { get; private set; }

        public bool NoEncontrado { get; private set; }

        public void OnGet(string token)
        {
            string? texto = UrlProtector.Descifrar(token);

            if (texto is null
                || !int.TryParse(texto, out int idVenta)
                || idVenta <= 0)
            {
                NoEncontrado = true;
                return;
            }

            Comprobante = _fachada.ObtenerComprobante(idVenta);

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