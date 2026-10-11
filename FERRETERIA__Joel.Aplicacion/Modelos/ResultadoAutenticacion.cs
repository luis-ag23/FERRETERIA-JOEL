using FERRETERIA__Joel.Dominio.Entidades;

namespace FERRETERIA__Joel.Aplicacion.Modelos
{
    /// <summary>
    /// Resultado de un intento de autenticación. Cuando es exitoso
    /// transporta el empleado autenticado para que la capa Web emita
    /// las claims (identidad, nombre y rol).
    /// </summary>
    public class ResultadoAutenticacion
    {
        public bool Exito { get; private init; }

        public string Mensaje { get; private init; } = string.Empty;

        public Empleado? Empleado { get; private init; }

        public static ResultadoAutenticacion Correcto(Empleado empleado)
        {
            return new ResultadoAutenticacion
            {
                Exito = true,
                Empleado = empleado
            };
        }

        public static ResultadoAutenticacion Fallido(string mensaje)
        {
            return new ResultadoAutenticacion
            {
                Exito = false,
                Mensaje = mensaje
            };
        }
    }
}
