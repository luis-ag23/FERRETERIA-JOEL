using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Resultado del inicio de sesión: indica si fue exitoso, el mensaje a
    /// mostrar y, cuando corresponde, el empleado autenticado.
    /// </summary>
    public class ResultadoInicioSesion
    {
        public bool Exitoso { get; init; }

        public string Mensaje { get; init; } = string.Empty;

        public Empleado? Empleado { get; init; }
    }
}
