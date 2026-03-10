using BilleteraDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BilleteraDigital.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Usuario.
/// Índice único en Email; longitudes NVARCHAR razonables.
/// Mapea las columnas de Soft Delete.
/// La relación 1-N con Cuentas se declara en CuentaConfiguration.
/// </summary>
internal sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
               .ValueGeneratedNever();

        builder.Property(u => u.Nombre)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(u => u.Email)
               .IsRequired()
               .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
               .IsUnique()
               .HasDatabaseName("UX_Usuarios_Email");

        builder.Property(u => u.PasswordHash)
               .IsRequired()
               .HasMaxLength(512);

        builder.Property(u => u.FechaRegistro)
               .IsRequired();

        // ── Soft Delete ──────────────────────────────────────────────────────
        builder.Property(u => u.EstaEliminado)
               .IsRequired()
               .HasDefaultValue(false);

        builder.Property(u => u.FechaEliminacion)
               .IsRequired(false);

        // La navegación inversa (Cuentas) es mapeada por EF mediante la FK
        // declarada en CuentaConfiguration. No se necesita configuración adicional aquí.
    }
}
