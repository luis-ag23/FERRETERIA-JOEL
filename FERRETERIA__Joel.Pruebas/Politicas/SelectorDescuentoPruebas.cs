using FERRETERIA__Joel.Aplicacion.Politicas;

namespace FERRETERIA__Joel.Pruebas.Politicas
{
    public class SelectorDescuentoPruebas
    {
        [Fact]
        public void DescuentoPorMonto_NoAplica_BajoElUmbral()
        {
            DescuentoPorMonto politica = new(montoMinimo: 1000m, porcentaje: 5m);

            ResultadoPoliticaDescuento resultado = politica.Calcular(999m, 1);

            Assert.Equal(0m, resultado.Monto);
            Assert.Equal(0m, resultado.Porcentaje);
        }

        [Fact]
        public void DescuentoPorMonto_Aplica_AlAlcanzarElUmbral()
        {
            DescuentoPorMonto politica = new(montoMinimo: 1000m, porcentaje: 5m);

            ResultadoPoliticaDescuento resultado = politica.Calcular(2000m, 1);

            Assert.Equal(100m, resultado.Monto);
            Assert.Equal(5m, resultado.Porcentaje);
        }

        [Fact]
        public void DescuentoPorCantidad_Aplica_AlAlcanzarLasUnidades()
        {
            DescuentoPorCantidad politica = new(cantidadMinima: 12, porcentaje: 3m);

            ResultadoPoliticaDescuento resultado = politica.Calcular(600m, 12);

            Assert.Equal(18m, resultado.Monto);
        }

        [Fact]
        public void DescuentoPorCantidad_NoAplica_BajoLasUnidades()
        {
            DescuentoPorCantidad politica = new(cantidadMinima: 12, porcentaje: 3m);

            ResultadoPoliticaDescuento resultado = politica.Calcular(600m, 11);

            Assert.Equal(0m, resultado.Monto);
        }

        [Fact]
        public void Selector_EligeElMayorBeneficio()
        {
            SelectorDescuento selector = new(new IPoliticaDescuento[]
            {
                new DescuentoPorMonto(montoMinimo: 1000m, porcentaje: 5m),
                new DescuentoPorCantidad(cantidadMinima: 12, porcentaje: 10m)
            });

            SeleccionDescuento seleccion = selector.Seleccionar(200m, 12);

            Assert.Equal(20m, seleccion.Monto);
            Assert.Equal(10m, seleccion.Porcentaje);
        }

        [Fact]
        public void Selector_PrefiereMonto_CuandoRindeMas()
        {
            SelectorDescuento selector = new(new IPoliticaDescuento[]
            {
                new DescuentoPorMonto(montoMinimo: 1000m, porcentaje: 10m),
                new DescuentoPorCantidad(cantidadMinima: 12, porcentaje: 3m)
            });

            SeleccionDescuento seleccion = selector.Seleccionar(1000m, 12);

            Assert.Equal(100m, seleccion.Monto);
        }

        [Fact]
        public void Selector_SinPoliticasAplicables_DevuelveCero()
        {
            SelectorDescuento selector = new(new IPoliticaDescuento[]
            {
                new DescuentoPorMonto(montoMinimo: 5000m, porcentaje: 5m),
                new DescuentoPorCantidad(cantidadMinima: 50, porcentaje: 3m)
            });

            SeleccionDescuento seleccion = selector.Seleccionar(100m, 1);

            Assert.Equal(0m, seleccion.Monto);
            Assert.Equal("Sin descuento", seleccion.Politica);
        }

        [Fact]
        public void Selector_SinPoliticasRegistradas_DevuelveCero()
        {
            SelectorDescuento selector = new(Array.Empty<IPoliticaDescuento>());

            SeleccionDescuento seleccion = selector.Seleccionar(100m, 1);

            Assert.Equal(0m, seleccion.Monto);
        }
    }
}
