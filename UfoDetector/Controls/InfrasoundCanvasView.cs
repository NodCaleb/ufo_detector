using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace UfoDetector.Controls;

/// <summary>GPU-accelerated 20-bin infrasound bar chart. SKGLView exempt from unit tests per Principle V.</summary>
public class InfrasoundCanvasView : SKGLView
{
    // ── Alarm threshold (relative amplitude 0–1) ────────────────────────────
    public const double AlarmThreshold = 0.5;

    // ── Bindable property: ViewModel updates this to drive animation ─────────
    public static readonly BindableProperty BandsProperty = BindableProperty.Create(
        nameof(Bands),
        typeof(double[]),
        typeof(InfrasoundCanvasView),
        defaultValue: null,
        propertyChanged: (b, _, _) => ((InfrasoundCanvasView)b).InvalidateSurface());

    public double[]? Bands
    {
        get => (double[]?)GetValue(BandsProperty);
        set => SetValue(BandsProperty, value);
    }

    // ── Pre-allocated paints ─────────────────────────────────────────────────
    private readonly SKPaint _bgPaint = new()
    {
        Color = new SKColor(6, 12, 6),
        Style = SKPaintStyle.Fill,
    };

    private readonly SKPaint _barPaint = new()
    {
        Color = new SKColor(57, 255, 20, 210),
        Style = SKPaintStyle.Fill,
    };

    private readonly SKPaint _alarmBarPaint = new()
    {
        Color = new SKColor(255, 34, 0, 220),
        Style = SKPaintStyle.Fill,
    };

    private readonly SKPaint _thresholdPaint;

    // Reusable struct — assigned per bar, no heap allocation
    private SKRect _barRect;

    public InfrasoundCanvasView()
    {
        HasRenderLoop = true;

        // Dashed threshold line — PathEffect allocated once, never inside PaintSurface
        _thresholdPaint = new SKPaint
        {
            Color = new SKColor(255, 179, 0, 160),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            PathEffect = SKPathEffect.CreateDash([4f, 4f], 0f),
        };
    }

    protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        int w = e.Info.Width;
        int h = e.Info.Height;

        canvas.DrawRect(0, 0, w, h, _bgPaint);

        var bands = Bands;
        if (bands == null || bands.Length == 0)
            return;

        int binCount = bands.Length; // always 20
        float barW = (float)w / binCount;

        for (int i = 0; i < binCount; i++)
        {
            float barH = (float)(bands[i] * h);
            bool aboveAlarm = bands[i] > AlarmThreshold;

            // SKRect is a value type — assignment is a field write, zero allocation
            _barRect = new SKRect(
                i * barW + 1,
                h - barH,
                (i + 1) * barW - 1,
                h);
            canvas.DrawRect(_barRect, aboveAlarm ? _alarmBarPaint : _barPaint);
        }

        // Dashed alarm threshold line
        float threshY = (float)(h - AlarmThreshold * h);
        canvas.DrawLine(0, threshY, w, threshY, _thresholdPaint);
    }
}
