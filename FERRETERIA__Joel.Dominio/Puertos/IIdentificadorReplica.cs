namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Identificador real de la réplica del monolito que está ejecutando la
    /// petición. La implementación real lo toma del nombre del contenedor
    /// (<c>HOSTNAME</c>); nunca es un valor fijo simulado.
    ///
    /// Se declara en el dominio porque tanto la aplicación (registro de
    /// accesos a la caché) como la capa web (identificador mostrado en la
    /// interfaz) necesitan conocerlo, y ninguno debe depender de la
    /// infraestructura concreta.
    /// </summary>
    public interface IIdentificadorReplica
    {
        /// <summary>
        /// Nombre de la réplica en ejecución, por ejemplo <c>app1</c>.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Identificador único de la réplica, generado en el momento en que
        /// arranca, por ejemplo <c>app1-3F2A9C77</c>.
        ///
        /// Es lo que se muestra en la interfaz y lo que viaja en la cabecera
        /// <c>X-Replica-Id</c>: al refrescar la página se ve un identificador
        /// distinto en cada petición, lo que demuestra que hay varias
        /// instancias reales repartiendo el tráfico.
        /// </summary>
        string IdInstancia { get; }
    }
}
