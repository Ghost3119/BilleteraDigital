namespace BilleteraDigital.Application.UseCases.Transferencia;

// ── Requests ──────────────────────────────────────────────────────────────────

/// <summary>
/// Body enviado por el cliente al realizar una transferencia.
/// El cliente especifica la cuenta origen por su Id (extraído de su sesión activa)
/// y el destinatario mediante un identificador amigable: correo electrónico O número de cuenta.
/// La resolución del destinatario al CuentaId concreto ocurre en el Use Case.
/// </summary>
public record RealizarTransferenciaRequest(
    Guid CuentaOrigenId,
    /// <summary>
    /// Correo electrónico del destinatario (p. ej. "maria@ejemplo.com")
    /// o número de cuenta de 16 dígitos (p. ej. "1234567890123456").
    /// </summary>
    string Destinatario,
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
    string Destinatario,
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
