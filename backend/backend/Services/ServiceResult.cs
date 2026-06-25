namespace backend.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }

    public T? Data { get; private set; }

    public string? ErrorMessage { get; private set; }

    public int StatusCode { get; private set; }

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = StatusCodes.Status200OK
        };
    }

    public static ServiceResult<T> BadRequest(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    public static ServiceResult<T> Unauthorized(string message = "Unauthorized.")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = StatusCodes.Status401Unauthorized
        };
    }

    public static ServiceResult<T> NotFound(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = StatusCodes.Status404NotFound
        };
    }

    public static ServiceResult<T> Problem(string message)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = StatusCodes.Status500InternalServerError
        };
    }
}