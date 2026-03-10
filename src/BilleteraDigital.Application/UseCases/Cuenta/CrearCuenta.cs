using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Crear una nueva cuenta bancaria para el usuario autenticado.
/// <para>
/// Responsabilidades:
/// <list type="bullet">
///   <item>Verificar que el usuario existe.</item>
///   <item>Usar el <c>Nombre</c> del usuario como <c>NombreTitular</c> — nunca datos del cliente.</item>
///   <item>Persistir la cuenta; SQL Server asigna el <c>NumeroCuenta</c> automáticamente
///         mediante la secuencia <c>CuentaNumbers</c>. EF Core lo lee de vuelta tras
///         <c>SaveChanges</c>.</item>
/// </list>
/// </para>
/// </summary>
public sealed class CrearCuenta
{
    private readonly ICuentaRepository  _cuentaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IUnitOfWork        _unitOfWork;

    public CrearCuenta(
        ICuentaRepository  cuentaRepository,
        IUsuarioRepository usuarioRepository,
        IUnitOfWork        unitOfWork)
    {
        _cuentaRepository  = cuentaRepository;
        _usuarioRepository = usuarioRepository;
        _unitOfWork        = unitOfWork;
    }

    /// <summary>
    /// Ejecuta la creación de cuenta para el usuario identificado por <paramref name="command"/>.
    /// </summary>
    public async Task<Result<CuentaResponse>> EjecutarAsync(
        CrearCuentaCommand command,
        CancellationToken  cancellationToken = default)
    {
        // 1. Resolver el usuario desde la base de datos
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(command.UsuarioId, cancellationToken);
        if (usuario is null)
            return Result<CuentaResponse>.Fallido("El usuario autenticado no existe en el sistema.");

        try
        {
            // 2. Crear la entidad de dominio.
            //    NumeroCuenta será asignado por la secuencia SQL Server tras SaveChanges.
            //    Saldo inicial = 0 siempre (fijado en el constructor de dominio).
            var cuenta = new Domain.Entities.Cuenta(
                usuario.Nombre,
                usuario.Id);

            await _cuentaRepository.CrearAsync(cuenta, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

            // 3. Tras SaveChanges EF Core ha poblado NumeroCuenta con el valor de la secuencia.
            return Result<CuentaResponse>.Exitoso(new CuentaResponse(
                cuenta.Id,
                cuenta.NumeroCuenta,
                cuenta.NombreTitular,
                cuenta.Saldo,
                cuenta.Estado,
                cuenta.FechaCreacion,
                cuenta.FechaUltimaOperacion));
        }
        catch (Exception ex)
        {
            return Result<CuentaResponse>.Fallido(ex.Message);
        }
    }
}
