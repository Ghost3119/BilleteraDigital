using BilleteraDigital.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace BilleteraDigital.API.Infrastructure.Persistence;

/// <summary>
/// Extensiones de paginación para <see cref="IQueryable{T}"/>.
/// Viven en Infrastructure porque dependen de EF Core (CountAsync / ToListAsync).
/// El Application layer sólo conoce los tipos del contrato: <see cref="PaginationParams"/>
/// y <see cref="PagedResult{T}"/>.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Cuenta el total de registros y aplica Skip/Take sobre la consulta ya ordenada,
    /// devolviendo un <see cref="PagedResult{T}"/> listo para serializar.
    /// </summary>
    /// <typeparam name="T">Tipo de entidad o proyección.</typeparam>
    /// <param name="source">Consulta IQueryable con filtros y ordenamiento ya aplicados.</param>
    /// <param name="paginationParams">Parámetros de paginación (PageNumber, PageSize).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
}
