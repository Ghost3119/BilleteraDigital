namespace BilleteraDigital.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se opera sobre una cuenta que no está activa.
/// </summary>
public sealed class CuentaInactivaException : DomainException
{
    public CuentaInactivaException(Guid cuentaId)
        : base($"La cuenta con ID '{cuentaId}' no está activa y no puede operar.") { }
}
