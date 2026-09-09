using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace UfoDetector.Controls;

/// <summary>Vertical bar-style slider with a rectangular thumb, drawn via SkiaSharp because native
/// Slider track/thumb thickness cannot be resized on Android regardless of rotation or layout size.</summary>
public class VerticalBarSliderView : SKCanvasView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(VerticalBarSliderView), 0.0,
        BindingMode.TwoWay,
        propertyChanged: (b, _, _) => ((VerticalBarSliderView)b).InvalidateSurface());

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly BindableProperty MinimumProperty = BindableProperty.Create(
        nameof(Minimum), typeof(double), typeof(VerticalBarSliderView), 0.0,
        propertyChanged: (b, _, _) => ((VerticalBarSliderView)b).InvalidateSurface());

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly BindableProperty MaximumProperty = BindableProperty.Create(
        nameof(Maximum), typeof(double), typeof(VerticalBarSliderView), 100.0,
        propertyChanged: (b, _, _) => ((VerticalBarSliderView)b).InvalidateSurface());

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly BindableProperty BarWidthProperty = BindableProperty.Create(
        nameof(BarWidth), typeof(double), typeof(VerticalBarSliderView), 16.0,
        propertyChanged: (b, _, _) => ((VerticalBarSliderView)b).InvalidateSurface());

    public double BarWidth
    {
        get => (double)GetValue(BarWidthProperty);
        set => SetValue(BarWidthProperty, value);
    }

    private readonly SKPaint _trackPaint = new()
    {
        Color = new SKColor(6, 15, 8),
        Style = SKPaintStyle.Fill,
    };

    private readonly SKPaint _trackBorderPaint = new()
    {
        Color = new SKColor(0x1A, 0x3A, 0x20),
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1,
    };

    private readonly SKPaint _fillPaint = new()
    {
        Color = new SKColor(0x39, 0xFF, 0x14, 210),
        Style = SKPaintStyle.Fill,
    };

    private readonly SKPaint _thumbPaint = new()
    {
        Color = new SKColor(0x7D, 0xFF, 0x9A),
        Style = SKPaintStyle.Fill,
    };

    private double _panStartValue;

    public VerticalBarSliderView()
    {
        EnableTouchEvents = false;

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;
        GestureRecognizers.Add(pan);
    }

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (Height <= 0)
        {
            return;
        }

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartValue = Value;
                break;

            case GestureStatus.Running:
                // Dragging up (negative Y) increases the value, matching a physical vertical slider.
                double range = Maximum - Minimum;
                double deltaValue = -e.TotalY / Height * range;
                Value = Math.Clamp(_panStartValue + deltaValue, Minimum, Maximum);
                break;
        }
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear();

        if (Width <= 0 || Height <= 0 || e.Info.Width <= 0)
        {
            return;
        }

        // Scale so drawing coordinates match the control's own (device-independent) point units.
        float density = e.Info.Width / (float)Width;
        canvas.Scale(density);

        float w = (float)Width;
        float h = (float)Height;
        float barW = (float)BarWidth;
        float cx = w / 2f;

        double range = Maximum - Minimum;
        float normalized = range > 0 ? (float)Math.Clamp((Value - Minimum) / range, 0.0, 1.0) : 0f;

        var trackRect = new SKRect(cx - barW / 2f, 0, cx + barW / 2f, h);
        canvas.DrawRect(trackRect, _trackPaint);
        canvas.DrawRect(trackRect, _trackBorderPaint);

        float fillTop = h - normalized * h;
        var fillRect = new SKRect(cx - barW / 2f, fillTop, cx + barW / 2f, h);
        canvas.DrawRect(fillRect, _fillPaint);

        // Rectangular thumb (not the native circular thumb) centered on the fill boundary.
        const float thumbHeight = 14f;
        float thumbWidth = barW + 10f;
        float thumbCenterY = Math.Clamp(fillTop, thumbHeight / 2f, h - thumbHeight / 2f);
        var thumbRect = new SKRect(
            cx - thumbWidth / 2f, thumbCenterY - thumbHeight / 2f,
            cx + thumbWidth / 2f, thumbCenterY + thumbHeight / 2f);
        canvas.DrawRect(thumbRect, _thumbPaint);
    }
}
