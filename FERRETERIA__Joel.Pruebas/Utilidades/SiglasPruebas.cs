using FERRETERIA__Joel.Dominio.Utilidades;

namespace FERRETERIA__Joel.Pruebas.Utilidades
{
    public class SiglasPruebas
    {
        [Theory]
        [InlineData("Juan Pérez Vargas", "JPV")]
        [InlineData("Usuario De Prueba", "UDP")]
        [InlineData("  maría   lópez ", "ML")]
        [InlineData("Admin", "A")]
        [InlineData("Empleado #5", "E5")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void De_ReduceNombreASiglas(string? nombre, string esperado)
        {
            Assert.Equal(esperado, Siglas.De(nombre));
        }
    }
}