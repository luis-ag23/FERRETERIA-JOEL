using System.Text.RegularExpressions;
using FERRETERIA__Joel.Dominio.ConfiguracionValidacion;

namespace FERRETERIA__Joel.Dominio.Validaciones
{
    public class ProductoValidaciones
    {
        public bool EsCodigoValido(string codigo)
        {
            return !string.IsNullOrWhiteSpace(codigo)
                && codigo.Trim().Length <= ConfiguracionProducto.CodigoMaxLength;
        }

        // Nombre del producto: se permiten letras, números, espacios y símbolos
        // habituales en descripciones de ferretería (oz, 4-1/2", 300ml, etc.).
        private static readonly Regex FormatoNombreProducto =
            new(@"^[\p{L}\p{N}\s.,'""\-&()/#+*]+$",
                RegexOptions.Compiled);

        public bool EsNombreValido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return false;
            }

            string limpio = nombre.Trim();

            if (limpio.Length > ConfiguracionProducto.NombreMaxLength)
            {
                return false;
            }

            return FormatoNombreProducto.IsMatch(limpio);
        }

        public bool EsDescripcionValida(string? descripcion)
        {
            return string.IsNullOrWhiteSpace(descripcion)
                || descripcion.Trim().Length <= ConfiguracionProducto.DescripcionMaxLength;
        }

        public bool EsMarcaValida(int idMarca)
        {
            return idMarca >= ConfiguracionProducto.MarcaMinima;
        }

        public bool EsUnidadMedidaValida(string? unidadMedida)
        {
            return !string.IsNullOrWhiteSpace(unidadMedida)
                && unidadMedida.Trim().Length <= ConfiguracionProducto.UnidadMedidaMaxLength;
        }

        public bool EsPrecioValido(decimal precioVenta)
        {
            return precioVenta > ConfiguracionProducto.PrecioVentaMinimo;
        }

        public bool EsCategoriaValida(int idCategoria)
        {
            return idCategoria >= ConfiguracionProducto.CategoriaMinima;
        }

        public bool EsCantidadStockValido(int cantidadStock)
        {
            return cantidadStock >= ConfiguracionProducto.StockMinimo;
        }

        public bool EsEmpleadoValido(int idEmpleadoResponsable)
        {
            return idEmpleadoResponsable >= ConfiguracionProducto.EmpleadoMinimo;
        }
    }
}