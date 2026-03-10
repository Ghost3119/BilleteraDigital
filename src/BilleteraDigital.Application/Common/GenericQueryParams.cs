using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace BilleteraDigital.Application.Common;

/// <summary>
/// Parámetros de consulta genéricos: paginación + filtros dinámicos en Base64.
///
/// El frontend envía los filtros como un objeto JSON serializado en Base64:
///   ?filtersBase64=eyJmZWNoYUluaWNpbyI6IjIwMjUtMDEtMDEiLCJmZWNoYUZpbiI6IjIwMjUtMTItMzEifQ==
///
/// El objeto JSON decodificado es un diccionario plano clave-valor:
///   { "fechaInicio": "2025-01-01", "fechaFin": "2025-12-31" }
///
/// Las claves permitidas las conoce cada repositorio; claves desconocidas se ignoran.
/// </summary>
public sealed class GenericQueryParams
{
    private const int MaxPageSize = 50;

    private int _pageNumber = 1;
    private int _pageSize   = 10;

    // ── Paginación ────────────────────────────────────────────────────────────

    /// <summary>Número de página (mínimo 1, por defecto 1).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "PageNumber debe ser mayor o igual a 1.")]
    [DefaultValue(1)]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>Registros por página (entre 1 y 50, por defecto 10).</summary>
    [Range(1, MaxPageSize, ErrorMessage = "PageSize debe estar entre 1 y 50.")]
    [DefaultValue(10)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : value > MaxPageSize ? MaxPageSize : value;
    }

    // ── Filtros ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Objeto JSON de filtros serializado en Base64 URL-safe.
    /// Ejemplo del objeto antes de codificar:
    /// <code>{ "fechaInicio": "2025-01-01", "fechaFin": "2025-12-31", "tipo": "Transferencia" }</code>
    /// </summary>
    public string? FiltersBase64 { get; set; }

    /// <summary>
    /// Decodifica <see cref="FiltersBase64"/> y devuelve los filtros como un diccionario
    /// clave-valor de strings. Devuelve un diccionario vacío si el parámetro está ausente
    /// o si el payload es inválido, sin lanzar excepciones al llamador.
    /// </summary>
    public Dictionary<string, string> GetDecodedFilters()
    {
        if (string.IsNullOrWhiteSpace(FiltersBase64))
            return [];

        try
        {
            // Base64 estándar usa '+' y '/'; la URL los reemplaza por '-' y '_'.
            // Normalizamos antes de decodificar para soportar ambos formatos.
            var normalized = FiltersBase64
                .Replace('-', '+')
                .Replace('_', '/');

            // Añadir padding '=' si falta (los clientes suelen omitirlo).
            var padded = normalized.PadRight(
                normalized.Length + (4 - normalized.Length % 4) % 4, '=');

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? [];
        }
        catch
        {
            // Payload malformado: se ignoran los filtros en lugar de romper la petición.
            return [];
        }
    }
}
