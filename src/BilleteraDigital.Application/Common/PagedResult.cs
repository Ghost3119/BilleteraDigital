namespace BilleteraDigital.Application.Common;

/// <summary>
/// Envuelve una página de resultados con metadatos de paginación.
/// Es el tipo de retorno estándar para cualquier endpoint paginado.
/// </summary>
/// <typeparam name="T">Tipo de los ítems de la página actual.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Ítems de la página actual.</summary>
    public IEnumerable<T> Items { get; }

    /// <summary>Total de registros en la fuente de datos (sin paginar).</summary>
    public int TotalCount { get; }

    /// <summary>Número de página actual.</summary>
    public int PageNumber { get; }

    /// <summary>Tamaño de página solicitado.</summary>
    public int PageSize { get; }

    /// <summary>Total de páginas disponibles.</summary>
    public int TotalPages { get; }

    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items      = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize   = pageSize;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }
}
