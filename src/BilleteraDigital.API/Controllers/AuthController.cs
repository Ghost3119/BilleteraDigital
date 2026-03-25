using System.Security.Claims;
using System.Security.Cryptography;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.Application.UseCases.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BilleteraDigital.API.Controllers;

/// <summary>
/// Controlador de autenticación: genera y rota tokens JWT + Refresh Token.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IJwtService        _jwtService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher    _passwordHasher;
    private readonly IUnitOfWork        _unitOfWork;

    // Vigencia del Refresh Token en días
    private const int RefreshTokenDias = 7;

    public AuthController(
        IJwtService        jwtService,
        IUsuarioRepository usuarioRepository,
        IPasswordHasher    passwordHasher,
        IUnitOfWork        unitOfWork)
    {
        _jwtService        = jwtService;
        _usuarioRepository = usuarioRepository;
        _passwordHasher    = passwordHasher;
        _unitOfWork        = unitOfWork;
    }

    /// <summary>
    /// Genera un JWT y un Refresh Token para un usuario registrado.
    /// Valida email y contraseña contra la base de datos.
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerarToken(
        [FromBody] LoginConEmailRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Unauthorized(new { error = "Credenciales inválidas." });

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        // Necesitamos la entidad con change-tracking para persistir el Refresh Token
        var usuario = await _usuarioRepository.ObtenerPorEmailTrackedAsync(emailNormalizado, cancellationToken);

        if (usuario is null || !_passwordHasher.Verificar(request.Password, usuario.PasswordHash))
            return Unauthorized(new { error = "Credenciales inválidas." });

        var accessToken  = _jwtService.GenerarToken(usuario.Id, usuario.Nombre, usuario.Email, ["Usuario"]);
        var refreshToken = GenerarRefreshTokenSeguro();

        usuario.ActualizarRefreshToken(refreshToken, DateTime.UtcNow.AddDays(RefreshTokenDias));
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return Ok(new TokenResponse(
            AccessToken:  accessToken,
            RefreshToken: refreshToken,
            ExpiraEn:     "60 minutos",
            Tipo:         "Bearer"
        ));
    }

    /// <summary>
    /// Rota el par JWT / Refresh Token.
    /// Recibe el access token (puede estar expirado) y el refresh token activo.
    /// Si ambos son válidos, devuelve un nuevo par de tokens.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AccessToken) ||
            string.IsNullOrWhiteSpace(request.RefreshToken))
            return Unauthorized(new { error = "Tokens inválidos." });

        // 1. Extraer claims del access token (firma válida, ignorar expiración)
        var principal = _jwtService.ObtenerPrincipalDeTokenExpirado(request.AccessToken);
        if (principal is null)
            return Unauthorized(new { error = "Access token inválido." });

        var subClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (subClaim is null || !Guid.TryParse(subClaim, out var usuarioId))
            return Unauthorized(new { error = "Access token inválido: sub claim ausente." });

        // 2. Cargar usuario con tracking para poder persistir el nuevo refresh token
        var usuario = await _usuarioRepository.ObtenerPorIdTrackedAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return Unauthorized(new { error = "Usuario no encontrado." });

        // 3. Validar el Refresh Token almacenado
        if (usuario.RefreshToken is null ||
            !usuario.RefreshToken.Equals(request.RefreshToken, StringComparison.Ordinal) ||
            usuario.RefreshTokenExpiryTime is null ||
            usuario.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Unauthorized(new { error = "Refresh token inválido o expirado." });
        }

        // 4. Rotar: generar nuevo par y persistir
        var nuevoAccessToken  = _jwtService.GenerarToken(usuario.Id, usuario.Nombre, usuario.Email, ["Usuario"]);
        var nuevoRefreshToken = GenerarRefreshTokenSeguro();

        usuario.ActualizarRefreshToken(nuevoRefreshToken, DateTime.UtcNow.AddDays(RefreshTokenDias));
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        return Ok(new TokenResponse(
            AccessToken:  nuevoAccessToken,
            RefreshToken: nuevoRefreshToken,
            ExpiraEn:     "60 minutos",
            Tipo:         "Bearer"
        ));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Genera un Refresh Token criptográficamente seguro de 64 bytes en Base64.
    /// </summary>
    private static string GenerarRefreshTokenSeguro()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
