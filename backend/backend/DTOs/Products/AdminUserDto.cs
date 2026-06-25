namespace backend.DTOs.Admin;

public class AdminUserDto
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public List<string> Roles { get; set; } = new();
}