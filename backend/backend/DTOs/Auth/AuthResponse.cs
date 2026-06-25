namespace backend.DTOs.Auth;

public class AuthResponse
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();

    public string Token { get; set; } = string.Empty;
}