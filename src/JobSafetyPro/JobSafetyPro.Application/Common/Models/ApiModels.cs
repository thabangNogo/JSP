namespace JobSafetyPro.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPrevious => Page > 1;

    public bool HasNext => Page < TotalPages;
}

public class ApiResponse<T>
{
    public bool Success { get; init; } = true;

    public T? Data { get; init; }

    public string? Message { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new() { Data = data, Message = message };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}
