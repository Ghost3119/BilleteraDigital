namespace BilleteraDigital.Application.UseCases.Transferencia;

// ── Requests ──────────────────────────────────────────────────────────────────

/// <summary>
/// Body enviado por el cliente al realizar una transferencia.
/// El cliente especifica explícitamente la cuenta origen (multi-cuenta).
/// La validación de titularidad se realiza en el Use Case comparando
/// CuentaOrigen.UsuarioId contra el UsuarioId extraído del JWT.
/// </summary>
public record RealizarTransferenciaRequest(
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Monto,
    string Descripcion
);

// ── Commands ──────────────────────────────────────────────────────────────────

/// <summary>
/// Comando interno que viaja del Controller al Use Case.
/// Combina el UsuarioId del JWT (para la validación de titularidad)
/// con los datos del request HTTP. No es parte del contrato HTTP público.
/// </summary>
public record TransferenciaCommand(
    Guid UsuarioId,
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Monto,
    string Descripcion
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record TransferenciaResponse(
    Guid TransaccionId,
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Monto,
    decimal SaldoOrigenResultante,
    DateTime FechaHora,
    string Descripcion
);
