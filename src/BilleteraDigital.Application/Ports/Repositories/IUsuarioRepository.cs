using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Ports.Repositories;

/// <summary>
/// Puerto de salida (driven port) para el repositorio de usuarios.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistePorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario> CrearAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
