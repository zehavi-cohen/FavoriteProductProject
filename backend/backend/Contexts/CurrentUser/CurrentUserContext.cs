namespace backend.Contexts.CurrentUser;

public sealed class CurrentUserContext : ICurrentUserContext
{
    public bool IsAuthenticated { get; private set; }

    public int? UserId { get; private set; }

    public string? UserName { get; private set; }

    public string? Email { get; private set; }

    public IReadOnlyList<string> Roles { get; private set; } = [];

    public bool IsImpersonating { get; private set; }

    public int? ImpersonatedByUserId { get; private set; }

    public string? ImpersonatedByUserName { get; private set; }

    public void Set(
        int? userId,
        string? userName,
        string? email,
        IReadOnlyList<string> roles,
        bool isImpersonating,
        int? impersonatedByUserId,
        string? impersonatedByUserName)
    {
        IsAuthenticated = userId.HasValue;
        UserId = userId;
        UserName = userName;
        Email = email;
        Roles = roles;
        IsImpersonating = isImpersonating;
        ImpersonatedByUserId = impersonatedByUserId;
        ImpersonatedByUserName = impersonatedByUserName;
    }

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
}