using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.Ports.Repositories;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Obtener el historial de transacciones de una cuenta.
/// </summary>
public sealed class ObtenerHistorialTransacciones
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly ITransaccionRepository _transaccionRepository;

    public ObtenerHistorialTransacciones(
        ICuentaRepository cuentaRepository,
        ITransaccionRepository transaccionRepository)
    {
        _cuentaRepository = cuentaRepository;
        _transaccionRepository = transaccionRepository;
    }

    public async Task<Result<IEnumerable<TransaccionResponse>>> EjecutarAsync(
        Guid cuentaId,
        CancellationToken cancellationToken = default)
    {
        var existe = await _cuentaRepository.ExisteAsync(cuentaId, cancellationToken);
        if (!existe)
            return Result<IEnumerable<TransaccionResponse>>.Fallido($"Cuenta '{cuentaId}' no encontrada.");

        var transacciones = await _transaccionRepository.ObtenerPorCuentaAsync(cuentaId, cancellationToken);

        var respuesta = transacciones.Select(t => new TransaccionResponse(
            t.Id,
            t.Tipo,
            t.Monto,
            t.SaldoResultante,
            t.Descripcion,
            t.FechaHora
        ));

        return Result<IEnumerable<TransaccionResponse>>.Exitoso(respuesta);
    }
}
