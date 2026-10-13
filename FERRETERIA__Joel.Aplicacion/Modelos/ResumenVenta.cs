namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Totales calculados de una venta: subtotal, descuento aplicado
    /// por la política vigente y monto final a pagar.
    /// </summary>
    public class ResumenVenta
    {
        public decimal Subtotal { get; init; }

        public decimal MontoDescuento { get; init; }

        public decimal PorcentajeDescuento { get; init; }

        public decimal MontoTotal { get; init; }

        public string PoliticaDescuento { get; init; } = "Sin descuento";
    }
}
