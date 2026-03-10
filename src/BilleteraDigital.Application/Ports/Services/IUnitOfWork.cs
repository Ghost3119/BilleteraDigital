namespace BilleteraDigital.Application.Ports.Services;

/// <summary>
/// Puerto de salida para la unidad de trabajo (Unit of Work).
/// Garantiza atomicidad entre múltiples operaciones de repositorio.
/// </summary>
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción de base de datos explícita.
    /// El llamador es responsable de hacer <c>await CommitAsync</c> en caso de éxito
    /// o <c>await RollbackAsync</c> en caso de error.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Confirma la transacción activa y libera el recurso.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Revierte la transacción activa y libera el recurso.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
