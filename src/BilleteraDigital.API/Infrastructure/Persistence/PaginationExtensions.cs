using BilleteraDigital.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BilleteraDigital.API.Infrastructure.Persistence;

/// <summary>
/// Extensiones de paginación para <see cref="IQueryable{T}"/>.
/// Viven en Infrastructure porque dependen de EF Core (CountAsync / ToListAsync).
/// El Application layer sólo conoce los tipos del contrato: <see cref="PaginationParams"/>,
/// <see cref="GenericQueryParams"/> y <see cref="PagedResult{T}"/>.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Sobrecarga para <see cref="PaginationParams"/>: cuenta registros y aplica Skip/Take.
    /// </summary>
    public static Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default)
        => source.ToPagedResultAsync(
               paginationParams.PageNumber,
               paginationParams.PageSize,
               cancellationToken);

    /// <summary>
    /// Sobrecarga para <see cref="GenericQueryParams"/>: cuenta registros y aplica Skip/Take.
    /// </summary>
    public static Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        GenericQueryParams queryParams,
        CancellationToken cancellationToken = default)
        => source.ToPagedResultAsync(
               queryParams.PageNumber,
               queryParams.PageSize,
               cancellationToken);

    /// <summary>
    /// Núcleo genérico: una sola implementación real; las sobrecargas de arriba delegan aquí.
    /// Emite exactamente dos queries SQL: COUNT(*) + SELECT con OFFSET/FETCH.
    /// </summary>
    /// <param name="source">IQueryable con filtros y ORDER BY ya aplicados.</param>
    /// <param name="pageNumber">Página solicitada (1-based).</param>
    /// <param name="pageSize">Registros por página.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
