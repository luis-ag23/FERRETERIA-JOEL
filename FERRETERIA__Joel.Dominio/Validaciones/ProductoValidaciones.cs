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

        public bool EsNombreValido(string nombre)
        {
            return ValidacionesTexto.EsNombreValido(
                nombre,
                ConfiguracionProducto.NombreMaxLength);
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

        public bool EsEmpleadoValido(int idEmpleadoResponsable)
        {
            return idEmpleadoResponsable >= ConfiguracionProducto.EmpleadoMinimo;
        }
    }
}