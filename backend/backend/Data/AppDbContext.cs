using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<UserFavoriteProduct> UserFavoriteProducts => Set<UserFavoriteProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUsers");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.HasIndex(x => x.UserName)
                .IsUnique();

            entity.HasIndex(x => x.Email)
                .IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });

        modelBuilder.Entity<AppUserRole>(entity =>
        {
            entity.ToTable("AppUserRoles");

            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.UserId);

            entity.HasOne(x => x.Role)
                .WithMany(x => x.UserRoles)
                .HasForeignKey(x => x.RoleId);

            entity.HasIndex(x => new { x.UserId, x.RoleId })
                .IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<UserFavoriteProduct>(entity =>
        {
            entity.ToTable("UserFavoriteProducts");

            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.User)
                .WithMany(x => x.FavoriteProducts)
                .HasForeignKey(x => x.UserId);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.FavoriteUsers)
                .HasForeignKey(x => x.ProductId);

            entity.HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();
        });
    }
}