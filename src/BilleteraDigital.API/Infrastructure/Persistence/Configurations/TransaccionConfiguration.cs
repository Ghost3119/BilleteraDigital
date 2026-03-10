using BilleteraDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BilleteraDigital.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Transaccion.
/// Los registros son inmutables: ningún update debería ejecutarse sobre esta tabla.
/// </summary>
internal sealed class TransaccionConfiguration : IEntityTypeConfiguration<Transaccion>
{
    public void Configure(EntityTypeBuilder<Transaccion> builder)
    {
        builder.ToTable("Transacciones");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
               .ValueGeneratedNever();

        builder.Property(t => t.CuentaOrigenId)
               .IsRequired();

        builder.Property(t => t.CuentaDestinoId)
               .IsRequired(false);

        builder.Property(t => t.Tipo)
               .HasConversion<int>()
               .IsRequired();

        // Tipo financiero preciso: DECIMAL(18,2)
        builder.Property(t => t.Monto)
               .HasColumnType("DECIMAL(18,2)")
               .IsRequired();

        builder.Property(t => t.SaldoResultante)
               .HasColumnType("DECIMAL(18,2)")
               .IsRequired();

        builder.Property(t => t.Descripcion)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(t => t.FechaHora)
               .IsRequired();

        // Índice para consultas de historial por cuenta
        builder.HasIndex(t => t.CuentaOrigenId)
               .HasDatabaseName("IX_Transacciones_CuentaOrigenId");

        builder.HasIndex(t => t.FechaHora)
               .HasDatabaseName("IX_Transacciones_FechaHora");
    }
}
