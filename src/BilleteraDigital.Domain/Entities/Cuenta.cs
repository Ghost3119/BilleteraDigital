using BilleteraDigital.Domain.Enums;
using BilleteraDigital.Domain.Exceptions;

namespace BilleteraDigital.Domain.Entities;

/// <summary>
/// Entidad raíz del agregado Cuenta.
/// Encapsula toda la lógica de negocio relacionada con el saldo y las operaciones financieras.
/// No depende de ninguna librería externa: es POCO puro.
/// </summary>
public sealed class Cuenta
{
    // ── Propiedades de identidad e información ────────────────────────────────
    public Guid Id { get; private set; }
    public string NumeroCuenta { get; private set; }
    public string NombreTitular { get; private set; }
    public decimal Saldo { get; private set; }
    public EstadoCuenta Estado { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaUltimaOperacion { get; private set; }

    // ── Relación con Usuario (FK) ────────────────────────────────────────────
    /// <summary>Clave foránea al Usuario propietario de esta cuenta. Nullable para preservar compatibilidad con cuentas históricas.</summary>
    public Guid? UsuarioId { get; private set; }

    /// <summary>Propiedad de navegación hacia el Usuario propietario.</summary>
    public Usuario? Usuario { get; private set; }

    // ── Soft Delete ──────────────────────────────────────────────────────────
    /// <summary>Indica si la cuenta fue eliminada lógicamente. Los filtros globales de EF la excluyen automáticamente.</summary>
    public bool EstaEliminado { get; private set; } = false;

    /// <summary>Fecha UTC en que se realizó la eliminación lógica. Null si la cuenta está activa.</summary>
    public DateTime? FechaEliminacion { get; private set; }

    // ── Navegación ──────────────────────────────────────────────────────────
    private readonly List<Transaccion> _transacciones = [];
    public IReadOnlyCollection<Transaccion> Transacciones => _transacciones.AsReadOnly();

    // ── Constructor privado para EF Core ────────────────────────────────────
#pragma warning disable CS8618 // EF Core utiliza este constructor vía reflexión; las propiedades se inicializan desde la BD.
    private Cuenta() { }
#pragma warning restore CS8618

    // ── Constructor de creación ──────────────────────────────────────────────
    /// <summary>
    /// Crea una nueva cuenta con saldo inicial validado.
    /// </summary>
    public Cuenta(string numeroCuenta, string nombreTitular, decimal saldoInicial = 0m)
    {
        if (string.IsNullOrWhiteSpace(numeroCuenta))
            throw new ArgumentException("El número de cuenta no puede estar vacío.", nameof(numeroCuenta));

        if (string.IsNullOrWhiteSpace(nombreTitular))
            throw new ArgumentException("El nombre del titular no puede estar vacío.", nameof(nombreTitular));

        if (saldoInicial < 0)
            throw new MontoInvalidoException(saldoInicial);

        Id = Guid.NewGuid();
        NumeroCuenta = numeroCuenta;
        NombreTitular = nombreTitular;
        Saldo = saldoInicial;
        Estado = EstadoCuenta.Activa;
        FechaCreacion = DateTime.UtcNow;
    }

    // ── Comportamiento del dominio ───────────────────────────────────────────

    /// <summary>
    /// Acredita fondos en la cuenta. Registra la transacción internamente.
    /// </summary>
    public void Acreditar(decimal monto, string descripcion)
    {
        ValidarActiva();
        ValidarMonto(monto);

        Saldo += monto;
        FechaUltimaOperacion = DateTime.UtcNow;

        _transacciones.Add(Transaccion.CrearCredito(Id, monto, descripcion, Saldo));
    }

    /// <summary>
    /// Debita fondos de la cuenta. Lanza <see cref="SaldoInsuficienteException"/> si el saldo es insuficiente.
    /// </summary>
    public void Debitar(decimal monto, string descripcion)
    {
        ValidarActiva();
        ValidarMonto(monto);

        if (Saldo < monto)
            throw new SaldoInsuficienteException(Saldo, monto);

        Saldo -= monto;
        FechaUltimaOperacion = DateTime.UtcNow;

        _transacciones.Add(Transaccion.CrearDebito(Id, monto, descripcion, Saldo));
    }

    /// <summary>
    /// Bloquea la cuenta impidiendo cualquier operación futura.
    /// </summary>
    public void Bloquear() => Estado = EstadoCuenta.Bloqueada;

    /// <summary>
    /// Desactiva la cuenta.
    /// </summary>
    public void Desactivar() => Estado = EstadoCuenta.Inactiva;

    /// <summary>
    /// Elimina lógicamente la cuenta (Soft Delete).
    /// Una vez eliminada, los filtros globales de EF la ocultarán de todas las consultas.
    /// </summary>
    public void Eliminar()
    {
        EstaEliminado   = true;
        FechaEliminacion = DateTime.UtcNow;
    }

    // ── Validaciones privadas ────────────────────────────────────────────────

    private void ValidarActiva()
    {
        if (Estado != EstadoCuenta.Activa)
            throw new CuentaInactivaException(Id);
    }

    private static void ValidarMonto(decimal monto)
    {
        if (monto <= 0)
            throw new MontoInvalidoException(monto);
    }
}
