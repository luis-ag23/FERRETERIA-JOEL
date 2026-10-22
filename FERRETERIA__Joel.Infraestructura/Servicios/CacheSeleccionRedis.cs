using FERRETERIA__Joel.Dominio.Modelos;
using FERRETERIA__Joel.Dominio.Puertos;
using Microsoft.Extensions.Logging;

namespace FERRETERIA__Joel.Infraestructura.Servicios;

/// <summary>
/// Adaptador del puerto <see cref="ICacheSeleccion"/> sobre Redis.
///
/// Aplica el patrón <em>Cache-Aside</em>: ante un MISS se lee de MySQL y el
/// resultado se guarda en Redis con expiración, de modo que la siguiente
/// réplica que atienda la misma consulta ya encuentra el dato cacheado.
///
/// La caché se observa directamente en el contenedor de Redis (las claves y
/// sus TTL), no desde la aplicación.
///
/// Ante una caída de Redis el adaptador degrada a MISS: la aplicación sigue
/// funcionando contra MySQL, que es el sistema de registro.
/// </summary>
public class CacheSeleccionRedis : ICacheSeleccion
{
    private readonly RedisCacheHelper _cache;
    private readonly IIdentificadorReplica _replica;
    private readonly ILogger<CacheSeleccionRedis> _log;

    public CacheSeleccionRedis(
        RedisCacheHelper cache,
        IIdentificadorReplica replica,
        ILogger<CacheSeleccionRedis> log)
    {
        _cache = cache;
        _replica = replica;
        _log = log;
    }

    public async Task<ResultadoCache<T>> ObtenerAsync<T>(string clave)
    {
        string replica = _replica.Id;

        try
        {
            T? valor = await _cache.GetAsync<T>(clave);

            bool esHit = valor is not null;

            if (esHit)
            {
                _log.LogInformation(
                    "CACHE HIT | clave={Clave} | replica={Replica} | origen=REDIS | filas={Filas}",
                    clave,
                    replica,
                    ContarFilas(valor));
            }
            else
            {
                _log.LogInformation(
                    "CACHE MISS | clave={Clave} | replica={Replica} | origen=MYSQL",
                    clave,
                    replica);
            }

            return new ResultadoCache<T>
            {
                Valor = valor,
                EsHit = esHit,
                Clave = clave
            };
        }
        catch (Exception ex)
        {
            _log.LogWarning(
                ex,
                "CACHE ERROR | clave={Clave} | replica={Replica} | degradado a MISS",
                clave,
                replica);

            return new ResultadoCache<T>
            {
                Valor = default,
                EsHit = false,
                Clave = clave
            };
        }
    }

    public async Task GuardarAsync<T>(string clave, T valor, TimeSpan? ttl = null)
    {
        TimeSpan expiracion = ttl ?? _cache.TtlPorDefecto;

        try
        {
            await _cache.SetAsync(clave, valor, expiracion);

            _log.LogInformation(
                "CACHE WRITE | clave={Clave} | replica={Replica} | ttl={Ttl} | origen=REDIS",
                clave,
                _replica.Id,
                expiracion);
        }
        catch (Exception ex)
        {
            _log.LogWarning(
                ex,
                "CACHE WRITE ERROR | clave={Clave} | replica={Replica}",
                clave,
                _replica.Id);
        }
    }

    public async Task InvalidarPorPrefijoAsync(string prefijo)
    {
        try
        {
            await _cache.RemoveByPatternAsync(prefijo);

            _log.LogInformation(
                "CACHE INVALIDATE | prefijo={Prefijo} | replica={Replica} | origen=REDIS",
                prefijo,
                _replica.Id);
        }
        catch (Exception ex)
        {
            _log.LogWarning(
                ex,
                "CACHE INVALIDATE ERROR | prefijo={Prefijo} | replica={Replica}",
                prefijo,
                _replica.Id);
        }
    }

    private static int ContarFilas<T>(T? valor)
    {
        if (valor is System.Collections.ICollection coleccion)
        {
            return coleccion.Count;
        }

        return valor is null ? 0 : 1;
    }
}
