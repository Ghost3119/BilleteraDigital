using BilleteraDigital.Domain.Enums;

namespace BilleteraDigital.Application.DTOs;

// ── Requests ─────────────────────────────────────────────────────────────────

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

/// <summary>
/// Body enviado por el cliente al crear una cuenta.
/// Está intencionalmente vacío: el número de cuenta lo genera la base de datos
/// (secuencia SQL Server), el nombre del titular viene del JWT y el saldo
/// inicial es siempre cero por política de negocio.
/// </summary>
public record CrearCuentaRequest();

/// <summary>
/// Comando interno que viaja del Controller al Use Case.
/// Lleva únicamente el UsuarioId extraído del JWT; no acepta ningún dato del cliente.
/// No es parte del contrato HTTP público.
/// </summary>
public record CrearCuentaCommand(Guid UsuarioId);

// ── Responses ────────────────────────────────────────────────────────────────

public record CuentaResponse(
    Guid Id,
    long NumeroCuenta,
    string NombreTitular,
    decimal Saldo,
    EstadoCuenta Estado,
    DateTime FechaCreacion,
    DateTime? FechaUltimaOperacion
);

public record TransferenciaResponse(
    Guid TransaccionId,
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Monto,
    decimal SaldoOrigenResultante,
    DateTime FechaHora,
    string Descripcion
);

public record SaldoResponse(
    Guid CuentaId,
    long NumeroCuenta,
    string NombreTitular,
    decimal Saldo,
    DateTime ConsultadoEn
);

public record TransaccionResponse(
    Guid Id,
    TipoTransaccion Tipo,
    decimal Monto,
    decimal SaldoResultante,
    string Descripcion,
    DateTime FechaHora
);

// ── Usuario ───────────────────────────────────────────────────────────────────

public record RegistrarUsuarioRequest(
    string Nombre,
    string Email,
    string Password
);

public record LoginConEmailRequest(
    string Email,
    string Password
);

/// <summary>Respuesta unificada para Login y Refresh; incluye ambos tokens.</summary>
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string ExpiraEn,
    string Tipo
);

/// <summary>Body del endpoint POST /api/v1/Auth/refresh.</summary>
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);

public record UsuarioRegistradoResponse(
    Guid Id,
    string Nombre,
    string Email,
    DateTime FechaRegistro
);
