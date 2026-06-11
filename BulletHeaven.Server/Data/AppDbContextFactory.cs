using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BulletHeaven.Server.Data;

// Used only by dotnet-ef tooling (migrations) — not loaded at runtime.
// Set ConnectionStrings__Default in your environment before running dotnet-ef commands.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? throw new InvalidOperationException(
                "Set the ConnectionStrings__Default environment variable before running dotnet-ef commands.\n" +
                "Example: export ConnectionStrings__Default=\"Host=localhost;Database=bulletheaven;Username=bh_user;Password=bh_pass\"");

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connStr)
            .Options;
        return new AppDbContext(opts);
    }
}
