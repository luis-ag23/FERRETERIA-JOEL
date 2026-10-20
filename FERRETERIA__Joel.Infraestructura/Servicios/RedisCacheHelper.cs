using System.Text.Json;
using StackExchange.Redis;

namespace FERRETERIA__Joel.Infraestructura.Servicios;

public class RedisCacheHelper
{
    private readonly IDatabase _db;
    private readonly string _instancePrefix;

    public RedisCacheHelper(RedisConnection connection, string instancePrefix = "ferreteria:")
    {
        _db = connection.Db;
        _instancePrefix = instancePrefix;
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
        if (ttl.HasValue)
            await _db.StringSetAsync(redisKey, json, ttl.Value);
        else
            await _db.StringSetAsync(redisKey, json);
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