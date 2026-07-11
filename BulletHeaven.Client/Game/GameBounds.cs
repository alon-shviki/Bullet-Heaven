namespace BulletHeaven.Client.Game;

public static class GameBounds
{
    public static double Width { get; set; } = 800;
    public static double Height { get; set; } = 600;
    public static double CenterX => Width / 2;
    public static double CenterY => Height / 2;
}
