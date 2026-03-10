namespace BilleteraDigital.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un usuario registrado en el sistema.
/// El hash de la contraseña se almacena aquí como opaque string; la lógica de
/// hashing vive en la infraestructura (puerto IPasswordHasher).
/// </summary>
public sealed class Usuario
{
    // ── Propiedades ──────────────────────────────────────────────────────────
    public Guid   Id              { get; private set; }
    public string Nombre          { get; private set; }
    public string Email           { get; private set; }
    public string PasswordHash    { get; private set; }
    public DateTime FechaRegistro { get; private set; }

    // ── Constructor privado para EF Core ────────────────────────────────────
#pragma warning disable CS8618
    private Usuario() { }
#pragma warning restore CS8618

    // ── Constructor de creación ──────────────────────────────────────────────
    /// <summary>
    /// Crea un nuevo usuario. El llamador es responsable de pasar el hash ya
    /// calculado — la contraseña en texto plano nunca entra al dominio.
    /// </summary>
    public Usuario(string nombre, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre no puede estar vacío.", nameof(nombre));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña no puede estar vacío.", nameof(passwordHash));

        Id             = Guid.NewGuid();
        Nombre         = nombre;
        Email          = email.Trim().ToLowerInvariant();
        PasswordHash   = passwordHash;
        FechaRegistro  = DateTime.UtcNow;
    }
}
