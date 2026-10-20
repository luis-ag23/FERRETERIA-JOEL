namespace FERRETERIA__Joel.Servicios;

public class InfoContenedor
{
    public string Id => Environment.GetEnvironmentVariable("HOSTNAME")
                        ?? Environment.MachineName;
}