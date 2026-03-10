using BilleteraDigital.Application.Common;
using BilleteraDigital.Application.Ports.Repositories;
using BilleteraDigital.Application.Ports.Services;
using BilleteraDigital.Domain.Entities;

namespace BilleteraDigital.Application.UseCases.Usuario;

/// <summary>
/// Caso de uso: Registrar un nuevo usuario en el sistema.
/// Orquesta la validación, el hashing de contraseña y la persistencia.
/// </summary>
public sealed class RegistrarUsuario
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher    _passwordHasher;
    private readonly IUnitOfWork        _unitOfWork;

    public RegistrarUsuario(
        IUsuarioRepository usuarioRepository,
        IPasswordHasher    passwordHasher,
        IUnitOfWork        unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher    = passwordHasher;
        _unitOfWork        = unitOfWork;
    }

    /// <summary>Ejecuta el registro de un nuevo usuario.</summary>
    public async Task<Result<UsuarioRegistradoResponse>> EjecutarAsync(
        RegistrarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validación básica de entrada
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<UsuarioRegistradoResponse>.Fallido("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return Result<UsuarioRegistradoResponse>.Fallido("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return Result<UsuarioRegistradoResponse>.Fallido("La contraseña debe tener al menos 8 caracteres.");

        // Verificar que el email no esté ya registrado
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var existe = await _usuarioRepository.ExistePorEmailAsync(emailNormalizado, cancellationToken);
        if (existe)
            return Result<UsuarioRegistradoResponse>.Fallido($"Ya existe un usuario con el email '{emailNormalizado}'.");

        // Hashear contraseña — nunca se persiste el texto plano
        var hash = _passwordHasher.Hashear(request.Password);

        // Crear entidad de dominio
        var usuario = new Domain.Entities.Usuario(request.Nombre.Trim(), emailNormalizado, hash);

        // Persistir
        await _usuarioRepository.CrearAsync(usuario, cancellationToken);
        await _unitOfWork.GuardarCambiosAsync(cancellationToken);

        var respuesta = new UsuarioRegistradoResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.FechaRegistro);

        return Result<UsuarioRegistradoResponse>.Exitoso(respuesta);
    }
}
