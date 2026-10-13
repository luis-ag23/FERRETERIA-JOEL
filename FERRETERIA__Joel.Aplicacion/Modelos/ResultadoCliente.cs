using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    public class ResultadoCliente
    {
        public bool Exito { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public Cliente? Cliente { get; set; }

        public Dictionary<string, string> Errores { get; set; } = new();
    }
}