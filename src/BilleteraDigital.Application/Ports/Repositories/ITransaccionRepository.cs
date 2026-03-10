using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Ports.Repositories;

/// <summary>
/// Puerto de salida para el repositorio de transacciones.
/// </summary>
public interface ITransaccionRepository
{
    Task<IEnumerable<Transaccion>> ObtenerPorCuentaAsync(Guid cuentaId, CancellationToken cancellationToken = default);
    Task<Transaccion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
