using System.Security.Cryptography;
using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.DTOs;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;

namespace BilleteraDigital.Application.UseCases.Cuenta;

/// <summary>
/// Caso de uso: Crear una nueva cuenta bancaria para el usuario autenticado.
/// <para>
/// Responsabilidades:
/// <list type="bullet">
///   <item>Verificar que el usuario existe.</item>
///   <item>Generar un <c>NumeroCuenta</c> único de 10 dígitos.</item>
///   <item>Usar el <c>Nombre</c> del usuario como <c>NombreTitular</c> — nunca datos del cliente.</item>
///   <item>Persistir la cuenta ya asociada al <c>UsuarioId</c>.</item>
/// </list>
/// </para>
/// </summary>
public sealed class CrearCuenta
{
    /// <summary>
    /// Máximo de intentos para generar un NumeroCuenta que no colisione con uno existente.
    /// En la práctica con 10 dígitos (10^10 combinaciones) la primera iteración siempre
    /// debería ser suficiente, pero el guardrail protege contra escenarios extremos.
    /// </summary>
    private const int MaxIntentosNumeroCuenta = 5;

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

        // 2. Validar saldo inicial
        if (command.SaldoInicial < 0)
            return Result<CuentaResponse>.Fallido("El saldo inicial no puede ser negativo.");

        // 3. Generar un NumeroCuenta único de 10 dígitos
        var numeroCuenta = await GenerarNumeroCuentaUnicoAsync(cancellationToken);
        if (numeroCuenta is null)
            return Result<CuentaResponse>.Fallido(
                "No se pudo generar un número de cuenta único. Intente de nuevo.");

        try
        {
            // 4. Crear la entidad de dominio
            //    NombreTitular = nombre real del usuario; nunca proviene del cliente.
            var cuenta = new Domain.Entities.Cuenta(
                numeroCuenta,
                usuario.Nombre,
                usuario.Id,
                command.SaldoInicial);

            await _cuentaRepository.CrearAsync(cuenta, cancellationToken);
            await _unitOfWork.GuardarCambiosAsync(cancellationToken);

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

    // ── Helpers privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Genera un número de cuenta de 10 dígitos que no exista en la base de datos.
    /// Devuelve <c>null</c> si se agota el número máximo de intentos.
    /// </summary>
    private async Task<string?> GenerarNumeroCuentaUnicoAsync(CancellationToken cancellationToken)
    {
        for (var intento = 0; intento < MaxIntentosNumeroCuenta; intento++)
        {
            var candidato = GenerarNumeroCuenta10Digitos();
            var existe    = await _cuentaRepository.ObtenerPorNumeroAsync(candidato, cancellationToken);
            if (existe is null)
                return candidato;
        }
        return null;
    }

    /// <summary>
    /// Genera una cadena numérica aleatoria de exactamente 10 dígitos usando
    /// <see cref="RandomNumberGenerator"/> (CSPRNG) para evitar colisiones predecibles.
    /// El primer dígito nunca es 0 para garantizar formato uniforme.
    /// </summary>
    private static string GenerarNumeroCuenta10Digitos()
    {
        // Primer dígito: 1-9 (nunca 0 para evitar números con cero líder)
        var primerDigito = RandomNumberGenerator.GetInt32(1, 10);

        // Dígitos restantes: 0-9
        Span<char> digitos = stackalloc char[10];
        digitos[0] = (char)('0' + primerDigito);
        for (var i = 1; i < 10; i++)
            digitos[i] = (char)('0' + RandomNumberGenerator.GetInt32(0, 10));

        return new string(digitos);
    }
}
