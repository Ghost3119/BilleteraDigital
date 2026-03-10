using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Domain.Entities;
using BilleteraDigital.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BilleteraDigital.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Adaptador de salida: implementación EF Core de ITransaccionRepository.
/// </summary>
internal sealed class TransaccionRepository : ITransaccionRepository
{
    private readonly BilleteraDbContext _context;

    public TransaccionRepository(BilleteraDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public Task<PagedResult<Transaccion>> ObtenerPorCuentaFiltradoAsync(
        Guid cuentaId,
        GenericQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        // Base: todas las transacciones de la cuenta, sin materializar aún.
        IQueryable<Transaccion> query = _context.Transacciones
            .AsNoTracking()
            .Where(t => t.CuentaOrigenId == cuentaId);

        // ── Filtros dinámicos ────────────────────────────────────────────────
        // GetDecodedFilters() ya normaliza claves a camelCase (PropertyNameCaseInsensitive).
        // Claves no reconocidas se ignoran; un payload malformado devuelve {} y
        // se sirve la consulta sin filtros adicionales.
        var filters = queryParams.GetDecodedFilters();

        if (filters.TryGetValue("fechaInicio", out var fechaInicioStr)
            && DateTime.TryParse(fechaInicioStr, out var fechaInicio))
        {
            query = query.Where(t => t.FechaHora >= fechaInicio.Date);
        }

        if (filters.TryGetValue("fechaFin", out var fechaFinStr)
            && DateTime.TryParse(fechaFinStr, out var fechaFin))
        {
            // Incluir todo el día final (hasta las 23:59:59.999).
            query = query.Where(t => t.FechaHora < fechaFin.Date.AddDays(1));
        }

        if (filters.TryGetValue("tipo", out var tipoStr)
            && Enum.TryParse<TipoTransaccion>(tipoStr, ignoreCase: true, out var tipo))
        {
            query = query.Where(t => t.Tipo == tipo);
        }

        if (filters.TryGetValue("montoMinimo", out var montoMinimoStr)
            && decimal.TryParse(montoMinimoStr, out var montoMinimo))
        {
            query = query.Where(t => t.Monto >= montoMinimo);
        }

        if (filters.TryGetValue("montoMaximo", out var montoMaximoStr)
            && decimal.TryParse(montoMaximoStr, out var montoMaximo))
        {
            query = query.Where(t => t.Monto <= montoMaximo);
        }

        // ORDER BY se aplica después de los filtros para que EF genere
        // un plan óptimo (filtro + orden + OFFSET/FETCH en una sola query).
        query = query.OrderByDescending(t => t.FechaHora);

        return query.ToPagedResultAsync(queryParams, cancellationToken);
    }

    public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaAsync(
        Guid cuentaId,
        CancellationToken cancellationToken = default)
        => await _context.Transacciones
                         .AsNoTracking()
                         .Where(t => t.CuentaOrigenId == cuentaId)
                         .OrderByDescending(t => t.FechaHora)
                         .ToListAsync(cancellationToken);

    public async Task<Transaccion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Transacciones
                         .AsNoTracking()
                         .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}
