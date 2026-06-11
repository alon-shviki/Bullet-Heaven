using BulletHeaven.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulletHeaven.Server.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController(AppDbContext db) : ControllerBase
{
    record LeaderboardEntry(string Username, int Value, int Kills, int Level, DateTime PlayedAt);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var entries = await db.Scores
            .AsNoTracking()
            .OrderByDescending(s => s.Value)
            .Take(10)
            .Select(s => new LeaderboardEntry(s.User.Username, s.Value, s.Kills, s.Level, s.PlayedAt))
            .ToListAsync();

        return Ok(entries);
    }
}
