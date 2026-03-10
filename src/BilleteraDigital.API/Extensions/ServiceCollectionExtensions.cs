using System.Text;
using BilleteraDigital.API.Infrastructure.Persistence;
using BilleteraDigital.API.Infrastructure.Persistence.Repositories;
using BilleteraDigital.API.Infrastructure.Security;
using BilleteraDigital.Application.Mappings;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.Application.UseCases.Cuenta;
using BilleteraDigital.Application.UseCases.Transferencia;
using BilleteraDigital.Application.UseCases.Usuario;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace BilleteraDigital.API.Extensions;

/// <summary>
/// Extensiones de IServiceCollection para registrar todos los servicios de la aplicación.
/// Centraliza la configuración del contenedor de DI siguiendo el principio de responsabilidad única.
/// </summary>
public static class ServiceCollectionExtensions
{
    // ── Persistencia ─────────────────────────────────────────────────────────
    public static IServiceCollection AddPersistencia(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<BilleteraDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("BilleteraDigital"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

        // Registrar repositorios (adaptadores de salida)
        services.AddScoped<ICuentaRepository, CuentaRepository>();
        services.AddScoped<ITransaccionRepository, TransaccionRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    // ── Casos de uso (Application) ────────────────────────────────────────
    public static IServiceCollection AddCasosDeUso(this IServiceCollection services)
    {
        services.AddScoped<RealizarTransferencia>();
        services.AddScoped<ConsultarSaldo>();
        services.AddScoped<CrearCuenta>();
        services.AddScoped<ObtenerHistorialTransacciones>();
        services.AddScoped<RegistrarUsuario>();

        // AutoMapper: descubre MappingProfile y todos los perfiles en el assembly de Application.
        // AddAutoMapper está integrado en AutoMapper 13+ sin paquete adicional.
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

        return services;
    }

    // ── Seguridad JWT ────────────────────────────────────────────────────
    public static IServiceCollection AddSeguridadJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey no configurada.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtSection["Issuer"],
                ValidAudience            = jwtSection["Audience"],
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew                = TimeSpan.Zero
            };
        });

        services.AddAuthorization();
        return services;
    }

    // ── CORS ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Nombre de la política CORS utilizada en toda la aplicación.
    /// Referenciarlo desde Program.cs evita cadenas mágicas duplicadas.
    /// </summary>
    public const string CorsPolicyName = "BilleteraDigitalCors";

    public static IServiceCollection AddCorsBilletera(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Los orígenes permitidos se leen desde la configuración para no hardcodear
        // URLs de frontend en el código fuente.
        // Ejemplo en appsettings.Development.json:
        //   "Cors": { "AllowedOrigins": ["http://localhost:4200", "http://localhost:3000"] }
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins);
                else
                    policy.AllowAnyOrigin();

                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    // Exponer X-Pagination para que el navegador permita al frontend leerlo.
                    // Sin esto, CORS bloquea el acceso al header desde JavaScript.
                    .WithExposedHeaders("X-Pagination");
            });
        });

        return services;
    }

    // ── Swagger / OpenAPI ────────────────────────────────────────────────
    public static IServiceCollection AddSwaggerDocumentacion(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "BilleteraDigital API",
                Version     = "v1",
                Description = "API REST para gestión de billetera digital con arquitectura hexagonal.",
                Contact     = new OpenApiContact { Name = "BilleteraDigital Team" }
            });

            // Habilitar autorización JWT en Swagger UI
            // Microsoft.OpenApi 2.x: AddSecurityDefinition toma IOpenApiSecurityScheme
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Ingresa el token JWT. Ejemplo: Bearer {token}"
            });

            // Microsoft.OpenApi 2.x: AddSecurityRequirement recibe Func<OpenApiDocument, OpenApiSecurityRequirement>
            // OpenApiSecurityRequirement es Dictionary<OpenApiSecuritySchemeReference, IList<string>>
            options.AddSecurityRequirement(doc =>
            {
                var requirement = new OpenApiSecurityRequirement();
                requirement.Add(
                    new OpenApiSecuritySchemeReference("Bearer", doc),
                    new List<string>());
                return requirement;
            });
        });

        return services;
    }
}
