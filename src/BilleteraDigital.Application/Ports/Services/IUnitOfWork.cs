namespace BilleteraDigital.Application.Ports.Services;

/// <summary>
/// Puerto de salida para la unidad de trabajo (Unit of Work).
/// Garantiza atomicidad entre múltiples operaciones de repositorio.
/// </summary>
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ejecuta <paramref name="operacion"/> dentro de una transacción de base de datos,
    /// envuelta en la estrategia de reintentos del proveedor (p.ej. SqlServerRetryingExecutionStrategy).
    /// Toda la lógica transaccional debe ir dentro del delegate; el SaveChanges se invoca
    /// automáticamente antes del Commit.
    /// </summary>
    Task EjecutarEnTransaccionAsync(Func<Task> operacion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inicia una transacción de base de datos explícita.
    /// <para>
    /// ADVERTENCIA: no compatible con <c>EnableRetryOnFailure</c>. Usar
    /// <see cref="EjecutarEnTransaccionAsync"/> en entornos de producción con SQL Server.
    /// </para>
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>Confirma la transacción activa y libera el recurso.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Revierte la transacción activa y libera el recurso.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
