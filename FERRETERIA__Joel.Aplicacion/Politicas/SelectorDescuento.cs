namespace FERRETERIA__Joel.Aplicacion.Politicas
{
    /// <summary>
    /// Descuento finalmente elegido entre todas las políticas.
    /// </summary>
    public readonly record struct SeleccionDescuento(
        string Politica,
        decimal Porcentaje,
        decimal Monto);

    /// <summary>
    /// Contexto del patrón Strategy. Evalúa todas las políticas
    /// registradas y aplica la que otorgue el mayor beneficio; si
    /// ninguna aplica, el descuento es cero.
    /// </summary>
    public sealed class SelectorDescuento
    {
        private readonly IReadOnlyList<IPoliticaDescuento> _politicas;

        public SelectorDescuento(IEnumerable<IPoliticaDescuento> politicas)
        {
            _politicas = politicas.ToList();
        }

        public SeleccionDescuento Seleccionar(
            decimal subtotal,
            int totalUnidades)
        {
            SeleccionDescuento mejor = new("Sin descuento", 0m, 0m);

            foreach (IPoliticaDescuento politica in _politicas)
            {
                ResultadoPoliticaDescuento resultado =
                    politica.Calcular(subtotal, totalUnidades);

                if (resultado.Monto > mejor.Monto)
                {
                    mejor = new SeleccionDescuento(
                        politica.Nombre,
                        resultado.Porcentaje,
                        resultado.Monto);
                }
            }

            return mejor;
        }
    }
}
