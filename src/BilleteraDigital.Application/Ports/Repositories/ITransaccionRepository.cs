using BilleteraDigital.Application.Common;
using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Ports.Repositories;

/// <summary>
/// Puerto de salida para el repositorio de transacciones.
/// </summary>
public interface ITransaccionRepository
{
    /// <summary>
    /// Devuelve el historial filtrado y paginado de transacciones de una cuenta,
    /// ordenado de más reciente a más antiguo.
    /// Los filtros dinámicos se transportan dentro de <paramref name="queryParams"/>
    /// como Base64; la decodificación y aplicación de cláusulas WHERE las realiza
    /// la implementación concreta en Infrastructure.
    /// </summary>
    Task<PagedResult<Transaccion>> ObtenerPorCuentaFiltradoAsync(
        Guid cuentaId,
        GenericQueryParams queryParams,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Transaccion>> ObtenerPorCuentaAsync(Guid cuentaId, CancellationToken cancellationToken = default);
    Task<Transaccion?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
}
