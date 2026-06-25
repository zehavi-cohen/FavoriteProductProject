using System.Security.Claims;

namespace backend.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            throw new UnauthorizedAccessException("User id was not found in token.");
        }

        if (!int.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user id in token.");
        }

        return userId;
    }
}