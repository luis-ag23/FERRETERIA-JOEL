namespace FERRETERIA__Joel.Aplicacion.Politicas
{
    /// <summary>
    /// Estrategia concreta: aplica un porcentaje fijo cuando el
    /// subtotal alcanza un monto mínimo.
    /// </summary>
    public sealed class DescuentoPorMonto : IPoliticaDescuento
    {
        private readonly decimal _montoMinimo;
        private readonly decimal _porcentaje;

        public DescuentoPorMonto(decimal montoMinimo, decimal porcentaje)
        {
            _montoMinimo = montoMinimo;
            _porcentaje = porcentaje;
        }

        public string Nombre =>
            $"Descuento por monto desde {_montoMinimo:C} ({_porcentaje}%)";

        public ResultadoPoliticaDescuento Calcular(
            decimal subtotal,
            int totalUnidades)
        {
            if (_montoMinimo <= 0 || subtotal < _montoMinimo)
            {
                return new ResultadoPoliticaDescuento(0m, 0m);
            }

            decimal monto = Math.Round(subtotal * _porcentaje / 100m, 2);

            return new ResultadoPoliticaDescuento(_porcentaje, monto);
        }
    }
}
