using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilleteraDigital.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Adaptador de salida (driven adapter): implementación EF Core de IUsuarioRepository.
/// </summary>
internal sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly BilleteraDbContext _context;

    public UsuarioRepository(BilleteraDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorEmailAsync(
        string email, CancellationToken cancellationToken = default)
        => await _context.Usuarios
                         .AsNoTracking()
                         .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<Usuario?> ObtenerPorIdAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await _context.Usuarios
                         .AsNoTracking()
                         .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<bool> ExistePorEmailAsync(
        string email, CancellationToken cancellationToken = default)
        => await _context.Usuarios
                         .AsNoTracking()
                         .AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<Usuario> CrearAsync(
        Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        return usuario;
    }
}
