using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Infraestructura.Servicios
{
    /// <summary>
    /// Identificador real de la réplica del monolito. Toma el nombre del
    /// contenedor (<c>HOSTNAME</c>), que Docker Compose asigna de forma
    /// distinta a cada réplica (app1, app2, app3, app4). Cuando la aplicación
    /// se ejecuta fuera de Docker recurre al nombre de la máquina, que también
    /// distingue cada proceso.
    ///
    /// No es un identificador fijo simulado: refleja quién atendió la
    /// petición en realidad.
    /// </summary>
    public class IdentificadorReplica : IIdentificadorReplica
    {
        private readonly Lazy<string> _id;

        public IdentificadorReplica()
        {
            _id = new Lazy<string>(ResolverId);
        }

        public string Id => _id.Value;

        /// <summary>
        /// Resuelve el identificador de la réplica. Se consulta, en este
        /// orden: la variable de entorno <c>HOSTNAME</c> (la define Docker
        /// Compose para cada réplica), el archivo <c>/etc/hostname</c> del
        /// contenedor y, como último recurso, el nombre de la máquina.
        /// </summary>
        private static string ResolverId()
        {
            string? hostname = Environment.GetEnvironmentVariable("HOSTNAME");

            if (!string.IsNullOrWhiteSpace(hostname))
            {
                return hostname.Trim();
            }

            string? nombreContenedor = LeerArchivoHostname();

            if (!string.IsNullOrWhiteSpace(nombreContenedor))
            {
                return nombreContenedor;
            }

            return Environment.MachineName;
        }

        private static string? LeerArchivoHostname()
        {
            try
            {
                if (!File.Exists("/etc/hostname"))
                {
                    return null;
                }

                return File.ReadAllText("/etc/hostname").Trim();
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }
    }
}
