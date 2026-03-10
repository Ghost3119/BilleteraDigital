using System.Security.Claims;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.UseCases.Cuenta;
using BilleteraDigital.Application.UseCases.Transferencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BilleteraDigital.API.Controllers;

/// <summary>
/// Adaptador de entrada (driving adapter): expone los casos de uso de cuentas como endpoints REST.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public sealed class CuentasController : ControllerBase
{
    private readonly CrearCuenta _crearCuenta;
    private readonly ConsultarSaldo _consultarSaldo;
    private readonly ObtenerHistorialTransacciones _obtenerHistorial;
    private readonly RealizarTransferencia _realizarTransferencia;

    public CuentasController(
        CrearCuenta crearCuenta,
        ConsultarSaldo consultarSaldo,
        ObtenerHistorialTransacciones obtenerHistorial,
        RealizarTransferencia realizarTransferencia)
    {
        _crearCuenta           = crearCuenta;
        _consultarSaldo        = consultarSaldo;
        _obtenerHistorial      = obtenerHistorial;
        _realizarTransferencia = realizarTransferencia;
    }

    /// <summary>
    /// Crea una nueva cuenta para el usuario autenticado.
    /// El número de cuenta y el nombre del titular son generados/resueltos por el sistema.
    /// El usuario se identifica exclusivamente a través del JWT — no se acepta ningún ID en el body.
    /// </summary>
    /// <param name="request">Saldo inicial opcional (por defecto 0).</param>
    [HttpPost]
    [ProducesResponseType(typeof(CuentaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CrearCuenta(
        [FromBody] CrearCuentaRequest request,
        CancellationToken cancellationToken)
    {
        // Extraer el UsuarioId del claim "sub" del JWT.
        // ASP.NET Core mapea JwtRegisteredClaimNames.Sub → ClaimTypes.NameIdentifier.
        var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (subClaim is null || !Guid.TryParse(subClaim, out var usuarioId))
            return Unauthorized(new { error = "Token inválido: no se pudo identificar al usuario." });

        var command  = new CrearCuentaCommand(usuarioId, request.SaldoInicial);
        var resultado = await _crearCuenta.EjecutarAsync(command, cancellationToken);

        if (!resultado.EsExitoso)
            return BadRequest(new { error = resultado.Error });

        return CreatedAtAction(
            nameof(ConsultarSaldo),
            new { id = resultado.Valor!.Id },
            resultado.Valor);
    }

    /// <summary>Consulta el saldo disponible de una cuenta.</summary>
    /// <param name="id">ID de la cuenta.</param>
    [HttpGet("{id:guid}/saldo")]
    [ProducesResponseType(typeof(SaldoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConsultarSaldo(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _consultarSaldo.EjecutarAsync(id, cancellationToken);
        if (!resultado.EsExitoso)
            return NotFound(new { error = resultado.Error });

        return Ok(resultado.Valor);
    }

    /// <summary>Obtiene el historial de transacciones de una cuenta.</summary>
    /// <param name="id">ID de la cuenta.</param>
    [HttpGet("{id:guid}/transacciones")]
    [ProducesResponseType(typeof(IEnumerable<TransaccionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerHistorial(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _obtenerHistorial.EjecutarAsync(id, cancellationToken);
        if (!resultado.EsExitoso)
            return NotFound(new { error = resultado.Error });

        return Ok(resultado.Valor);
    }

    /// <summary>Realiza una transferencia de fondos entre dos cuentas.</summary>
    /// <param name="request">Datos de la transferencia.</param>
    [HttpPost("transferencias")]
    [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RealizarTransferencia(
        [FromBody] RealizarTransferenciaRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await _realizarTransferencia.EjecutarAsync(request, cancellationToken);
        if (!resultado.EsExitoso)
            return UnprocessableEntity(new { error = resultado.Error });

        return Ok(resultado.Valor);
    }
}

