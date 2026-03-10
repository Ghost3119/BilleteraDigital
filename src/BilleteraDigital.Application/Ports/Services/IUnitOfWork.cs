namespace BilleteraDigital.Application.Ports.Services;

/// <summary>
/// Puerto de salida para la unidad de trabajo (Unit of Work).
/// Garantiza atomicidad entre múltiples operaciones de repositorio.
/// </summary>
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
