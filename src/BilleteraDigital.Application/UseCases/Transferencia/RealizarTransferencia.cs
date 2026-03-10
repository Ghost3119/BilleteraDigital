using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.Domain.Exceptions;

namespace BilleteraDigital.Application.UseCases.Transferencia;

/// <summary>
/// Caso de uso: Realizar una transferencia de fondos entre dos cuentas.
///
/// Reglas de negocio aplicadas:
///   - El monto debe ser mayor a cero.
///   - La cuenta origen debe tener saldo suficiente.
///   - Ambas cuentas deben estar activas.
///   - El usuario autenticado debe ser el titular de la cuenta origen.
///     (si no lo es se lanza <see cref="AccesoDenegadoException"/> → HTTP 403)
///
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
        // ── 1. Cargar cuenta origen por su Id (proporcionado por el cliente) ──
        var cuentaOrigen = await _cuentaRepository.ObtenerPorIdAsync(command.CuentaOrigenId, cancellationToken);
        if (cuentaOrigen is null)
            return Result<TransferenciaResponse>.Fallido($"Cuenta origen '{command.CuentaOrigenId}' no encontrada.");

        // ── 2. VERIFICACIÓN DE TITULARIDAD ────────────────────────────────────
        // Comparar el propietario de la cuenta contra el UsuarioId del JWT.
        // Se lanza AccesoDenegadoException (→ HTTP 403) para no revelar
        // si la cuenta existe cuando el usuario no es el titular.
        if (cuentaOrigen.UsuarioId != command.UsuarioId)
            throw new AccesoDenegadoException(command.CuentaOrigenId);

        // ── 3. Validar que origen y destino sean distintos ────────────────────
        if (command.CuentaOrigenId == command.CuentaDestinoId)
            return Result<TransferenciaResponse>.Fallido("La cuenta origen y destino no pueden ser la misma.");

        // ── 4. Cargar cuenta destino ──────────────────────────────────────────
        var cuentaDestino = await _cuentaRepository.ObtenerPorIdAsync(command.CuentaDestinoId, cancellationToken);
        if (cuentaDestino is null)
            return Result<TransferenciaResponse>.Fallido($"Cuenta destino '{command.CuentaDestinoId}' no encontrada.");

        // ── 5. Ejecutar lógica de dominio dentro de una transacción ACID ──────
        // EjecutarEnTransaccionAsync envuelve el delegate en CreateExecutionStrategy(),
        // lo que es compatible con SqlServerRetryingExecutionStrategy (EnableRetryOnFailure).
        // SaveChangesAsync y Commit se invocan automáticamente dentro del delegate.
        // Las DomainException propagadas desde el delegate cancelan la transacción
        // y son re-lanzadas; las capturamos aquí para convertirlas en Result.Fallido.
        try
        {
            await _unitOfWork.EjecutarEnTransaccionAsync(() =>
            {
                cuentaOrigen.Retirar(command.Monto, cuentaDestino.Id, command.Descripcion);
                cuentaDestino.Depositar(command.Monto, cuentaOrigen.Id, command.Descripcion);
                return Task.CompletedTask;
            }, cancellationToken);
        }
        catch (DomainException ex) when (ex is not AccesoDenegadoException)
        {
            return Result<TransferenciaResponse>.Fallido(ex.Message);
        }

        // ── 6. Obtener la transacción recién registrada en la cuenta origen ───
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
