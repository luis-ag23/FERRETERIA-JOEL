using FERRETERIA__Joel.Dominio.Puertos;

namespace FERRETERIA__Joel.Infraestructura.Servicios
{
    /// <summary>
    /// Identificador real de la réplica del monolito.
    ///
    /// <para>
    /// Se compone de dos partes:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <b>Nombre</b> (<see cref="Id"/>): el nombre del contenedor
    /// (<c>HOSTNAME</c>), que Docker Compose asigna de forma distinta a cada
    /// réplica (app1, app2, app3, app4). Cuando la aplicación se ejecuta fuera
    /// de Docker recurre al nombre de la máquina, que también distingue cada
    /// proceso.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <b>Identificador único</b> (<see cref="IdInstancia"/>): un token
    /// aleatorio de 8 caracteres hexadecimales que se genera en el momento en
    /// que la réplica arranca. Nunca se repite entre réplicas ni entre dos
    /// arranques de la misma réplica, de modo que ver el identificador en la
    /// interfaz o en la cabecera <c>X-Replica-Id</c> demuestra que son
    /// procesos distintos y no una respuesta simulada.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// No es un valor fijo simulado: refleja quién atendió la petición en
    /// realidad.
    /// </summary>
    public class IdentificadorReplica : IIdentificadorReplica
    {
        private readonly Lazy<string> _id;
        private readonly Lazy<string> _idInstancia;

        public IdentificadorReplica()
        {
            _id = new Lazy<string>(ResolverNombre);
            _idInstancia = new Lazy<string>(ConstruirIdInstancia);
        }

        /// <summary>Nombre de la réplica: <c>app1</c>, <c>app2</c>, ...</summary>
        public string Id => _id.Value;

        /// <summary>
        /// Identificador único de la réplica, generado al arrancar:
        /// <c>app1-3F2A9C77</c>. Es el que viaja en la cabecera
        /// <c>X-Replica-Id</c> y el que muestra la interfaz.
        /// </summary>
        public string IdInstancia => _idInstancia.Value;

        /// <summary>
        /// Resuelve el nombre de la réplica. Se consulta, en este orden: la
        /// variable de entorno <c>HOSTNAME</c> (la define Docker Compose para
        /// cada réplica), el archivo <c>/etc/hostname</c> del contenedor y,
        /// como último recurso, el nombre de la máquina.
        /// </summary>
        private static string ResolverNombre()
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

        /// <summary>
        /// Construye el identificador único: el nombre de la réplica más un
        /// token aleatorio generado en este arranque.
        /// </summary>
        private string ConstruirIdInstancia()
        {
            string token = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

            return $"{_id.Value}-{token}";
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
