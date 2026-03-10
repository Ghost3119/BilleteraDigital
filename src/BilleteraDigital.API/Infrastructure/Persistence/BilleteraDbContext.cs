using BilleteraDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BilleteraDigital.API.Infrastructure.Persistence;

/// <summary>
/// DbContext de la billetera digital.
/// Actúa como adaptador de salida (driven adapter) para la persistencia SQL Server.
/// </summary>
public sealed class BilleteraDbContext : DbContext
{
    public BilleteraDbContext(DbContextOptions<BilleteraDbContext> options) : base(options) { }

    public DbSet<Cuenta> Cuentas => Set<Cuenta>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica todas las configuraciones del assembly actual (Fluent API)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BilleteraDbContext).Assembly);
    }
}
