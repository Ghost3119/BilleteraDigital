using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Devuelve una página de cuentas que pertenecen al usuario autenticado.
/// Cumple con la regla de arquitectura: toda colección se devuelve paginada.
/// </summary>
public sealed class ConsultarMisCuentas
{
    private readonly ICuentaRepository _cuentaRepository;

    public ConsultarMisCuentas(ICuentaRepository cuentaRepository)
    {
        _cuentaRepository = cuentaRepository;
    }

    public async Task<Result<PagedResult<CuentaResponse>>> EjecutarAsync(
        Guid usuarioId,
        GenericQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var pagina = await _cuentaRepository.ObtenerPaginadoPorUsuarioIdAsync(
            usuarioId, queryParams, cancellationToken);

        var respuestas = pagina.Items.Select(c => new CuentaResponse(
            c.Id,
            c.NumeroCuenta,
            c.NombreTitular,
            c.Saldo,
            c.Estado,
            c.FechaCreacion,
            c.FechaUltimaOperacion));

        var paginaDto = new PagedResult<CuentaResponse>(
            respuestas,
            pagina.TotalCount,
            pagina.PageNumber,
            pagina.PageSize);

        return Result<PagedResult<CuentaResponse>>.Exitoso(paginaDto);
    }
}
