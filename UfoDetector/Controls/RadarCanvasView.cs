using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace UfoDetector.Controls;

/// <summary>GPU-accelerated rotating PPI radar sweep. SKGLView exempt from unit tests per Principle V.</summary>
public class RadarCanvasView : SKGLView
{
    // ── Pre-allocated paints (no new() inside OnPaintSurface) ───────────────
    private readonly SKPaint _bgPaint = new()
    {
        Color = new SKColor(6, 12, 6),
        Style = SKPaintStyle.Fill,
    };

    private readonly SKPaint _ringPaint = new()
    {
        Color = new SKColor(57, 255, 20, 50),
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1,
        IsAntialias = true,
    };

    private readonly SKPaint _sweepPaint = new()
    {
        Color = new SKColor(57, 255, 20, 200),
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2,
        IsAntialias = true,
    };

    private readonly SKPaint _crosshairPaint = new()
    {
        Color = new SKColor(57, 255, 20, 25),
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1,
    };

    // ── State ────────────────────────────────────────────────────────────────
    private float _sweepAngle; // radians, advanced per frame

    public RadarCanvasView()
    {
        HasRenderLoop = true;
    }

    protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        int w = e.Info.Width;
        int h = e.Info.Height;
        float cx = w * 0.5f;
        float cy = h * 0.5f;
        float r  = MathF.Min(cx, cy) - 4f;

        // Advance sweep: ~1 full rotation every 3 s at 60 fps (~0.035 rad/frame)
        _sweepAngle = (_sweepAngle + 0.035f) % MathF.Tau;

        // Background
        canvas.DrawRect(0, 0, w, h, _bgPaint);

        // Cross-hairs
        canvas.DrawLine(cx, cy - r, cx, cy + r, _crosshairPaint);
        canvas.DrawLine(cx - r, cy, cx + r, cy, _crosshairPaint);

        // 3 concentric range rings
        canvas.DrawCircle(cx, cy, r,          _ringPaint);
        canvas.DrawCircle(cx, cy, r * 0.667f, _ringPaint);
        canvas.DrawCircle(cx, cy, r * 0.333f, _ringPaint);

        // Rotating sweep line — DrawLine avoids deprecated SKPath.MoveTo/LineTo
        canvas.DrawLine(
            cx, cy,
            cx + r * MathF.Cos(_sweepAngle),
            cy + r * MathF.Sin(_sweepAngle),
            _sweepPaint);
    }
}
