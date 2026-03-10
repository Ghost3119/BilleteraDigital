using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilleteraDigital.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Adaptador de salida (driven adapter): implementación EF Core de ICuentaRepository.
/// </summary>
internal sealed class CuentaRepository : ICuentaRepository
{
    private readonly BilleteraDbContext _context;

    public CuentaRepository(BilleteraDbContext context)
    {
        _context = context;
    }

    public async Task<Cuenta?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Cuentas
                         .Include(c => c.Transacciones)
                         .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Cuenta?> ObtenerPorNumeroAsync(string numeroCuenta, CancellationToken cancellationToken = default)
        => await _context.Cuentas
                         .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta, cancellationToken);

    public async Task<IEnumerable<Cuenta>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
        => await _context.Cuentas
                         .AsNoTracking()
                         .ToListAsync(cancellationToken);

    public async Task<Cuenta> CrearAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        await _context.Cuentas.AddAsync(cuenta, cancellationToken);
        return cuenta;
    }

    public Task ActualizarAsync(Cuenta cuenta, CancellationToken cancellationToken = default)
    {
        _context.Cuentas.Update(cuenta);
        return Task.CompletedTask;
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Cuentas.AnyAsync(c => c.Id == id, cancellationToken);
}
