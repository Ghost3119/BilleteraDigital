using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.Ports.Repositories;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Obtener el historial filtrado y paginado de transacciones de una cuenta.
/// </summary>
public sealed class ObtenerHistorialTransacciones
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly ITransaccionRepository _transaccionRepository;

    public ObtenerHistorialTransacciones(
        ICuentaRepository cuentaRepository,
        ITransaccionRepository transaccionRepository)
    {
        _cuentaRepository = cuentaRepository;
        _transaccionRepository = transaccionRepository;
    }

    public async Task<Result<PagedResult<TransaccionResponse>>> EjecutarAsync(
        Guid cuentaId,
        GenericQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var existe = await _cuentaRepository.ExisteAsync(cuentaId, cancellationToken);
        if (!existe)
            return Result<PagedResult<TransaccionResponse>>.Fallido($"Cuenta '{cuentaId}' no encontrada.");

        var paginaEntidades = await _transaccionRepository.ObtenerPorCuentaFiltradoAsync(
            cuentaId, queryParams, cancellationToken);

        var paginaDto = new PagedResult<TransaccionResponse>(
            paginaEntidades.Items.Select(t => new TransaccionResponse(
                t.Id,
                t.Tipo,
                t.Monto,
                t.SaldoResultante,
                t.Descripcion,
                t.FechaHora)),
            paginaEntidades.TotalCount,
            paginaEntidades.PageNumber,
            paginaEntidades.PageSize);

        return Result<PagedResult<TransaccionResponse>>.Exitoso(paginaDto);
    }
}
