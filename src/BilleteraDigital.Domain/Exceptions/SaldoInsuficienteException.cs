namespace BilleteraDigital.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta realizar una transferencia con saldo insuficiente.
/// </summary>
public sealed class SaldoInsuficienteException : DomainException
{
    public SaldoInsuficienteException(decimal saldoActual, decimal montoSolicitado)
        : base($"Saldo insuficiente. Saldo disponible: {saldoActual:C}, monto solicitado: {montoSolicitado:C}.") { }
}
