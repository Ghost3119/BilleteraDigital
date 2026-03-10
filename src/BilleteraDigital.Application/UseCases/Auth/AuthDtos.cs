namespace BilleteraDigital.Application.UseCases.Auth;

// ── Requests ──────────────────────────────────────────────────────────────────

public record LoginConEmailRequest(
    string Email,
    string Password
);

/// <summary>Body del endpoint POST /api/v1/Auth/refresh.</summary>
public record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken
);

// ── Responses ─────────────────────────────────────────────────────────────────

/// <summary>Respuesta unificada para Login y Refresh; incluye ambos tokens.</summary>
public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string ExpiraEn,
    string Tipo
);
