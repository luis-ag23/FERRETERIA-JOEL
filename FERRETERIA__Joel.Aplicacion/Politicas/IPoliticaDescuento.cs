namespace FERRETERIA__Joel.Aplicacion.Politicas
{
    /// <summary>
    /// Resultado del cálculo de una política de descuento concreta.
    /// </summary>
    public readonly record struct ResultadoPoliticaDescuento(
        decimal Porcentaje,
        decimal Monto);

    /// <summary>
    /// Patrón Strategy: encapsula un algoritmo de descuento. El
    /// <see cref="SelectorDescuento"/> es el contexto que decide cuál
    /// política aplicar. Agregar una regla nueva no obliga a modificar
    /// las existentes (Open/Closed).
    /// </summary>
    public interface IPoliticaDescuento
    {
        string Nombre { get; }

        /// <summary>
        /// Calcula el descuento sobre el subtotal y el total de unidades.
        /// Devuelve 0 cuando la política no aplica.
        /// </summary>
        ResultadoPoliticaDescuento Calcular(decimal subtotal, int totalUnidades);
    }
}
