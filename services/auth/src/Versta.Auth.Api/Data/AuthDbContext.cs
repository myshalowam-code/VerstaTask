using Microsoft.EntityFrameworkCore;
using Versta.Auth.Api.Models;

namespace Versta.Auth.Api.Data;

public sealed class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();
        entity.ToTable("users");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.Email).IsUnique();
        entity.Property(x => x.Email).HasMaxLength(320).IsRequired();
        entity.Property(x => x.PasswordHash).IsRequired();
    }
}
