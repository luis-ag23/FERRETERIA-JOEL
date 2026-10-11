using FERRETERIA__Joel.Dominio.ConfiguracionValidacion;
using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Dominio.Validaciones
{
    public class MarcaValidaciones
    {
        public bool EsNombreValido(string? nombre)
        {
            return ValidacionesTexto.EsNombreValido(
                nombre,
                ConfiguracionMarca.NombreMaxLength);
        }

        public bool EsDescripcionValida(string? descripcion)
        {
            return string.IsNullOrWhiteSpace(descripcion)
                || descripcion.Trim().Length <= ConfiguracionMarca.DescripcionMaxLength;
        }

        public bool EsPaisOrigenValido(string? paisOrigen)
        {
            return !string.IsNullOrWhiteSpace(paisOrigen)
                && paisOrigen.Trim().Length <= ConfiguracionMarca.PaisOrigenMaxLength;
        }

        public bool EsCalidadValida(string? calidad)
        {
            return !string.IsNullOrWhiteSpace(calidad)
                && calidad.Trim().Length <= ConfiguracionMarca.CalidadMaxLength;
        }

        public bool EsEmpleadoValido(int idEmpleadoResponsable)
        {
            return idEmpleadoResponsable >= ConfiguracionMarca.EmpleadoMinimo;
        }

        public bool EsValida(Marca marca)
        {
            return EsNombreValido(marca.Nombre) &&
                   EsDescripcionValida(marca.Descripcion) &&
                   EsPaisOrigenValido(marca.PaisOrigen) &&
                   EsCalidadValida(marca.Calidad) &&
                   EsEmpleadoValido(marca.IdEmpleadoResponsable);
        }
    }
}