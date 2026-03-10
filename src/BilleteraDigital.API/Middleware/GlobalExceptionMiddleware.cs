using System.Net;
using System.Text.Json;
using BilleteraDigital.Domain.Exceptions;

namespace BilleteraDigital.API.Middleware;

/// <summary>
/// Middleware global de manejo de errores.
/// Intercepta excepciones no controladas y devuelve respuestas JSON uniformes.
/// Aísla la infraestructura de presentación del resto de la arquitectura.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await ManejarExcepcionAsync(context, ex);
        }
    }

    private async Task ManejarExcepcionAsync(HttpContext context, Exception excepcion)
    {
        var (statusCode, titulo, detalle) = excepcion switch
        {
            AccesoDenegadoException accesoDenegadoEx =>
                (HttpStatusCode.Forbidden, "Acceso denegado", accesoDenegadoEx.Message),

            DomainException domainEx =>
                (HttpStatusCode.UnprocessableEntity, "Regla de negocio violada", domainEx.Message),

            ArgumentException argEx =>
                (HttpStatusCode.BadRequest, "Parámetro inválido", argEx.Message),

            KeyNotFoundException keyEx =>
                (HttpStatusCode.NotFound, "Recurso no encontrado", keyEx.Message),

            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, "No autorizado", "Credenciales inválidas o token expirado."),

            _ =>
                (HttpStatusCode.InternalServerError, "Error interno del servidor",
                 "Ocurrió un error inesperado. Por favor intente nuevamente.")
        };

        // Solo loguear detalles técnicos en el servidor, nunca exponerlos al cliente
        _logger.LogError(excepcion, "Excepción interceptada: {Tipo} | {Mensaje}", excepcion.GetType().Name, excepcion.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var respuesta = new
        {
            estado = (int)statusCode,
            titulo,
            detalle,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(respuesta, _jsonOptions));
    }
}
