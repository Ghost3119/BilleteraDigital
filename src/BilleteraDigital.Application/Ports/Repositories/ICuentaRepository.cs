using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Ports.Repositories;

/// <summary>
/// Puerto de salida (driven port) para el repositorio de cuentas.
/// La capa de dominio define el contrato; la infraestructura lo implementa.
/// </summary>
public interface ICuentaRepository
{
    Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cuenta?> ObtenerPorNumeroAsync(string numeroCuenta, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cuenta>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<Cuenta> CrearAsync(Cuenta cuenta, CancellationToken cancellationToken = default);
    Task ActualizarAsync(Cuenta cuenta, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
}
