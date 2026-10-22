using System.Text.Json;
using StackExchange.Redis;

namespace FERRETERIA__Joel.Infraestructura.Servicios;

/// <summary>
/// Utilidad de bajo nivel sobre Redis: lectura, escritura y borrado por patrón.
/// El prefijo de instancia ("ferreteria:") permite que varias aplicaciones
/// convivan en el mismo servidor Redis sin colisionar.
///
/// Las claves que se guardan son las consultas cacheadas del catálogo
/// (productos, categorías, marcas y proveedores). Se observan directamente en
/// el contenedor de Redis:
///     docker exec -it fj-redis redis-cli --scan --pattern "ferreteria:*"
/// </summary>
public class RedisCacheHelper
{
    private readonly IDatabase _db;
    private readonly string _instancePrefix;

    /// <summary>Expiración predeterminada para las entradas de caché.</summary>
    public TimeSpan TtlPorDefecto { get; }

    public RedisCacheHelper(
        RedisConnection connection,
        string instancePrefix = "ferreteria:",
        TimeSpan? ttlPorDefecto = null)
    {
        _db = connection.Db;
        _instancePrefix = instancePrefix;
        TtlPorDefecto = ttlPorDefecto ?? TimeSpan.FromMinutes(5);
    }

    private string GetKey(string key) => $"{_instancePrefix}{key}";

    public async Task<T?> GetAsync<T>(string key)
    {
        var redisKey = GetKey(key);
        var value = await _db.StringGetAsync(redisKey);
        if (!value.HasValue)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(value.ToString());
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        var redisKey = GetKey(key);
        var json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(redisKey, json, ttl ?? TtlPorDefecto);
    }

    public async Task RemoveAsync(string key)
    {
        var redisKey = GetKey(key);
        await _db.KeyDeleteAsync(redisKey);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // Buscar claves con patrón usando SCAN - requiere servidor
        var connection = _db.Multiplexer;
        var endpoints = connection.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = connection.GetServer(endpoint);
            var fullPattern = GetKey(pattern);
            var keys = server.Keys(pattern: fullPattern, pageSize: 1000).ToArray();
            if (keys.Length > 0)
            {
                await _db.KeyDeleteAsync(keys);
            }
        }
    }
}
