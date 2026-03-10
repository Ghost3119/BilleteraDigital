using AutoMapper;
using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Mappings;
using BilleteraDigital.Application.Ports.Repositories;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Obtener el historial filtrado y paginado de transacciones de una cuenta.
/// </summary>
public sealed class ObtenerHistorialTransacciones
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly ITransaccionRepository _transaccionRepository;
    private readonly IMapper _mapper;

    public ObtenerHistorialTransacciones(
        ICuentaRepository cuentaRepository,
        ITransaccionRepository transaccionRepository,
        IMapper mapper)
    {
        _cuentaRepository = cuentaRepository;
        _transaccionRepository = transaccionRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<TransaccionDto>>> EjecutarAsync(
        Guid cuentaId,
        GenericQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var existe = await _cuentaRepository.ExisteAsync(cuentaId, cancellationToken);
        if (!existe)
            return Result<PagedResult<TransaccionDto>>.Fallido($"Cuenta '{cuentaId}' no encontrada.");

        var paginaEntidades = await _transaccionRepository.ObtenerPorCuentaFiltradoAsync(
            cuentaId, queryParams, cancellationToken);

        // Mapear cada entidad pasando el CuentaId como contexto para que el perfil
        // pueda calcular Direccion (Ingreso / Egreso) sin romper la encapsulación del dominio.
        var dtos = paginaEntidades.Items
            .Select(t => _mapper.Map<TransaccionDto>(t, opts =>
                opts.Items[MappingProfile.CuentaIdContextKey] = cuentaId));

        var paginaDto = new PagedResult<TransaccionDto>(
            dtos,
            paginaEntidades.TotalCount,
            paginaEntidades.PageNumber,
            paginaEntidades.PageSize);

        return Result<PagedResult<TransaccionDto>>.Exitoso(paginaDto);
    }
}
