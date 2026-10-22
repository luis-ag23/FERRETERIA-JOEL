namespace FERRETERIA__Joel.Dominio.Modelos
{
    /// <summary>
    /// Resultado de consultar una entrada en la caché distribuida. Distingue
    /// entre un HIT (el valor venía de Redis) y un MISS (no había nada y el
    /// llamador debe ir a MySQL). El origen se expone como texto para poder
    /// presentarlo en los logs de la aplicación.
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
}
