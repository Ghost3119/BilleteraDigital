using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;
using Microsoft.AspNetCore.Mvc;

namespace BilleteraDigital.API.Controllers;

/// <summary>
/// Controlador de autenticación: genera tokens JWT validando credenciales contra la base de datos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IJwtService       _jwtService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher   _passwordHasher;

    public AuthController(
        IJwtService        jwtService,
        IUsuarioRepository usuarioRepository,
        IPasswordHasher    passwordHasher)
    {
        _jwtService        = jwtService;
        _usuarioRepository = usuarioRepository;
        _passwordHasher    = passwordHasher;
    }

    /// <summary>
    /// Genera un token JWT para un usuario registrado.
    /// Valida email y contraseña contra la base de datos.
    /// </summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerarToken(
        [FromBody] LoginConEmailRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Unauthorized(new { error = "Credenciales inválidas." });

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var usuario = await _usuarioRepository.ObtenerPorEmailAsync(emailNormalizado, cancellationToken);

        if (usuario is null || !_passwordHasher.Verificar(request.Password, usuario.PasswordHash))
            return Unauthorized(new { error = "Credenciales inválidas." });

        var token = _jwtService.GenerarToken(
            usuarioId:     usuario.Id,
            nombreUsuario: usuario.Nombre,
            roles:         ["Usuario"]);

        return Ok(new
        {
            token,
            expiraEn = "60 minutos",
            tipo     = "Bearer"
        });
    }
}
