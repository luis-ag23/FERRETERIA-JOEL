using FERRETERIA__Joel.Aplicacion.Servicios;
using FERRETERIA__Joel.Helpers;

namespace FERRETERIA__Joel.Servicios
{
    /// <summary>
    /// Construye el enlace absoluto de activación usando la URL base de la
    /// petición actual. El token viaja protegido con <see cref="UrlProtector"/>
    /// para no exponerlo en claro en la barra de direcciones ni en los logs del
    /// servidor web.
    ///
    /// En desarrollo (o cuando no hay SMTP configurado) el enlace se devuelve
    /// para mostrarlo en pantalla y poder probar el flujo sin servidor de
    /// correo.
    /// </summary>
    public class ConstructorEnlaceActivacion : IConstructorEnlaceActivacion
    {
        private readonly IHttpContextAccessor _accesor;
        private readonly IConfiguration _configuration;

        public ConstructorEnlaceActivacion(
            IHttpContextAccessor accesor,
            IConfiguration configuration)
        {
            _accesor = accesor;
            _configuration = configuration;
        }

        public bool EsEnlaceDeDesarrollo =>
            string.IsNullOrWhiteSpace(_configuration["Smtp:Host"]);

        public string Construir(string token)
        {
            HttpRequest? peticion = _accesor.HttpContext?.Request;

            string baseUrl = peticion is not null
                ? $"{peticion.Scheme}://{peticion.Host}"
                : (_configuration["App:UrlBase"]?.TrimEnd('/')
                    ?? string.Empty);

            string tokenProtegido = UrlProtector.Cifrar(token);

            return $"{baseUrl}/ActivarCuenta?token={Uri.EscapeDataString(tokenProtegido)}";
        }
    }
}
