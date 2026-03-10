using AutoMapper;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.Mappings;

/// <summary>
/// Perfil de AutoMapper para la capa de Application.
/// Mapea entidades de dominio a DTOs de salida sin ninguna dependencia
/// en Infrastructure ni en ASP.NET Core.
/// </summary>
public sealed class MappingProfile : Profile
{
    /// <summary>Clave usada en <see cref="IMappingOperationOptions.Items"/> para pasar el CuentaId consultado.</summary>
    public const string CuentaIdContextKey = "CuentaId";

    public MappingProfile()
    {
        // ── Transaccion → TransaccionDto ──────────────────────────────────────
        //
        // TipoMovimiento: representación textual del enum (no el número entero).
        //
        // Direccion: calculada con el CuentaId pasado en el contexto de mapeo.
        //   Si la cuenta consultada es la ORIGEN → es un Egreso (dinero que sale).
        //   Si la cuenta consultada es la DESTINO → es un Ingreso (dinero que entra).
        //   Cualquier otro caso (débito/depósito directo) → se toma por el tipo.
        //
        // Uso desde el Use Case:
        //   _mapper.Map<TransaccionDto>(entidad, opts =>
        //       opts.Items[MappingProfile.CuentaIdContextKey] = cuentaId);
        CreateMap<Transaccion, TransaccionDto>()
            .ForMember(
                dest => dest.TipoMovimiento,
                opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(
                dest => dest.Direccion,
                opt => opt.MapFrom((src, _, _, ctx) =>
                {
                    if (!ctx.Items.TryGetValue(CuentaIdContextKey, out var raw)
                        || raw is not Guid cuentaId)
                        return src.Tipo.ToString();

                    // Crédito explícito (destino de una transferencia)
                    if (src.CuentaDestinoId == cuentaId)
                        return "Ingreso";

                    // Débito: la cuenta consultada es la que originó el movimiento
                    if (src.CuentaOrigenId == cuentaId)
                        return "Egreso";

                    return src.Tipo.ToString();
                }));
    }
}
