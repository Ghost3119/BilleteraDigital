using Microsoft.AspNetCore.Mvc;

namespace BilleteraDigital.API.Controllers;

/// <summary>
/// Controlador de salud del sistema.
/// Permite monitorear que la API está en línea y operativa.
/// Es un estándar de la industria para sistemas en producción.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Devuelve el estado operativo actual de la API.
    /// No requiere autenticación: debe ser accesible por herramientas de monitoreo externas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthResponse(
            Estado:   "Operativo",
            Version:  "1.0",
            FechaHora: DateTime.UtcNow
        ));
    }
}

/// <summary>Respuesta estándar del endpoint de salud.</summary>
public sealed record HealthResponse(
    string   Estado,
    string   Version,
    DateTime FechaHora
);