using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.UseCases.Usuario;
using Microsoft.AspNetCore.Mvc;

namespace BilleteraDigital.API.Controllers;

/// <summary>
/// Controlador de usuarios: expone operaciones de registro.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class UsuariosController : ControllerBase
{
    private readonly RegistrarUsuario _registrarUsuario;

    public UsuariosController(RegistrarUsuario registrarUsuario)
    {
        _registrarUsuario = registrarUsuario;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// La contraseña nunca se almacena en texto plano.
    /// </summary>
    /// <param name="request">Datos del nuevo usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Datos del usuario registrado.</returns>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(UsuarioRegistradoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _registrarUsuario.EjecutarAsync(request, cancellationToken);

        if (!resultado.EsExitoso)
        {
            // 409 si el email ya está registrado, 400 para otros errores de validación
            var esConflicto = resultado.Error?.Contains("Ya existe") == true;
            return esConflicto
                ? Conflict(new { error = resultado.Error })
                : BadRequest(new { error = resultado.Error });
        }

        return CreatedAtAction(
            nameof(Registrar),
            new { id = resultado.Valor!.Id },
            resultado.Valor);
    }
}
