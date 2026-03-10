namespace BilleteraDigital.Domain.Exceptions;

/// <summary>
/// Excepción base del dominio. Todas las reglas de negocio violadas lanzan este tipo.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string mensaje) : base(mensaje) { }
}
