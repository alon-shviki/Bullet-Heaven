using BulletHeaven.Client.Game.Entities;

namespace BulletHeaven.Client.Pages;

public partial class Game
{
    // ── PLAYER ──────────────────────────────────────────────────────────────
    private async Task DrawPlayer(double x, double y, double angle)
    {
        await _ctx.SaveAsync();
        await _ctx.TranslateAsync(x, y);
        await _ctx.RotateAsync((float)angle);

        // engine flame (behind hull)
        await _ctx.SetFillStyleAsync("#ffd23d");
        await _ctx.BeginPathAsync();
        await _ctx.MoveToAsync(-9, -5.5);
        await _ctx.LineToAsync(-18, 0);
        await _ctx.LineToAsync(-9, 5.5);
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();

        // hull — sleek dart with rear notch
        await _ctx.SetShadowBlurAsync(14f);
        await _ctx.SetShadowColorAsync("#23e0ff");
        await _ctx.SetFillStyleAsync("#1fd6c4");
        await _ctx.BeginPathAsync();
        await _ctx.MoveToAsync(18, 0);
        await _ctx.LineToAsync(-10, -13);
        await _ctx.LineToAsync(-4, 0);
        await _ctx.LineToAsync(-10, 13);
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();
        await _ctx.SetShadowBlurAsync(0f);
        await _ctx.SetLineWidthAsync(2);
        await _ctx.SetStrokeStyleAsync("#073f3a");
        await _ctx.StrokeAsync();

        // canopy
        await _ctx.SetFillStyleAsync("#eafffb");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(2.5, 0, 4, 0, 2 * Math.PI);
        await _ctx.FillAsync();
        await _ctx.SetLineWidthAsync(1.4f);
        await _ctx.SetStrokeStyleAsync("#073f3a");
        await _ctx.StrokeAsync();

        await _ctx.RestoreAsync();
    }

    // ── STANDARD ENEMY ──────────────────────────────────────────────────────
    private async Task DrawStandard(double x, double y)
    {
        const double r = 12;

        await _ctx.SetFillStyleAsync("#8a5cd0");
        await _ctx.BeginPathAsync();
        for (int i = 0; i < 6; i++)
        {
            double a = Math.PI / 6 + i * Math.PI / 3;
            double px = x + r * Math.Cos(a);
            double py = y + r * Math.Sin(a);
            if (i == 0) await _ctx.MoveToAsync(px, py);
            else await _ctx.LineToAsync(px, py);
        }
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();
        await _ctx.SetLineWidthAsync(2);
        await _ctx.SetStrokeStyleAsync("#371c5e");
        await _ctx.StrokeAsync();

        // eye socket
        await _ctx.SetFillStyleAsync("#1d0f33");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 4.6, 0, 2 * Math.PI);
        await _ctx.FillAsync();

        // pupil
        await _ctx.SetFillStyleAsync("#ff5a7a");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 2.1, 0, 2 * Math.PI);
        await _ctx.FillAsync();
    }

    // ── RUNNER ENEMY ────────────────────────────────────────────────────────
    private async Task DrawRunner(double x, double y)
    {
        const double outer = 8, inner = 3;

        await _ctx.SetFillStyleAsync("#f5a623");
        await _ctx.BeginPathAsync();
        for (int k = 0; k < 8; k++)
        {
            double r = (k % 2 == 0) ? outer : inner;
            double a = -Math.PI / 2 + k * Math.PI / 4;
            double px = x + r * Math.Cos(a);
            double py = y + r * Math.Sin(a);
            if (k == 0) await _ctx.MoveToAsync(px, py);
            else await _ctx.LineToAsync(px, py);
        }
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();
        await _ctx.SetLineWidthAsync(1.6f);
        await _ctx.SetStrokeStyleAsync("#6e3d00");
        await _ctx.StrokeAsync();

        // hot core
        await _ctx.SetFillStyleAsync("#fff0c2");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 2, 0, 2 * Math.PI);
        await _ctx.FillAsync();
    }

    // ── TANK ENEMY ──────────────────────────────────────────────────────────
    private async Task DrawTank(double x, double y)
    {
        const double r = 20, ri = 12.5;

        // outer armored octagon
        await _ctx.SetFillStyleAsync("#5d7a6b");
        await _ctx.BeginPathAsync();
        for (int i = 0; i < 8; i++)
        {
            double a = Math.PI / 8 + i * Math.PI / 4;
            double px = x + r * Math.Cos(a);
            double py = y + r * Math.Sin(a);
            if (i == 0) await _ctx.MoveToAsync(px, py);
            else await _ctx.LineToAsync(px, py);
        }
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();
        await _ctx.SetLineWidthAsync(3);
        await _ctx.SetStrokeStyleAsync("#1f2a25");
        await _ctx.StrokeAsync();

        // inner plate ring
        await _ctx.BeginPathAsync();
        for (int i = 0; i < 8; i++)
        {
            double a = Math.PI / 8 + i * Math.PI / 4;
            double px = x + ri * Math.Cos(a);
            double py = y + ri * Math.Sin(a);
            if (i == 0) await _ctx.MoveToAsync(px, py);
            else await _ctx.LineToAsync(px, py);
        }
        await _ctx.ClosePathAsync();
        await _ctx.SetLineWidthAsync(2);
        await _ctx.SetStrokeStyleAsync("#3c5249");
        await _ctx.StrokeAsync();

        // corner bolts
        await _ctx.SetFillStyleAsync("#cdd6cf");
        for (int i = 0; i < 4; i++)
        {
            double a = Math.PI / 4 + i * Math.PI / 2;
            double bx = x + 15 * Math.Cos(a);
            double by = y + 15 * Math.Sin(a);
            await _ctx.BeginPathAsync();
            await _ctx.ArcAsync(bx, by, 1.9, 0, 2 * Math.PI);
            await _ctx.FillAsync();
        }

        // central vision slit
        await _ctx.SetFillStyleAsync("#10241c");
        await _ctx.FillRectAsync(x - 7, y - 2.2, 14, 4.4);
    }

    // ── ELITE ENEMY ─────────────────────────────────────────────────────────
    private async Task DrawElite(double x, double y)
    {
        const double outer = 16, inner = 8;

        // glow ring
        await _ctx.SetShadowBlurAsync(16f);
        await _ctx.SetShadowColorAsync("#ff4f9d");
        await _ctx.SetLineWidthAsync(2);
        await _ctx.SetStrokeStyleAsync("#ff4f9d");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 18.5, 0, 2 * Math.PI);
        await _ctx.StrokeAsync();
        await _ctx.SetShadowBlurAsync(0f);

        // 6-point spiked star
        await _ctx.SetFillStyleAsync("#e0457b");
        await _ctx.BeginPathAsync();
        for (int k = 0; k < 12; k++)
        {
            double r = (k % 2 == 0) ? outer : inner;
            double a = -Math.PI / 2 + k * Math.PI / 6;
            double px = x + r * Math.Cos(a);
            double py = y + r * Math.Sin(a);
            if (k == 0) await _ctx.MoveToAsync(px, py);
            else await _ctx.LineToAsync(px, py);
        }
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();
        await _ctx.SetLineWidthAsync(2);
        await _ctx.SetStrokeStyleAsync("#530f30");
        await _ctx.StrokeAsync();

        // glowing core
        await _ctx.SetShadowBlurAsync(10f);
        await _ctx.SetShadowColorAsync("#ff4f9d");
        await _ctx.SetFillStyleAsync("#ffd9e8");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 5, 0, 2 * Math.PI);
        await _ctx.FillAsync();
        await _ctx.SetShadowBlurAsync(0f);
    }

    // ── BOSS ────────────────────────────────────────────────────────────────
    private async Task DrawBoss(double x, double y)
    {
        const double outer = 35, inner = 22;

        // outer aura ring
        await _ctx.SetShadowBlurAsync(26f);
        await _ctx.SetShadowColorAsync("#ff6a2c");
        await _ctx.SetLineWidthAsync(3);
        await _ctx.SetStrokeStyleAsync("#ff6a2c");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 39, 0, 2 * Math.PI);
        await _ctx.StrokeAsync();
        await _ctx.SetShadowBlurAsync(0f);

        // crown — 10-point star body
        await _ctx.SetFillStyleAsync("#c11f3a");
        await _ctx.BeginPathAsync();
        for (int k = 0; k < 20; k++)
        {
            double r = (k % 2 == 0) ? outer : inner;
            double a = -Math.PI / 2 + k * Math.PI / 10;
            double px = x + r * Math.Cos(a);
            double py = y + r * Math.Sin(a);
            if (k == 0) await _ctx.MoveToAsync(px, py);
            else await _ctx.LineToAsync(px, py);
        }
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();
        await _ctx.SetLineWidthAsync(3);
        await _ctx.SetStrokeStyleAsync("#460813");
        await _ctx.StrokeAsync();

        // inner gear plate
        await _ctx.SetFillStyleAsync("#7a1226");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 18, 0, 2 * Math.PI);
        await _ctx.FillAsync();
        await _ctx.SetLineWidthAsync(2);
        await _ctx.SetStrokeStyleAsync("#460813");
        await _ctx.StrokeAsync();

        // gear teeth ticks
        await _ctx.SetLineWidthAsync(2.4f);
        await _ctx.SetStrokeStyleAsync("#460813");
        for (int i = 0; i < 8; i++)
        {
            double a = i * Math.PI / 4;
            await _ctx.BeginPathAsync();
            await _ctx.MoveToAsync(x + 14 * Math.Cos(a), y + 14 * Math.Sin(a));
            await _ctx.LineToAsync(x + 18 * Math.Cos(a), y + 18 * Math.Sin(a));
            await _ctx.StrokeAsync();
        }

        // glowing cyclops eye
        await _ctx.SetShadowBlurAsync(18f);
        await _ctx.SetShadowColorAsync("#ff6a2c");
        await _ctx.SetFillStyleAsync("#ffb43d");
        await _ctx.BeginPathAsync();
        await _ctx.ArcAsync(x, y, 9, 0, 2 * Math.PI);
        await _ctx.FillAsync();
        await _ctx.SetShadowBlurAsync(0f);

        // slit pupil
        await _ctx.SetFillStyleAsync("#240306");
        await _ctx.BeginPathAsync();
        await _ctx.MoveToAsync(x, y - 7);
        await _ctx.LineToAsync(x + 2.6, y);
        await _ctx.LineToAsync(x, y + 7);
        await _ctx.LineToAsync(x - 2.6, y);
        await _ctx.ClosePathAsync();
        await _ctx.FillAsync();
    }

    // ── DISPATCHER ──────────────────────────────────────────────────────────
    private async Task DrawEnemy(Enemy e, double elapsedTime)
    {
        if (e.Type == EnemyType.Elite)
        {
            await _ctx.SaveAsync();
            await _ctx.TranslateAsync(e.X, e.Y);
            await _ctx.RotateAsync((float)(elapsedTime * 0.8));
            await DrawElite(0, 0);
            await _ctx.RestoreAsync();
        }
        else if (e.Type == EnemyType.Boss)
        {
            await _ctx.SaveAsync();
            await _ctx.TranslateAsync(e.X, e.Y);
            await _ctx.RotateAsync((float)(elapsedTime * 0.35));
            await DrawBoss(0, 0);
            await _ctx.RestoreAsync();
        }
        else if (e.Type == EnemyType.Tank) await DrawTank(e.X, e.Y);
        else if (e.Type == EnemyType.Runner) await DrawRunner(e.X, e.Y);
        else await DrawStandard(e.X, e.Y);
    }
}
