using System.Security.Claims;
using BulletHeaven.Server.Data;
using BulletHeaven.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulletHeaven.Server.Controllers;

[ApiController]
[Route("api/scores")]
[Authorize]
public class ScoresController(AppDbContext db) : ControllerBase
{
    // Sanity ceilings: far beyond anything a legitimate run can produce,
    // but they stop trivially spoofed int.MaxValue payloads from sticking
    // to the top of the leaderboard forever.
    private const int MaxValue = 100_000_000;
    private const int MaxKills = 1_000_000;
    private const int MaxLevel = 10_000;

    public record ScoreRequest(int Value, int Kills, int Level);
    public record PersonalBestEntry(int Value, int Kills, int Level, DateTime PlayedAt);

    [HttpPost]
    public async Task<IActionResult> Submit(ScoreRequest req)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        if (req.Value < 0 || req.Kills < 0 || req.Level < 1 ||
            req.Value > MaxValue || req.Kills > MaxKills || req.Level > MaxLevel)
            return BadRequest("Invalid score data.");

        db.Scores.Add(new Score
        {
            UserId   = userId,
            Value    = req.Value,
            Kills    = req.Kills,
            Level    = req.Level,
            PlayedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return Ok();
    }

    /// <summary>Top 5 runs of the authenticated player — no usernames/ids in the payload.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBest()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var entries = await db.Scores
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.Value)
            .Take(5)
            .Select(s => new PersonalBestEntry(s.Value, s.Kills, s.Level, s.PlayedAt))
            .ToListAsync();

        return Ok(entries);
    }
}
