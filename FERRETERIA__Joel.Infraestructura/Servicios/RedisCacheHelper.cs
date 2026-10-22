using System.Text.Json;
using FERRETERIA__Joel.Dominio.Modelos;
using StackExchange.Redis;

namespace FERRETERIA__Joel.Infraestructura.Servicios;

/// <summary>
/// Utilidad de bajo nivel sobre Redis: lectura, escritura, borrado por patrón
/// y soporte de los metadatos de las claves (TTL y tamaño). El prefijo de
/// instancia ("ferreteria:") permite que varias aplicaciones convivan en el
/// mismo servidor Redis sin colisionar.
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

    /// <summary>
    /// Devuelve las claves que coinciden con el prefijo indicado junto con su
    /// tiempo de vida restante y su tamaño en bytes.
    /// </summary>
    public async Task<List<ClaveCache>> ObtenerClavesAsync(string prefijo)
    {
        List<ClaveCache> resultado = new();

        var connection = _db.Multiplexer;
        var endpoints = connection.GetEndPoints();
        string fullPattern = GetKey(prefijo);

        foreach (var endpoint in endpoints)
        {
            var server = connection.GetServer(endpoint);
            var keys = server.Keys(pattern: fullPattern, pageSize: 500).ToArray();

            foreach (var key in keys)
            {
                TimeSpan? ttl = await _db.KeyTimeToLiveAsync(key);
                long longitud = await _db.StringLengthAsync(key);

                resultado.Add(new ClaveCache
                {
                    Clave = key.ToString(),
                    TtlSegundos = ttl is null ? -1 : (long)ttl.Value.TotalSeconds,
                    Longitud = longitud
                });
            }
        }

        return resultado
            .OrderBy(c => c.ClaveLogica, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// Añade un evento al registro compartido de accesos a la caché. El
    /// registro se conserva acotado para no crecer de forma indefinida.
    /// </summary>
    public async Task RegistrarEventoAsync(EventoCache evento, int maximoEventos = 100)
    {
        const string lista = "cache:eventos";

        string json = JsonSerializer.Serialize(evento);

        await _db.ListRightPushAsync(GetKey(lista), json);

        // Se conservan los maximoEventos más recientes (los últimos del rango).
        await _db.ListTrimAsync(GetKey(lista), -maximoEventos, -1);
    }

    /// <summary>
    /// Devuelve los eventos más recientes del registro compartido, del más
    /// nuevo al más antiguo.
    /// </summary>
    public async Task<List<EventoCache>> ObtenerEventosAsync(int cantidad = 50)
    {
        const string lista = "cache:eventos";

        // Rango negativo: se leen los últimos "cantidad" elementos, que son
        // los más recientes porque los eventos se agregan por la derecha.
        RedisValue[] valores = await _db.ListRangeAsync(
            GetKey(lista), -cantidad, -1);

        List<EventoCache> eventos = new();

        foreach (RedisValue valor in valores)
        {
            try
            {
                EventoCache? evento =
                    JsonSerializer.Deserialize<EventoCache>(valor.ToString());

                if (evento is not null)
                {
                    eventos.Add(evento);
                }
            }
            catch
            {
                // Un evento corrupto no debe impedir mostrar el resto.
            }
        }

        return eventos;
    }
}
