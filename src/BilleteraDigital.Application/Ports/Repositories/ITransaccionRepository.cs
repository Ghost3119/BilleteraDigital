using BilleteraDigital.Application.Common;
using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Ports.Repositories;

/// <summary>
/// Puerto de salida para el repositorio de transacciones.
/// </summary>
public interface ITransaccionRepository
{
    /// <summary>
    /// Devuelve el historial paginado de transacciones de una cuenta,
    /// ordenado de más reciente a más antiguo.
    /// </summary>
    Task<PagedResult<Transaccion>> ObtenerPorCuentaPaginadoAsync(
        Guid cuentaId,
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaccion>> ObtenerPorCuentaAsync(Guid cuentaId, CancellationToken cancellationToken = default);
    Task<Transaccion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
