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
///
/// La cuenta origen se identifica a través del UsuarioId extraído del JWT,
/// nunca a partir de un dato proporcionado por el cliente.
/// La operación completa se envuelve en una transacción de base de datos explícita.
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
        TransferenciaCommand command,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Resolver la cuenta origen a partir del UsuarioId del JWT ────────
        var cuentaOrigen = await _cuentaRepository.ObtenerPorUsuarioIdAsync(command.UsuarioId, cancellationToken);
        if (cuentaOrigen is null)
            return Result<TransferenciaResponse>.Fallido("No se encontró una cuenta asociada al usuario autenticado.");

        // ── 2. Validar que origen y destino sean distintos ────────────────────
        if (cuentaOrigen.Id == command.CuentaDestinoId)
            return Result<TransferenciaResponse>.Fallido("La cuenta origen y destino no pueden ser la misma.");

        // ── 3. Cargar cuenta destino ──────────────────────────────────────────
        var cuentaDestino = await _cuentaRepository.ObtenerPorIdAsync(command.CuentaDestinoId, cancellationToken);
        if (cuentaDestino is null)
            return Result<TransferenciaResponse>.Fallido($"Cuenta destino '{command.CuentaDestinoId}' no encontrada.");

        // ── 4. Ejecutar lógica de dominio dentro de una transacción ACID ──────
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            cuentaOrigen.Retirar(command.Monto, cuentaDestino.Id, command.Descripcion);
            cuentaDestino.Depositar(command.Monto, cuentaOrigen.Id, command.Descripcion);

            // CommitAsync llama a SaveChangesAsync internamente antes de confirmar.
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (DomainException ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return Result<TransferenciaResponse>.Fallido(ex.Message);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }

        // ── 5. Obtener la transacción recién registrada en la cuenta origen ───
        var transaccion = cuentaOrigen.Transacciones.Last();

        return Result<TransferenciaResponse>.Exitoso(new TransferenciaResponse(
            transaccion.Id,
            cuentaOrigen.Id,
            cuentaDestino.Id,
            command.Monto,
            cuentaOrigen.Saldo,
            transaccion.FechaHora,
            command.Descripcion
        ));
    }
}
