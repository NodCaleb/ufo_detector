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

    // Bars are downsampled to this count for display, independent of the source bin count
    private const int DisplayBarCount = 17;

    // ── Pre-allocated paints ─────────────────────────────────────────────────
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

    // Reusable struct — assigned per bar, no heap allocation
    private SKRect _barRect;

    public InfrasoundCanvasView()
    {
        // No continuous render loop needed: bars only change when Bands is set (10 Hz tick),
        // which already calls InvalidateSurface(). A second always-on GL loop next to the
        // radar's would double concurrent GPU work and starve both on low-power hardware.
        HasRenderLoop = false;
    }

    protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        int w = e.Info.Width;
        int h = e.Info.Height;

        var bands = Bands;
        if (bands == null || bands.Length == 0)
            return;

        float barW = (float)w / DisplayBarCount;

        for (int i = 0; i < DisplayBarCount; i++)
        {
            // Nearest-neighbor downsample from the source bin count to DisplayBarCount
            int srcIndex = Math.Clamp((int)((i + 0.5f) * bands.Length / DisplayBarCount), 0, bands.Length - 1);
            float barH = (float)(bands[srcIndex] * h);
            bool aboveAlarm = bands[srcIndex] > AlarmThreshold;

            // SKRect is a value type — assignment is a field write, zero allocation
            _barRect = new SKRect(
                i * barW + 1,
                h - barH,
                (i + 1) * barW - 1,
                h);
            canvas.DrawRect(_barRect, aboveAlarm ? _alarmBarPaint : _barPaint);
        }
    }
}
