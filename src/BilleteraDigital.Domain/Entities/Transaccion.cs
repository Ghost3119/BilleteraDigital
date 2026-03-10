using BilleteraDigital.Domain.Enums;

namespace BilleteraDigital.Domain.Entities;

/// <summary>
/// Entidad que representa un movimiento financiero atómico.
/// Es inmutable una vez creada: los movimientos financieros no se modifican.
/// </summary>
public sealed class Transaccion
{
    // ── Propiedades ──────────────────────────────────────────────────────────
    public Guid Id { get; private set; }
    public Guid CuentaOrigenId { get; private set; }
    public Guid? CuentaDestinoId { get; private set; }
    public TipoTransaccion Tipo { get; private set; }
    public decimal Monto { get; private set; }

    /// <summary>Saldo de la cuenta origen después de aplicar esta transacción.</summary>
    public decimal SaldoResultante { get; private set; }

    public string Descripcion { get; private set; }
    public DateTime FechaHora { get; private set; }

    // ── Constructor privado para EF Core ────────────────────────────────────
#pragma warning disable CS8618 // EF Core utiliza este constructor vía reflexión; las propiedades se inicializan desde la BD.
    private Transaccion() { }
#pragma warning restore CS8618

    // ── Fábrica: Transferencia ────────────────────────────────────────────
    /// <summary>
    /// Crea el registro de una transferencia entre dos cuentas.
    /// </summary>
    internal static Transaccion CrearTransferencia(
        Guid cuentaOrigenId,
        Guid cuentaDestinoId,
        decimal monto,
        string descripcion,
        decimal saldoResultante) => new()
    {
        Id = Guid.NewGuid(),
        CuentaOrigenId = cuentaOrigenId,
        CuentaDestinoId = cuentaDestinoId,
        Tipo = TipoTransaccion.Transferencia,
        Monto = monto,
        Descripcion = descripcion,
        SaldoResultante = saldoResultante,
        FechaHora = DateTime.UtcNow
    };

    // ── Fábrica: Crédito ──────────────────────────────────────────────────
    internal static Transaccion CrearCredito(
        Guid cuentaId,
        decimal monto,
        string descripcion,
        decimal saldoResultante) => new()
    {
        Id = Guid.NewGuid(),
        CuentaOrigenId = cuentaId,
        Tipo = TipoTransaccion.Deposito,
        Monto = monto,
        Descripcion = descripcion,
        SaldoResultante = saldoResultante,
        FechaHora = DateTime.UtcNow
    };

    // ── Fábrica: Débito ───────────────────────────────────────────────────
    internal static Transaccion CrearDebito(
        Guid cuentaId,
        decimal monto,
        string descripcion,
        decimal saldoResultante) => new()
    {
        Id = Guid.NewGuid(),
        CuentaOrigenId = cuentaId,
        Tipo = TipoTransaccion.Retiro,
        Monto = monto,
        Descripcion = descripcion,
        SaldoResultante = saldoResultante,
        FechaHora = DateTime.UtcNow
    };
}
