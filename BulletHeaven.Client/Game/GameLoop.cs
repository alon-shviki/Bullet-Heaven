namespace BulletHeaven.Client.Game;

public class GameLoop
{
    private double _lastTimestamp;
    private bool _started;

    public double DeltaTime { get; private set; }
    public double Fps { get; private set; }
    public bool IsPaused { get; set; }

    public void ProcessTick(double timestamp)
    {
        if (!_started)
        {
            _lastTimestamp = timestamp;
            _started = true;
            return;
        }

        DeltaTime = (timestamp - _lastTimestamp) / 1000.0;
        _lastTimestamp = timestamp;

        if (DeltaTime > 0)
            Fps = 1.0 / DeltaTime;

    }
}
