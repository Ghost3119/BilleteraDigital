using System.Security.Claims;

namespace BilleteraDigital.Application.Ports.Services;

/// <summary>
/// Puerto de salida para la generación y validación de tokens JWT.
/// La infraestructura provee la implementación concreta.
/// </summary>
public interface IJwtService
{
    string GenerarToken(Guid usuarioId, string nombreUsuario, string email, IEnumerable<string> roles);

    /// <summary>
    /// Valida la firma y los claims de un JWT que puede estar expirado.
    /// Usado exclusivamente en el flujo de Refresh Token para extraer la identidad
    /// del access token anterior sin rechazarlo por expiración.
    /// </summary>
    /// <returns>El <see cref="ClaimsPrincipal"/> extraído, o <c>null</c> si la firma es inválida.</returns>
    ClaimsPrincipal? ObtenerPrincipalDeTokenExpirado(string accessToken);
}
