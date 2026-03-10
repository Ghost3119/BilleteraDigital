using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Crear una nueva cuenta en el sistema.
/// </summary>
public sealed class CrearCuenta
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearCuenta(ICuentaRepository cuentaRepository, IUnitOfWork unitOfWork)
    {
        _cuentaRepository = cuentaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CuentaResponse>> EjecutarAsync(
        CrearCuentaRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verificar unicidad del número de cuenta
        var cuentaExistente = await _cuentaRepository.ObtenerPorNumeroAsync(request.NumeroCuenta, cancellationToken);
        if (cuentaExistente is not null)
            return Result<CuentaResponse>.Fallido($"Ya existe una cuenta con el número '{request.NumeroCuenta}'.");

        try
        {
            var cuenta = new Domain.Entities.Cuenta(
                request.NumeroCuenta,
                request.NombreTitular,
                request.SaldoInicial);

            await _cuentaRepository.CrearAsync(cuenta, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            return Result<CuentaResponse>.Exitoso(new CuentaResponse(
                cuenta.Id,
                cuenta.NumeroCuenta,
                cuenta.NombreTitular,
                cuenta.Saldo,
                cuenta.Estado,
                cuenta.FechaCreacion,
                cuenta.FechaUltimaOperacion
            ));
        }
        catch (Exception ex)
        {
            return Result<CuentaResponse>.Fallido(ex.Message);
        }
    }
}
