namespace BilleteraDigital.Application.UseCases.Usuario;

// ── Requests ──────────────────────────────────────────────────────────────────

public record RegistrarUsuarioRequest(
    string Nombre,
    string Email,
    string Password
);

// ── Responses ─────────────────────────────────────────────────────────────────

public record UsuarioRegistradoResponse(
    Guid Id,
    string Nombre,
    string Email,
    DateTime FechaRegistro
);
