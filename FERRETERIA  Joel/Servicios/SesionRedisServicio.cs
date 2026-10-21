using System.Text.Json;
using FERRETERIA__Joel.Infraestructura.Servicios;
using StackExchange.Redis;

namespace FERRETERIA__Joel.Servicios;

public class SesionUsuarioRedis
{
    public int IdEmpleado { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaLogin { get; set; } = DateTime.UtcNow;
}

public class SesionRedisServicio
{
    private readonly IDatabase _db;
    private readonly string _prefix = "ferreteria:sesion:";

    public SesionRedisServicio(RedisConnection connection)
    {
        _db = connection.Db;
    }

    public async Task GuardarSesionAsync(string claveSesion, SesionUsuarioRedis sesion, TimeSpan ttl)
    {
        var key = _prefix + claveSesion;
        var json = JsonSerializer.Serialize(sesion);
        await _db.StringSetAsync(key, json, ttl);
    }

    public async Task<SesionUsuarioRedis?> ObtenerSesionAsync(string claveSesion)
    {
        var key = _prefix + claveSesion;
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue) return null;
        try
        {
            return JsonSerializer.Deserialize<SesionUsuarioRedis>(value.ToString());
        }
        catch
        {
            return null;
        }
    }

    public async Task EliminarSesionAsync(string claveSesion)
    {
        var key = _prefix + claveSesion;
        await _db.KeyDeleteAsync(key);
    }
}