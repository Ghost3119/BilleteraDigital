using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.Ports.Repositories;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Consultar el saldo disponible de una cuenta por su ID.
/// </summary>
public sealed class ConsultarSaldo
{
    private readonly ICuentaRepository _cuentaRepository;

    public ConsultarSaldo(ICuentaRepository cuentaRepository)
    {
        _cuentaRepository = cuentaRepository;
    }

    public async Task<Result<SaldoResponse>> EjecutarAsync(
        Guid cuentaId,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _cuentaRepository.ObtenerPorIdAsync(cuentaId, cancellationToken);
        if (cuenta is null)
            return Result<SaldoResponse>.Fallido($"Cuenta '{cuentaId}' no encontrada.");

        return Result<SaldoResponse>.Exitoso(new SaldoResponse(
            cuenta.Id,
            cuenta.NumeroCuenta,
            cuenta.NombreTitular,
            cuenta.Saldo,
            DateTime.UtcNow
        ));
    }
}
