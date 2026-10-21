using FERRETERIA__Joel.Dominio.Utilidades;

namespace FERRETERIA__Joel.Pruebas.Utilidades
{
    public class NombreAbreviadoPruebas
    {
        [Theory]
        [InlineData("María López", "M.López")]
        [InlineData("  maría   lópez ", "M.López")]
        [InlineData("Usuario De Prueba", "U.Prueba")]
        [InlineData("JUAN PÉREZ", "J.Pérez")]
        [InlineData("Admin", "Admin")]
        [InlineData("Empleado #5", "Empleado #5")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void De_AbreviaNombreCompleto(string? nombre, string esperado)
        {
            Assert.Equal(esperado, NombreAbreviado.De(nombre));
        }
    }
}