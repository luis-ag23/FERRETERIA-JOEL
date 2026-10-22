using FERRETERIA__Joel.Dominio.Constantes;
using FERRETERIA__Joel.Dominio.Modelos;
using FERRETERIA__Joel.Dominio.Puertos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FERRETERIA__Joel.Pages
{
    /// <summary>
    /// Diagnóstico de la caché distribuida. Muestra las claves que hay ahora
    /// mismo en Redis con su tiempo de vida restante y el registro compartido
    /// de accesos (HIT/MISS) con la réplica que los produjo y el origen real
    /// del dato.
    ///
    /// <para>
    /// Es la evidencia en pantalla de que la caché es compartida entre las
    /// réplicas: un HIT registrado por <c>app1</c> puede ser el que se ve desde
    /// <c>app3</c>.
    /// </para>
    /// </summary>
    [Authorize(Roles = RolesSistema.Administrador)]
    public class DiagnosticoCacheModel : PageModel
    {
        /// <summary>Prefijo de las claves de consulta cacheadas.</summary>
        private const string PrefijoConsultas = "cache:select:";

        private readonly ICacheSeleccion _cache;
        private readonly IIdentificadorReplica _replica;
        private readonly ILogger<DiagnosticoCacheModel> _logger;

        public DiagnosticoCacheModel(
            ICacheSeleccion cache,
            IIdentificadorReplica replica,
            ILogger<DiagnosticoCacheModel> logger)
        {
            _cache = cache;
            _replica = replica;
            _logger = logger;
        }

        /// <summary>Réplica que está atender esta página.</summary>
        public string Replica => _replica.Id;

        /// <summary>Claves de caché con su TTL y su tamaño.</summary>
        public List<ClaveCache> Claves { get; set; } = new();

        /// <summary>Últimos accesos a la caché registrados por cualquier réplica.</summary>
        public List<EventoCache> Eventos { get; set; } = new();

        public List<string> Errores { get; set; } = new();

        public async Task OnGetAsync()
        {
            ViewData["ActiveMenu"] = "Registro";

            try
            {
                Claves = await _cache.ObtenerClavesAsync(PrefijoConsultas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar las claves de caché.");
                Errores.Add("No se pudieron listar las claves de caché.");
            }

            try
            {
                Eventos = await _cache.ObtenerEventosAsync(50);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al leer los eventos de caché.");
                Errores.Add("No se pudieron leer los eventos de caché.");
            }
        }
    }
}
