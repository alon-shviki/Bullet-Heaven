using Microsoft.JSInterop;

namespace BulletHeaven.Client.Game.Input;

public class InputHandler(IJSRuntime js)
{
    public async Task InitAsync() =>
        await js.InvokeVoidAsync("gameInterop.initInput");

    public async Task<(double Vx, double Vy, bool SpacePressed, bool RPressed)> GetInputAsync()
    {
        var v = await js.InvokeAsync<double[]>("gameInterop.getMovementVector");
        return (v[0], v[1], v[2] != 0, v[3] != 0);
    }
}
