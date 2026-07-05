using System.Security.Claims;

namespace backend.Contexts.CurrentUser;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public int? UserId =>
        GetIntClaim(
            ClaimTypes.NameIdentifier,
            "sub",
            "userId");

    public string? UserName =>
        GetStringClaim(
            ClaimTypes.Name,
            "name");

    public string? Email =>
        GetStringClaim(
            ClaimTypes.Email,
            "email");

    public IReadOnlyList<string> Roles =>
        User?
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Concat(User.FindAll("role").Select(claim => claim.Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
        ?? [];

    public bool IsImpersonating =>
        GetBoolClaim("is_impersonating");

    public int? ImpersonatedByUserId =>
        GetIntClaim("impersonated_by_user_id");

    public string? ImpersonatedByUserName =>
        GetStringClaim("impersonated_by_user_name");

    public int GetRequiredUserId()
    {
        if (UserId is null)
        {
            throw new UnauthorizedAccessException("User id claim is missing.");
        }

        return UserId.Value;
    }

    public bool IsInRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    private string? GetStringClaim(params string[] claimTypes)
    {
        if (User is null)
        {
            return null;
        }

        foreach (var claimType in claimTypes)
        {
            var value = User.FindFirstValue(claimType);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private int? GetIntClaim(params string[] claimTypes)
    {
        var value = GetStringClaim(claimTypes);

        return int.TryParse(value, out var result)
            ? result
            : null;
    }

    private bool GetBoolClaim(params string[] claimTypes)
    {
        var value = GetStringClaim(claimTypes);

        return bool.TryParse(value, out var result) && result;
    }
}