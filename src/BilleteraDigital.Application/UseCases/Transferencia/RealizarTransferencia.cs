using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.Domain.Exceptions;
using CuentaEntity = BilleteraDigital.Domain.Entities.Cuenta;

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
///   - La cuenta destino se resuelve desde un identificador amigable:
///     * Si <c>Destinatario</c> contiene "@", se busca por correo del titular.
///     * Si no, se interpreta como número de cuenta (<c>long</c>) y se busca directamente.
///
/// La operación completa se envuelve en una transacción de base de datos explícita.
/// </summary>
public sealed class RealizarTransferencia
{
    private readonly ICuentaRepository  _cuentaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork        _unitOfWork;

    public RealizarTransferencia(
        ICuentaRepository  cuentaRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork        unitOfWork)
    {
        _cuentaRepository  = cuentaRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork        = unitOfWork;
    }

    public async Task<Result<TransferenciaResponse>> EjecutarAsync(
        TransferenciaCommand command,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Cargar cuenta origen ───────────────────────────────────────────
        var cuentaOrigen = await _cuentaRepository.ObtenerPorIdAsync(command.CuentaOrigenId, cancellationToken);
        if (cuentaOrigen is null)
            return Result<TransferenciaResponse>.Fallido($"Cuenta origen '{command.CuentaOrigenId}' no encontrada.");

        // ── 2. Verificación de titularidad ────────────────────────────────────
        // AccesoDenegadoException → HTTP 403. No revelar si la cuenta existe.
        if (cuentaOrigen.UsuarioId != command.UsuarioId)
            throw new AccesoDenegadoException(command.CuentaOrigenId);

        // ── 3. Resolver cuenta destino desde el identificador amigable ────────
        var cuentaDestino = await ResolverDestinatarioAsync(command.Destinatario, cancellationToken);
        if (cuentaDestino is null)
            return Result<TransferenciaResponse>.Fallido(
                "No se encontró ninguna cuenta asociada a ese correo o número de cuenta.");

        // ── 4. Validar que origen y destino sean distintos ────────────────────
        if (cuentaOrigen.Id == cuentaDestino.Id)
            return Result<TransferenciaResponse>.Fallido(
                "La cuenta origen y destino no pueden ser la misma.");

        // ── 5. Ejecutar lógica de dominio dentro de una transacción ACID ──────
        // EjecutarEnTransaccionAsync envuelve el delegate en CreateExecutionStrategy(),
        // compatible con SqlServerRetryingExecutionStrategy (EnableRetryOnFailure).
        // Las DomainException propagadas cancelan la transacción y se convierten en Result.Fallido.
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

        // ── 6. Construir respuesta con la transacción recién registrada ───────
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

    // ── Private: smart lookup ─────────────────────────────────────────────────

    /// <summary>
    /// Resuelve el <paramref name="destinatario"/> a una <see cref="CuentaEntity"/>.
    /// <list type="bullet">
    ///   <item>Si contiene "@": busca el usuario por email y luego su cuenta primaria.</item>
    ///   <item>Si no: intenta parsear como <c>long</c> y busca por NumeroCuenta.</item>
    /// </list>
    /// Devuelve <c>null</c> si no se encuentra ninguna coincidencia.
    /// </summary>
    private async Task<CuentaEntity?> ResolverDestinatarioAsync(
        string destinatario,
        CancellationToken cancellationToken)
    {
        var trimmed = destinatario.Trim();

        if (trimmed.Contains('@'))
        {
            // ── Flujo por email ───────────────────────────────────────────────
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(
                trimmed.ToLowerInvariant(), cancellationToken);

            if (usuario is null)
                return null;

            return await _cuentaRepository.ObtenerPorUsuarioIdAsync(usuario.Id, cancellationToken);
        }
        else
        {
            // ── Flujo por número de cuenta ────────────────────────────────────
            if (!long.TryParse(trimmed, out var numeroCuenta))
                return null;

            return await _cuentaRepository.ObtenerPorNumeroAsync(numeroCuenta, cancellationToken);
        }
    }
}

