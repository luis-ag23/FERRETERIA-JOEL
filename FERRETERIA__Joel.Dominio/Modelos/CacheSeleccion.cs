namespace FERRETERIA__Joel.Dominio.Modelos
{
    /// <summary>
    /// Resultado de consultar una entrada en la caché distribuida. Distingue
    /// entre un HIT (el valor venía de Redis) y un MISS (no había nada y el
    /// llamador debe ir a MySQL). El origen se expone como texto para poder
    /// presentarlo en logs, en la vista de diagnóstico y en las evidencias de
    /// la defensa.
    /// </summary>
    public sealed class ResultadoCache<T>
    {
        /// <summary>Valor almacenado, o el valor por defecto si hubo MISS.</summary>
        public T? Valor { get; init; }

        /// <summary><c>true</c> si el valor se obtuvo de Redis (HIT).</summary>
        public bool EsHit { get; init; }

        /// <summary>Clave consultada en Redis.</summary>
        public string Clave { get; init; } = string.Empty;

        /// <summary>Origen real del dato: <c>REDIS</c> o <c>MYSQL</c>.</summary>
        public string Origen => EsHit ? "REDIS" : "MYSQL";
    }

    /// <summary>
    /// Evento de caché (HIT, MISS o INVALIDACIÓN) registrado por una réplica.
    /// Se almacena en Redis para que el rastro sea compartido por todas las
    /// réplicas y sirva como evidencia del comportamiento de la caché.
    /// </summary>
    public sealed class EventoCache
    {
        /// <summary>Momento en que ocurrió el evento (UTC).</summary>
        public DateTime Fecha { get; init; }

        /// <summary>Clave de caché afectada.</summary>
        public string Clave { get; init; } = string.Empty;

        /// <summary>Tipo de evento: HIT, MISS o INVALIDATE.</summary>
        public string Resultado { get; init; } = string.Empty;

        /// <summary>Identificador de la réplica que produjo el evento.</summary>
        public string Replica { get; init; } = string.Empty;

        /// <summary>Origen del dato: REDIS o MYSQL.</summary>
        public string Origen { get; init; } = string.Empty;

        /// <summary>Cantidad de filas afectadas por la consulta.</summary>
        public int Filas { get; init; }
    }

    /// <summary>
    /// Clave de caché encontrada en Redis junto con su tiempo de vida
    /// restante. Se usa en la vista de diagnóstico.
    /// </summary>
    public sealed class ClaveCache
    {
        /// <summary>Clave completa en Redis.</summary>
        public string Clave { get; init; } = string.Empty;

        /// <summary>Segundos restantes antes de expirar (-1 si no expira).</summary>
        public long TtlSegundos { get; init; }

        /// <summary>Tamaño del valor almacenado en bytes.</summary>
        public long Longitud { get; init; }

        /// <summary>Clave sin el prefijo de instancia de la aplicación.</summary>
        public string ClaveLogica =>
            Clave.Contains(':')
                ? Clave[(Clave.IndexOf(':') + 1)..]
                : Clave;
    }
}
