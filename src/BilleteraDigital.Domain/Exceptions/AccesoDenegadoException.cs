namespace BilleteraDigital.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un usuario autenticado intenta operar sobre una cuenta
/// que no le pertenece. Produce HTTP 403 Forbidden.
/// </summary>
public sealed class AccesoDenegadoException : DomainException
{
    public AccesoDenegadoException(Guid cuentaId)
        : base($"No tienes permisos para operar la cuenta '{cuentaId}'.") { }
}
