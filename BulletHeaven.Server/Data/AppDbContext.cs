using BulletHeaven.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BulletHeaven.Server.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Score> Scores => Set<Score>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>()
          .HasIndex(u => u.Username)
          .IsUnique();

        mb.Entity<Score>()
          .HasOne(s => s.User)
          .WithMany(u => u.Scores)
          .HasForeignKey(s => s.UserId);
    }
}
