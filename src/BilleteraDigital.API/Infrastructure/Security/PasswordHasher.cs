using System.Security.Cryptography;
using System.Text;
using BilleteraDigital.Application.Ports.Services;

namespace BilleteraDigital.API.Infrastructure.Security;

/// <summary>
/// Adaptador de salida: implementa IPasswordHasher usando HMACSHA256 con salt embebido.
/// El salt de 16 bytes aleatorios se almacena como prefijo del hash (Base64).
/// Formato almacenado: Base64( salt[16] + hmac[32] )
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    // Clave derivada de la configuración es opcional; usamos HMAC con salt por usuario.
    // Para mayor seguridad en producción considerar BCrypt / Argon2.
    private const int SaltBytes = 16;

    public string Hashear(string passwordPlano)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordPlano);

        // Generar salt aleatorio
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);

        // Calcular HMAC-SHA256 usando el salt como clave
        using var hmac = new HMACSHA256(salt);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(passwordPlano));

        // Concatenar salt + hash y codificar en Base64
        var resultado = new byte[SaltBytes + hash.Length];
        Buffer.BlockCopy(salt, 0, resultado, 0, SaltBytes);
        Buffer.BlockCopy(hash, 0, resultado, SaltBytes, hash.Length);

        return Convert.ToBase64String(resultado);
    }

    public bool Verificar(string passwordPlano, string hashGuardado)
    {
        if (string.IsNullOrWhiteSpace(passwordPlano) || string.IsNullOrWhiteSpace(hashGuardado))
            return false;

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(hashGuardado);
        }
        catch (FormatException)
        {
            return false;
        }

        if (bytes.Length < SaltBytes + 1)
            return false;

        // Extraer salt
        var salt = new byte[SaltBytes];
        Buffer.BlockCopy(bytes, 0, salt, 0, SaltBytes);

        // Recalcular HMAC con el mismo salt
        using var hmac = new HMACSHA256(salt);
        var hashCalculado = hmac.ComputeHash(Encoding.UTF8.GetBytes(passwordPlano));

        // Comparar en tiempo constante para evitar timing attacks
        var hashOriginal = new byte[bytes.Length - SaltBytes];
        Buffer.BlockCopy(bytes, SaltBytes, hashOriginal, 0, hashOriginal.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashOriginal);
    }
}
