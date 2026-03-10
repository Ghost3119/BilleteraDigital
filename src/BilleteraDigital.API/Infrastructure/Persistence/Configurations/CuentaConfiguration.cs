using BilleteraDigital.Domain.Entities;
using BilleteraDigital.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BilleteraDigital.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Cuenta.
/// Usa tipos de datos precisos para importes financieros (DECIMAL(18,2)).
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

        // Navegación hacia transacciones
        builder.HasMany(c => c.Transacciones)
               .WithOne()
               .HasForeignKey(t => t.CuentaOrigenId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
