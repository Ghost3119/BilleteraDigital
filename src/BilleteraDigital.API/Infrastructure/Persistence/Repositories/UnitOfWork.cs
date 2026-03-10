using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace BilleteraDigital.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del patrón Unit of Work usando el DbContext de EF Core.
/// </summary>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly BilleteraDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(BilleteraDbContext context)
    {
        _context = context;
    }

    public async Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            throw new InvalidOperationException("Ya existe una transacción activa.");

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            throw new InvalidOperationException("No hay ninguna transacción activa para confirmar.");

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }
}
