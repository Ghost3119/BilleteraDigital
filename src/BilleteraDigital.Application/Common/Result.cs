namespace BilleteraDigital.Application.Common;

/// <summary>
/// Resultado genérico de un caso de uso. Encapsula éxito o error sin lanzar excepciones al llamador.
/// </summary>
public sealed class Result<T>
{
    public bool EsExitoso { get; }
    public T? Valor { get; }
    public string? Error { get; }

    private Result(bool esExitoso, T? valor, string? error)
    {
        EsExitoso = esExitoso;
        Valor = valor;
        Error = error;
    }

    public static Result<T> Exitoso(T valor) => new(true, valor, null);
    public static Result<T> Fallido(string error) => new(false, default, error);
}

/// <summary>Resultado sin valor de retorno.</summary>
public sealed class Result
{
    public bool EsExitoso { get; }
    public string? Error { get; }

    private Result(bool esExitoso, string? error)
    {
        EsExitoso = esExitoso;
        Error = error;
    }

    public static Result Exitoso() => new(true, null);
    public static Result Fallido(string error) => new(false, error);
}
