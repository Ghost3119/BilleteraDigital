namespace BilleteraDigital.Application.Common;

/// <summary>
/// Parámetros de paginación reutilizables.
/// Se puede recibir como [FromQuery] en cualquier endpoint de listado.
/// </summary>
public sealed class PaginationParams
{
    private const int MaxPageSize = 50;

    private int _pageSize = 10;
    private int _pageNumber = 1;

    /// <summary>Número de página solicitada (mínimo 1).</summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>Tamaño de página (entre 1 y 50; por defecto 10).</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : value > MaxPageSize ? MaxPageSize : value;
    }
}
