using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.Domain.Exceptions;

namespace BilleteraDigital.Application.UseCases.Transferencia;

/// <summary>
/// Caso de uso: Realizar una transferencia de fondos entre dos cuentas.
///
/// Reglas de negocio aplicadas (en la capa de dominio):
///   - El monto debe ser mayor a cero.
///   - La cuenta origen debe tener saldo suficiente.
///   - Ambas cuentas deben estar activas.
/// </summary>
public sealed class RealizarTransferencia
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RealizarTransferencia(ICuentaRepository cuentaRepository, IUnitOfWork unitOfWork)
    {
        _cuentaRepository = cuentaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TransferenciaResponse>> EjecutarAsync(
        RealizarTransferenciaRequest request,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Validar que origen y destino sean distintos ────────────────────
        if (request.CuentaOrigenId == request.CuentaDestinoId)
            return Result<TransferenciaResponse>.Fallido("La cuenta origen y destino no pueden ser la misma.");

        // ── 2. Cargar entidades ───────────────────────────────────────────────
        var cuentaOrigen = await _cuentaRepository.ObtenerPorIdAsync(request.CuentaOrigenId, cancellationToken);
        if (cuentaOrigen is null)
            return Result<TransferenciaResponse>.Fallido($"Cuenta origen '{request.CuentaOrigenId}' no encontrada.");

        var cuentaDestino = await _cuentaRepository.ObtenerPorIdAsync(request.CuentaDestinoId, cancellationToken);
        if (cuentaDestino is null)
            return Result<TransferenciaResponse>.Fallido($"Cuenta destino '{request.CuentaDestinoId}' no encontrada.");

        // ── 3. Ejecutar lógica de dominio (puede lanzar DomainException) ─────
        try
        {
            cuentaOrigen.Debitar(request.Monto, request.Descripcion);
            cuentaDestino.Acreditar(request.Monto, request.Descripcion);
        }
        catch (DomainException ex)
        {
            return Result<TransferenciaResponse>.Fallido(ex.Message);
        }

        // ── 4. Persistir cambios ──────────────────────────────────────────────
        await _cuentaRepository.ActualizarAsync(cuentaOrigen, cancellationToken);
        await _cuentaRepository.ActualizarAsync(cuentaDestino, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        // ── 5. Obtener la transacción recién registrada en la cuenta origen ───
        var transaccion = cuentaOrigen.Transacciones.Last();

        return Result<TransferenciaResponse>.Exitoso(new TransferenciaResponse(
            transaccion.Id,
            cuentaOrigen.Id,
            cuentaDestino.Id,
            request.Monto,
            cuentaOrigen.Saldo,
            transaccion.FechaHora,
            request.Descripcion
        ));
    }
}
