using BilleteraDigital.Domain.Enums;

namespace BilleteraDigital.Application.DTOs;

// ── Requests ─────────────────────────────────────────────────────────────────

public record RealizarTransferenciaRequest(
    Guid CuentaOrigenId,
    Guid CuentaDestinoId,
    decimal Monto,
    string Descripcion
);

public record CrearCuentaRequest(
    string NumeroCuenta,
    string NombreTitular,
    decimal SaldoInicial
);

// ── Responses ────────────────────────────────────────────────────────────────

public record CuentaResponse(
    Guid Id,
    string NumeroCuenta,
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
    string NumeroCuenta,
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

public record UsuarioRegistradoResponse(
    Guid Id,
    string Nombre,
    string Email,
    DateTime FechaRegistro
);
