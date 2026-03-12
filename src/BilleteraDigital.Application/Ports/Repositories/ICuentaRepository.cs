using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Ports.Repositories;

/// <summary>
/// Puerto de salida (driven port) para el repositorio de cuentas.
/// La capa de dominio define el contrato; la infraestructura lo implementa.
/// </summary>
public interface ICuentaRepository
{
    Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cuenta?> ObtenerPorNumeroAsync(long numeroCuenta, CancellationToken cancellationToken = default);
    Task<Cuenta?> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cuenta>> ObtenerListaPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cuenta>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<Cuenta> CrearAsync(Cuenta cuenta, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Cuenta cuenta, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}
