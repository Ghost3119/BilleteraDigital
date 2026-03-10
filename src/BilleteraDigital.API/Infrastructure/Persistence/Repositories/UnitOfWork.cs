using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.API.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace BilleteraDigital.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del patrón Unit of Work usando el DbContext de EF Core.
///
/// NOTA sobre SqlServerRetryingExecutionStrategy:
/// EF Core prohíbe abrir una transacción de usuario directamente cuando está activa
/// una estrategia de reintentos (EnableRetryOnFailure), porque el reintento podría
/// re-ejecutar operaciones ya confirmadas parcialmente.
/// La solución oficial es envolver toda la unidad de trabajo en
/// <c>CreateExecutionStrategy().ExecuteAsync</c>, que convierte el bloque completo
/// (BeginTransaction → operaciones de dominio → Commit) en la unidad retriable.
/// Por eso <see cref="BeginTransactionAsync"/> no abre la transacción inmediatamente;
/// la apertura real ocurre dentro del delegate pasado a
/// <see cref="EjecutarEnTransaccionAsync"/>.
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

    /// <summary>
    /// Ejecuta <paramref name="operacion"/> dentro de una transacción de base de datos,
    /// envuelta en la estrategia de reintentos de EF Core.
    /// Usar este método en lugar de BeginTransactionAsync / CommitAsync / RollbackAsync
    /// cuando la aplicación tiene EnableRetryOnFailure configurado.
    /// </summary>
    public async Task EjecutarEnTransaccionAsync(
        Func<Task> operacion,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(
            state: (operacion, cancellationToken),
            operation: async (ctx, state, ct) =>
            {
                await using var transaction = await ctx.Database.BeginTransactionAsync(ct);
                try
                {
                    await state.operacion();
                    await ctx.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
                return 0;
            },
            verifySucceeded: null,
            cancellationToken: cancellationToken);
    }

    // Los tres métodos siguientes se mantienen para compatibilidad con entornos
    // sin estrategia de reintentos (tests, SQLite in-memory, etc.).
    // En producción con SQL Server + EnableRetryOnFailure usar EjecutarEnTransaccionAsync.

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
