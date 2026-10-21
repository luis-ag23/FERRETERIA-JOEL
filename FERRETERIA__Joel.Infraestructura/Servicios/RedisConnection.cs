using System.Text.Json;
using StackExchange.Redis;

namespace FERRETERIA__Joel.Infraestructura.Servicios;

public class RedisConnection
{
    private readonly Lazy<ConnectionMultiplexer> _lazy;

    public RedisConnection(string configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration))
            throw new ArgumentException("Redis connection configuration is required", nameof(configuration));

        _lazy = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configuration));
    }

    public IDatabase Db => _lazy.Value.GetDatabase();

    public IServer GetServer(string endpoint)
    {
        var endpoints = _lazy.Value.GetEndPoints();
        return _lazy.Value.GetServer(endpoint);
    }

    public IEnumerable<IServer> GetServers()
    {
        var endpoints = _lazy.Value.GetEndPoints();
        return endpoints.Select(ep => _lazy.Value.GetServer(ep));
    }
}