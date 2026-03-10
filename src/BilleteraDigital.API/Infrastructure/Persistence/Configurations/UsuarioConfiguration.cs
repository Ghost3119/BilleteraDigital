using BilleteraDigital.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BilleteraDigital.API.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Usuario.
/// Índice único en Email; longitudes NVARCHAR razonables.
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
    }
}
