namespace backend.Entities;

public class AppUserRole
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public AppUser User { get; set; } = null!;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}