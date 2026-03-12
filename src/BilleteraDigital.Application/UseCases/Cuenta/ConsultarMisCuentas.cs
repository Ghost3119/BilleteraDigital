using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Devuelve todas las cuentas que pertenecen al usuario autenticado.
/// </summary>
public sealed class ConsultarMisCuentas
{
    private readonly ICuentaRepository _cuentaRepository;

    public ConsultarMisCuentas(ICuentaRepository cuentaRepository)
    {
        _cuentaRepository = cuentaRepository;
    }

    public async Task<Result<IReadOnlyList<CuentaResponse>>> EjecutarAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var cuentas = await _cuentaRepository.ObtenerListaPorUsuarioIdAsync(usuarioId, cancellationToken);

        var respuestas = cuentas.Select(c => new CuentaResponse(
            c.Id,
            c.NumeroCuenta,
            c.NombreTitular,
            c.Saldo,
            c.Estado,
            c.FechaCreacion,
            c.FechaUltimaOperacion)).ToList();

        return Result<IReadOnlyList<CuentaResponse>>.Exitoso(respuestas);
    }
}
