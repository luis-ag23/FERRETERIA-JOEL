using FERRETERIA__Joel.Dominio.Modelos;

namespace FERRETERIA__Joel.Dominio.Puertos
{
    /// <summary>
    /// Puerto de la caché distribuida compartida por las réplicas del
    /// monolito. La aplicación y el dominio dependen de esta abstracción;
    /// la infraestructura decide el transporte real (Redis).
    ///
    /// Implementa el patrón <em>Cache-Aside</em>: el repositorio consulta la
    /// caché, y solo ante un MISS recorre a MySQL. La caché se observa
    /// directamente en Redis (contenedor <c>fj-redis</c>), no desde la
    /// aplicación.
    /// </summary>
    public interface ICacheSeleccion
    {
        /// <summary>
        /// Intenta obtener un valor de la caché. Devuelve un
        /// <see cref="ResultadoCache{T}"/> que indica si hubo HIT o MISS.
        /// Nunca lanza por una caída de Redis: ante un error de transporte
        /// degrada a MISS para no interrumpir la operación principal.
        /// </summary>
        Task<ResultadoCache<T>> ObtenerAsync<T>(string clave);

        /// <summary>
        /// Almacena un valor en la caché. Si no se indica expiración se
        /// aplica la predeterminada configurada para la aplicación.
        /// </summary>
        Task GuardarAsync<T>(string clave, T valor, TimeSpan? ttl = null);

        /// <summary>
        /// Invalida todas las claves que empiecen por el prefijo indicado.
        /// Se invoca después de cualquier escritura sobre los datos cacheados
        /// (alta, actualización o baja lógica).
        /// </summary>
        Task InvalidarPorPrefijoAsync(string prefijo);
    }
}
