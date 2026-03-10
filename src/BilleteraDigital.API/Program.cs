using BilleteraDigital.API.Extensions;
using BilleteraDigital.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Registro de servicios ────────────────────────────────────────────────────

builder.Services.AddControllers();

// Persistencia: DbContext + Repositorios (adaptadores de salida)
builder.Services.AddPersistencia(builder.Configuration);

// Casos de uso (capa de Aplicación)
builder.Services.AddCasosDeUso();

// Seguridad JWT
builder.Services.AddSeguridadJwt(builder.Configuration);

// Documentación Swagger/OpenAPI
builder.Services.AddSwaggerDocumentacion();

// ── Pipeline HTTP ────────────────────────────────────────────────────────────

var app = builder.Build();

// Middleware global de manejo de errores — debe ser el primero en el pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BilleteraDigital API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raíz: http://localhost:5000/
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
