namespace FERRETERIA__Joel.Dominio.Utilidades
{
    /// <summary>
    /// Reduce un nombre completo a sus siglas para mostrarlo en documentos
    /// donde no debe aparecer el nombre completo. Ejemplo: "Juan Pérez Vargas"
    /// produce "JPV".
    /// </summary>
    public static class Siglas
    {
        public static string De(string? nombreCompleto)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return string.Empty;
            }

            var iniciales = nombreCompleto
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(PrimeraLetraODigito)
                .Where(c => c is not null)
                .Select(c => char.ToUpperInvariant(c!.Value));

            return string.Concat(iniciales);
        }

        private static char? PrimeraLetraODigito(string palabra)
        {
            foreach (char c in palabra)
            {
                if (char.IsLetterOrDigit(c))
                {
                    return c;
                }
            }

            return null;
        }
    }
}