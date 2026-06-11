using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BulletHeaven.Server.Data;
using BulletHeaven.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BulletHeaven.Server.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
{
    public record AuthRequest(string Username, string Password);
    public record AuthResponse(string Token);

    [HttpPost("register")]
    public async Task<IActionResult> Register(AuthRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || req.Username.Length > 32)
            return BadRequest("Username must be 1-32 characters.");

        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters.");

        if (await db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict("Username already taken.");

        var user = new User
        {
            Username    = req.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            CreatedAt   = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(new AuthResponse(BuildToken(user, config)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AuthRequest req)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        return Ok(new AuthResponse(BuildToken(user, config)));
    }

    private static string BuildToken(User user, IConfiguration config)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
        };
        var token = new JwtSecurityToken(
            issuer:   "BulletHeaven",
            audience: "BulletHeaven",
            claims:   claims,
            expires:  DateTime.UtcNow.AddDays(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
