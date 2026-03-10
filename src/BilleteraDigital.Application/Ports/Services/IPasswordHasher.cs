namespace BilleteraDigital.Application.Ports.Services;

/// <summary>
/// Puerto de salida (driven port) para el servicio de hashing de contraseñas.
/// La implementación concreta vive en la infraestructura.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Genera el hash de una contraseña en texto plano.</summary>
    string Hashear(string passwordPlano);

    /// <summary>Verifica que una contraseña en texto plano coincida con su hash.</summary>
    bool Verificar(string passwordPlano, string hash);
}
