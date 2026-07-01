namespace backend.Contexts.CurrentUser;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    string? UserName { get; }

    string? Email { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsImpersonating { get; }

    int? ImpersonatedByUserId { get; }

    string? ImpersonatedByUserName { get; }

    int GetRequiredUserId();

    bool IsInRole(string role);
}