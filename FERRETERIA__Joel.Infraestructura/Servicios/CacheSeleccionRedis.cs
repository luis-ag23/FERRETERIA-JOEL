using FERRETERIA__Joel.Dominio.Modelos;
using FERRETERIA__Joel.Dominio.Puertos;
using Microsoft.Extensions.Logging;

namespace FERRETERIA__Joel.Infraestructura.Servicios;

/// <summary>
/// Adaptador del puerto <see cref="ICacheSeleccion"/> sobre Redis.
///
/// Aplica el patrón <em>Cache-Aside</em> y deja rastro de cada acceso: un
/// HIT o un MISS quedan registrados en el log con la clave, la réplica que
/// atendió la petición y el origen real del dato. Además se guardan en el
/// propio Redis para que el rastro sea compartido entre réplicas y sirva de
/// evidencia en la vista de diagnóstico.
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

            await RegistrarEventoAsync(new EventoCache
            {
                Fecha = DateTime.UtcNow,
                Clave = clave,
                Resultado = esHit ? "HIT" : "MISS",
                Replica = replica,
                Origen = esHit ? "REDIS" : "MYSQL",
                Filas = esHit ? ContarFilas(valor) : 0
            });

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

            await RegistrarEventoAsync(new EventoCache
            {
                Fecha = DateTime.UtcNow,
                Clave = prefijo,
                Resultado = "INVALIDATE",
                Replica = _replica.Id,
                Origen = "REDIS"
            });
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

    public Task<List<EventoCache>> ObtenerEventosAsync(int cantidad = 50)
    {
        try
        {
            return _cache.ObtenerEventosAsync(cantidad);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "No se pudieron leer los eventos de caché");
            return Task.FromResult(new List<EventoCache>());
        }
    }

    public Task<List<ClaveCache>> ObtenerClavesAsync(string prefijo)
    {
        try
        {
            return _cache.ObtenerClavesAsync(prefijo);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "No se pudieron listar las claves de caché");
            return Task.FromResult(new List<ClaveCache>());
        }
    }

    private async Task RegistrarEventoAsync(EventoCache evento)
    {
        try
        {
            await _cache.RegistrarEventoAsync(evento);
        }
        catch (Exception ex)
        {
            // El registro del evento es informativo: si falla, no debe
            // interrumpir la operación principal.
            _log.LogDebug(ex, "No se pudo registrar el evento de caché");
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
