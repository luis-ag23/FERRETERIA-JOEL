using FERRETERIA__Joel.Infraestructura.Servicios;

namespace FERRETERIA__Joel.Servicios;

public class CacheSelectRedisServicio
{
    private readonly RedisCacheHelper _cache;

    public CacheSelectRedisServicio(RedisCacheHelper cache)
    {
        _cache = cache;
    }

    public async Task<T?> ObtenerAsync<T>(string clave)
    {
        return await _cache.GetAsync<T>(clave);
    }

    public async Task GuardarAsync<T>(string clave, T valor, TimeSpan ttl)
    {
        await _cache.SetAsync(clave, valor, ttl);
    }

    public async Task InvalidarPorTablaAsync(string tabla)
    {
        // Invalidar todos los selects cacheados para esa tabla
        await _cache.RemoveByPatternAsync($"cache:select:{tabla.ToLower()}:*");
    }

    public async Task InvalidarClaveAsync(string clave)
    {
        await _cache.RemoveAsync(clave);
    }
}