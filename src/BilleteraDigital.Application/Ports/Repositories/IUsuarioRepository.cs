using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Ports.Repositories;

/// <summary>
/// Puerto de salida (driven port) para el repositorio de usuarios.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Igual que <see cref="ObtenerPorEmailAsync"/> pero devuelve la entidad con
    /// change-tracking activo para operaciones que necesiten persistir cambios (p.ej. Refresh Token).
    /// </summary>
    Task<Usuario?> ObtenerPorEmailTrackedAsync(string email, CancellationToken cancellationToken = default);

    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Igual que <see cref="ObtenerPorIdAsync"/> pero con change-tracking activo.
    /// </summary>
    Task<Usuario?> ObtenerPorIdTrackedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario> CrearAsync(Usuario usuario, CancellationToken cancellationToken = default);

    /// <summary>Marca la entidad como modificada para que EF Core genere el UPDATE correspondiente.</summary>
    Task ActualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
