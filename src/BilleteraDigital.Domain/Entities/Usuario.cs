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

    // ── Soft Delete ──────────────────────────────────────────────────────────
    /// <summary>Indica si el usuario fue eliminado lógicamente.</summary>
    public bool EstaEliminado { get; private set; } = false;

    /// <summary>Fecha UTC de la eliminación lógica. Null si el usuario está activo.</summary>
    public DateTime? FechaEliminacion { get; private set; }

    // ── Navegación: cuentas del usuario ─────────────────────────────────────
    private readonly List<Cuenta> _cuentas = [];

    /// <summary>Cuentas bancarias asociadas a este usuario.</summary>
    public IReadOnlyCollection<Cuenta> Cuentas => _cuentas.AsReadOnly();

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

    // ── Comportamiento del dominio ───────────────────────────────────────────

    /// <summary>
    /// Elimina lógicamente el usuario (Soft Delete).
    /// Los filtros globales de EF lo ocultarán de todas las consultas.
    /// </summary>
    public void Eliminar()
    {
        EstaEliminado    = true;
        FechaEliminacion = DateTime.UtcNow;
    }
}
