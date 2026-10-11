namespace FERRETERIA__Joel.Aplicacion.Politicas
{
    /// <summary>
    /// Parámetros de las políticas de descuento vigentes. Es la única
    /// fuente de verdad: la composición en Program registra las
    /// estrategias a partir de estos valores y la interfaz los expone
    /// para previsualizar el descuento sin duplicar reglas.
    /// </summary>
    public sealed class ConfiguracionDescuentos
    {
        public decimal MontoMinimo { get; init; } = 1000m;

        public decimal PorcentajePorMonto { get; init; } = 5m;

        public int CantidadMinima { get; init; } = 12;

        public decimal PorcentajePorCantidad { get; init; } = 3m;
    }
}
