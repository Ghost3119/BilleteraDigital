using BilleteraDigital.Domain.Enums;

namespace BilleteraDigital.Application.UseCases.Cuenta;

// ── Commands ──────────────────────────────────────────────────────────────────

/// <summary>
/// Comando interno que viaja del Controller al Use Case.
/// Lleva únicamente el UsuarioId extraído del JWT; no acepta ningún dato del cliente.
/// No es parte del contrato HTTP público.
/// </summary>
public record CrearCuentaCommand(Guid UsuarioId);

// ── Responses ─────────────────────────────────────────────────────────────────

public record CuentaResponse(
    Guid Id,
    long NumeroCuenta,
    string NombreTitular,
    decimal Saldo,
    EstadoCuenta Estado,
    DateTime FechaCreacion,
    DateTime? FechaUltimaOperacion
);

public record SaldoResponse(
    Guid CuentaId,
    long NumeroCuenta,
    string NombreTitular,
    decimal Saldo,
    DateTime ConsultadoEn
);

/// <summary>
/// DTO de salida para el historial de transacciones.
/// Producido por AutoMapper a partir de la entidad <c>Transaccion</c>.
/// Se declara con propiedades init-only para que AutoMapper pueda instanciarlo
/// sin constructor posicional (requerimiento de AutoMapper 13+).
/// </summary>
public record TransaccionDto
{
    /// <summary>Identificador único de la transacción.</summary>
    public Guid Id { get; init; }

    /// <summary>Importe del movimiento.</summary>
    public decimal Monto { get; init; }

    /// <summary>Saldo de la cuenta origen tras la operación.</summary>
    public decimal SaldoResultante { get; init; }

    /// <summary>Fecha y hora UTC del movimiento.</summary>
    public DateTime FechaHora { get; init; }

    /// <summary>Concepto libre del movimiento.</summary>
    public string Descripcion { get; init; } = string.Empty;

    /// <summary>Representación textual del tipo de transacción (p. ej. "Transferencia", "Deposito", "Retiro").</summary>
    public string TipoMovimiento { get; init; } = string.Empty;

    /// <summary>
    /// Indica si el movimiento es un <c>Ingreso</c> o un <c>Egreso</c> desde el punto de vista
    /// de la cuenta consultada. Calculado en el perfil de mapeo con el contexto <c>CuentaId</c>.
    /// </summary>
    public string Direccion { get; init; } = string.Empty;
}
