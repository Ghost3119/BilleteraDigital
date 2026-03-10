namespace BilleteraDigital.Domain.Exceptions;

/// <summary>
/// Se lanza cuando el monto de la operación es inválido (negativo o cero).
/// </summary>
public sealed class MontoInvalidoException : DomainException
{
    public MontoInvalidoException(decimal monto)
        : base($"El monto de la operación debe ser mayor a cero. Valor recibido: {monto:C}.") { }
}
