using backend.Services;

namespace backend.Extensions;

public static class EndpointResultExtensions
{
    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Data);
        }

        return result.StatusCode switch
        {
            StatusCodes.Status400BadRequest => Results.BadRequest(result.ErrorMessage),
            StatusCodes.Status401Unauthorized => Results.Unauthorized(),
            StatusCodes.Status404NotFound => Results.NotFound(result.ErrorMessage),
            _ => Results.Problem(result.ErrorMessage)
        };
    }
}