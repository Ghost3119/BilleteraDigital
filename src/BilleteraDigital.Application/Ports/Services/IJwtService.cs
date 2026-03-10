namespace BilleteraDigital.Application.Ports.Services;

/// <summary>
/// Puerto de salida para la generación de tokens JWT.
/// La infraestructura provee la implementación concreta.
/// </summary>
public interface IJwtService
{
    string GenerarToken(Guid usuarioId, string nombreUsuario, IEnumerable<string> roles);
}
