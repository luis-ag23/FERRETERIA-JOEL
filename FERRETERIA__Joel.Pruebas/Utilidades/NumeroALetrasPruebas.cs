using FERRETERIA__Joel.Dominio.Utilidades;

namespace FERRETERIA__Joel.Pruebas.Utilidades
{
    public class NumeroALetrasPruebas
    {
        [Theory]
        [InlineData(0, "cero bolivianos con 00/100")]
        [InlineData(1, "un boliviano con 00/100")]
        [InlineData(1.50, "un boliviano con 50/100")]
        [InlineData(2.05, "dos bolivianos con 05/100")]
        [InlineData(16, "dieciséis bolivianos con 00/100")]
        [InlineData(21, "veintiún bolivianos con 00/100")]
        [InlineData(31, "treinta y un bolivianos con 00/100")]
        [InlineData(42.99, "cuarenta y dos bolivianos con 99/100")]
        [InlineData(100, "cien bolivianos con 00/100")]
        [InlineData(101, "ciento un bolivianos con 00/100")]
        [InlineData(202.50, "doscientos dos bolivianos con 50/100")]
        [InlineData(500, "quinientos bolivianos con 00/100")]
        [InlineData(700, "setecientos bolivianos con 00/100")]
        [InlineData(1000, "mil bolivianos con 00/100")]
        [InlineData(2000, "dos mil bolivianos con 00/100")]
        [InlineData(1523.75, "mil quinientos veintitrés bolivianos con 75/100")]
        [InlineData(1234567.89, "un millón doscientos treinta y cuatro mil quinientos sesenta y siete bolivianos con 89/100")]
        public void EnLetras_VariosMontos_DevuelveLiteralEsperado(
            double monto,
            string esperado)
        {
            string resultado = NumeroALetras.EnLetras((decimal)monto);

            Assert.Equal(esperado, resultado);
        }

        [Theory]
        [InlineData(0.001, "cero bolivianos con 00/100")]
        [InlineData(0.005, "cero bolivianos con 01/100")]
        [InlineData(0.335, "cero bolivianos con 34/100")]
        public void EnLetras_RedondeaADosDecimales(double monto, string esperado)
        {
            string resultado = NumeroALetras.EnLetras((decimal)monto);

            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void EnLetras_ImporteNegativo_AnteponeMenos()
        {
            string resultado = NumeroALetras.EnLetras(-5.25m);

            Assert.Equal(
                "menos cinco bolivianos con 25/100",
                resultado);
        }
    }
}