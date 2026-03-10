using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BilleteraDigital.Application.Ports.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BilleteraDigital.API.Infrastructure.Security;

/// <summary>
/// Adaptador de salida: genera tokens JWT firmados con clave simétrica HMACSHA256.
/// La configuración se inyecta desde appsettings.json (sección "Jwt").
/// </summary>
internal sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerarToken(Guid usuarioId, string nombreUsuario, IEnumerable<string> roles)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey no configurada en appsettings.");
        var issuer   = jwtSection["Issuer"]   ?? "BilleteraDigital";
        var audience = jwtSection["Audience"] ?? "BilleteraDigital";
        var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var mins) ? mins : 60;

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, nombreUsuario),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        foreach (var rol in roles)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
