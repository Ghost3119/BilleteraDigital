using System.Security.Claims;
using System.Text.Json;
using BilleteraDigital.Application.Common;
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
    private static readonly JsonSerializerOptions _camelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly CrearCuenta _crearCuenta;
    private readonly ConsultarSaldo _consultarSaldo;
    private readonly ConsultarMisCuentas _consultarMisCuentas;
    private readonly ObtenerHistorialTransacciones _obtenerHistorial;
    private readonly RealizarTransferencia _realizarTransferencia;

    public CuentasController(
        CrearCuenta crearCuenta,
        ConsultarSaldo consultarSaldo,
        ConsultarMisCuentas consultarMisCuentas,
        ObtenerHistorialTransacciones obtenerHistorial,
        RealizarTransferencia realizarTransferencia)
    {
        _crearCuenta           = crearCuenta;
        _consultarSaldo        = consultarSaldo;
        _consultarMisCuentas   = consultarMisCuentas;
        _obtenerHistorial      = obtenerHistorial;
        _realizarTransferencia = realizarTransferencia;
    }

    /// <summary>
    /// Crea una nueva cuenta para el usuario autenticado.
    /// No requiere body: el número de cuenta es generado por la base de datos (secuencia SQL Server),
    /// el nombre del titular proviene del JWT y el saldo inicial es siempre cero.
    /// El usuario se identifica exclusivamente a través del JWT.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CuentaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CrearCuenta(CancellationToken cancellationToken)
    {
        // Extraer el UsuarioId del claim "sub" del JWT.
        // ASP.NET Core mapea JwtRegisteredClaimNames.Sub → ClaimTypes.NameIdentifier.
        var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (subClaim is null || !Guid.TryParse(subClaim, out var usuarioId))
            return Unauthorized(new { error = "Token inválido: no se pudo identificar al usuario." });

        var command   = new CrearCuentaCommand(usuarioId);
        var resultado = await _crearCuenta.EjecutarAsync(command, cancellationToken);

        if (!resultado.EsExitoso)
            return BadRequest(new { error = resultado.Error });

        return CreatedAtAction(
            nameof(ConsultarSaldo),
            new { id = resultado.Valor!.Id },
            resultado.Valor);
    }

    /// <summary>
    /// Devuelve una página de cuentas que pertenecen al usuario autenticado.
    /// Útil para que el frontend descubra el/los cuentaId(s) tras el login
    /// sin necesidad de almacenarlos manualmente en localStorage.
    /// </summary>
    [HttpGet("mias")]
    [ProducesResponseType(typeof(IEnumerable<CuentaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObtenerMisCuentas(
        [FromQuery] GenericQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (subClaim is null || !Guid.TryParse(subClaim, out var usuarioId))
            return Unauthorized(new { error = "Token inválido: no se pudo identificar al usuario." });

        var resultado = await _consultarMisCuentas.EjecutarAsync(usuarioId, queryParams, cancellationToken);

        var pagedResult = resultado.Valor!;

        var paginationMetadata = new
        {
            pagedResult.TotalCount,
            pagedResult.PageSize,
            pagedResult.PageNumber,
            pagedResult.TotalPages,
        };

        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationMetadata, _camelCase);

        return Ok(pagedResult.Items);
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

    /// <summary>Obtiene el historial filtrado y paginado de transacciones de una cuenta.</summary>
    /// <param name="id">ID de la cuenta.</param>
    /// <param name="queryParams.pageNumber">Número de página (mínimo 1, por defecto 1).</param>
    /// <param name="queryParams.pageSize">Registros por página (entre 1 y 50, por defecto 10).</param>
    /// <param name="queryParams.filtersBase64">
    /// Filtros dinámicos: objeto JSON serializado en Base64.
    /// Claves soportadas: <c>fechaInicio</c>, <c>fechaFin</c>, <c>tipo</c>,
    /// <c>montoMinimo</c>, <c>montoMaximo</c>.
    /// Ejemplo JSON antes de codificar:
    /// <code>{"fechaInicio":"2025-01-01","fechaFin":"2025-12-31","tipo":"Transferencia"}</code>
    /// </param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <remarks>
    /// Los metadatos de paginación (TotalCount, TotalPages, PageNumber, PageSize) se devuelven
    /// en el header <c>X-Pagination</c> como JSON. El body contiene únicamente el array de ítems.
    /// </remarks>
    [HttpGet("{id:guid}/transacciones")]
    [ProducesResponseType(typeof(IEnumerable<TransaccionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerHistorial(
        Guid id,
        [FromQuery] GenericQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var resultado = await _obtenerHistorial.EjecutarAsync(id, queryParams, cancellationToken);
        if (!resultado.EsExitoso)
            return NotFound(new { error = resultado.Error });

        var pagedResult = resultado.Valor!;

        var paginationMetadata = new
        {
            pagedResult.TotalCount,
            pagedResult.PageSize,
            pagedResult.PageNumber,
            pagedResult.TotalPages
        };

        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationMetadata, _camelCase);

        return Ok(pagedResult.Items);
    }

    /// <summary>
    /// Realiza una transferencia de fondos desde una cuenta del usuario autenticado.
    /// El destinatario se identifica con su correo electrónico o número de cuenta (no UUID).
    /// El usuario debe ser el titular de la cuenta origen; de lo contrario se devuelve 403.
    /// </summary>
    /// <param name="request">
    /// Cuenta origen (Id), destinatario (email o número de cuenta), monto y descripción.
    /// </param>
    [HttpPost("transferencias")]
    [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RealizarTransferencia(
        [FromBody] RealizarTransferenciaRequest request,
        CancellationToken cancellationToken)
    {
        var subClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (subClaim is null || !Guid.TryParse(subClaim, out var usuarioId))
            return Unauthorized(new { error = "Token inválido: no se pudo identificar al usuario." });

        var command   = new TransferenciaCommand(usuarioId, request.CuentaOrigenId, request.Destinatario, request.Monto, request.Descripcion);
        var resultado = await _realizarTransferencia.EjecutarAsync(command, cancellationToken);

        if (!resultado.EsExitoso)
            return UnprocessableEntity(new { error = resultado.Error });

        return Ok(resultado.Valor);
    }
}

