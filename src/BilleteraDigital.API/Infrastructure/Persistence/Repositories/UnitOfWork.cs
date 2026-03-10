using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.API.Infrastructure.Persistence;

namespace BilleteraDigital.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del patrón Unit of Work usando el DbContext de EF Core.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly BilleteraDbContext _context;

    public UnitOfWork(BilleteraDbContext context)
    {
        _context = context;
    }

    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
