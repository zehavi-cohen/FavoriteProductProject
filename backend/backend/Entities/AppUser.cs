namespace backend.Entities;

public class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();

    public ICollection<UserFavoriteProduct> FavoriteProducts { get; set; } = new List<UserFavoriteProduct>();
}