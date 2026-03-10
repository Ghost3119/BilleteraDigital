using BilleteraDigital.Domain.Entities;
using BilleteraDigital.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BilleteraDigital.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Cuenta.
/// Usa tipos de datos precisos para importes financieros (DECIMAL(18,2)).
/// Mapea la FK hacia Usuario y las columnas de Soft Delete.
/// </summary>
internal sealed class CuentaConfiguration : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("Cuentas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .ValueGeneratedNever();

        builder.Property(c => c.NumeroCuenta)
               .IsRequired()
               .HasMaxLength(20);

        builder.HasIndex(c => c.NumeroCuenta)
               .IsUnique()
               .HasDatabaseName("UX_Cuentas_NumeroCuenta");

        builder.Property(c => c.NombreTitular)
               .IsRequired()
               .HasMaxLength(150);

        // Tipo financiero preciso: DECIMAL(18,2) — saldo nunca en float/double
        builder.Property(c => c.Saldo)
               .HasColumnType("DECIMAL(18,2)")
               .IsRequired();

        builder.Property(c => c.Estado)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(c => c.FechaCreacion)
               .IsRequired();

        builder.Property(c => c.FechaUltimaOperacion)
               .IsRequired(false);

        // ── Relación 1-N: Usuario → Cuentas ─────────────────────────────────
        // UsuarioId es nullable para preservar cuentas históricas sin propietario.
        builder.Property(c => c.UsuarioId)
               .IsRequired(false);

        builder.HasOne(c => c.Usuario)
               .WithMany(u => u.Cuentas)
               .HasForeignKey(c => c.UsuarioId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Cuentas_Usuarios_UsuarioId");

        builder.HasIndex(c => c.UsuarioId)
               .HasDatabaseName("IX_Cuentas_UsuarioId");

        // ── Soft Delete ──────────────────────────────────────────────────────
        builder.Property(c => c.EstaEliminado)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(c => c.FechaEliminacion)
               .IsRequired(false);

        // ── Navegación hacia transacciones ───────────────────────────────────
        builder.HasMany(c => c.Transacciones)
               .WithOne()
               .HasForeignKey(t => t.CuentaOrigenId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
