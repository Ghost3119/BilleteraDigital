using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Domain.Entities;
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

    public Task<PagedResult<Transaccion>> ObtenerPorCuentaPaginadoAsync(
        Guid cuentaId,
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default)
        => _context.Transacciones
                   .AsNoTracking()
                   .Where(t => t.CuentaOrigenId == cuentaId)
                   .OrderByDescending(t => t.FechaHora)
                   .ToPagedResultAsync(paginationParams, cancellationToken);

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
