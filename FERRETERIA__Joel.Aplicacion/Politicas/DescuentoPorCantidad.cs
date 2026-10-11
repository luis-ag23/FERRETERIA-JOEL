namespace FERRETERIA__Joel.Aplicacion.Politicas
{
    /// <summary>
    /// Estrategia concreta: aplica un porcentaje fijo cuando la venta
    /// alcanza una cantidad mínima de unidades.
    /// </summary>
    public sealed class DescuentoPorCantidad : IPoliticaDescuento
    {
        private readonly int _cantidadMinima;
        private readonly decimal _porcentaje;

        public DescuentoPorCantidad(int cantidadMinima, decimal porcentaje)
        {
            _cantidadMinima = cantidadMinima;
            _porcentaje = porcentaje;
        }

        public string Nombre =>
            $"Descuento por cantidad desde {_cantidadMinima} unidades ({_porcentaje}%)";

        public ResultadoPoliticaDescuento Calcular(
            decimal subtotal,
            int totalUnidades)
        {
            if (_cantidadMinima <= 0 || totalUnidades < _cantidadMinima)
            {
                return new ResultadoPoliticaDescuento(0m, 0m);
            }

            decimal monto = Math.Round(subtotal * _porcentaje / 100m, 2);

            return new ResultadoPoliticaDescuento(_porcentaje, monto);
        }
    }
}
