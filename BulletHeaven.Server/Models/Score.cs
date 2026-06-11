namespace BulletHeaven.Server.Models;

public class Score
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int Value { get; set; }
    public int Kills { get; set; }
    public int Level { get; set; }
    public DateTime PlayedAt { get; set; }
}
