using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using UfoDetector.Models;
using UfoDetector.Models.Enums;

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

    // Blip paint — colour set per blip before each draw call
    private readonly SKPaint _blipPaint = new()
    {
        Style = SKPaintStyle.Fill,
        IsAntialias = true,
    };

    // ── Blip type colours (SKColor constants, no allocation per frame) ───────
    private static readonly SKColor ColourEM          = new(57,  255, 20);
    private static readonly SKColor ColourRadiation   = new(255, 179, 0);
    private static readonly SKColor ColourIonisation  = new(68,  170, 255);
    private static readonly SKColor ColourThermal     = new(255, 102, 0);
    private static readonly SKColor ColourGeomagnetic = new(0,   221, 204);
    private static readonly SKColor ColourChrono      = new(204, 68,  255);

    // Seconds until intensity decays to zero after a sweep pass
    private const float IntensityDecayTime = 3.0f;
    // Frame time estimate at 60 fps
    private const float FrameTime = 1f / 60f;

    // ── State ────────────────────────────────────────────────────────────────
    private float _sweepAngle; // radians, advanced per frame
    private float _prevSweepAngle;

    // Active blips managed internally (populated from RadarBlipTemplate)
    private RadarBlip[] _blips = [];
    private readonly Random _rng = new();

    // ── Bindable properties ──────────────────────────────────────────────────

    public static readonly BindableProperty ActiveAnomalyProperty = BindableProperty.Create(
        nameof(ActiveAnomaly),
        typeof(Anomaly),
        typeof(RadarCanvasView),
        defaultValue: null,
        propertyChanged: (b, oldVal, newVal) =>
            ((RadarCanvasView)b).OnActiveAnomalyChanged((Anomaly?)oldVal, (Anomaly?)newVal));

    public Anomaly? ActiveAnomaly
    {
        get => (Anomaly?)GetValue(ActiveAnomalyProperty);
        set => SetValue(ActiveAnomalyProperty, value);
    }

    public static readonly BindableProperty PhaseProperty = BindableProperty.Create(
        nameof(Phase),
        typeof(TransitionPhase),
        typeof(RadarCanvasView),
        defaultValue: TransitionPhase.Idle,
        propertyChanged: (b, _, newVal) =>
        {
            if ((TransitionPhase)newVal == TransitionPhase.Idle)
                ((RadarCanvasView)b)._blips = [];
        });

    public TransitionPhase Phase
    {
        get => (TransitionPhase)GetValue(PhaseProperty);
        set => SetValue(PhaseProperty, value);
    }

    public RadarCanvasView()
    {
        HasRenderLoop = true;
    }

    private void OnActiveAnomalyChanged(Anomaly? oldAnomaly, Anomaly? newAnomaly)
    {
        if (newAnomaly is null)
        {
            _blips = [];
            return;
        }
        // Don't re-spawn if the same anomaly is still active
        if (oldAnomaly?.Id == newAnomaly.Id)
            return;

        _blips = SpawnBlips(newAnomaly);
    }

    private RadarBlip[] SpawnBlips(Anomaly anomaly)
    {
        var list = new System.Collections.Generic.List<RadarBlip>();
        foreach (var template in anomaly.RadarBlips)
        {
            int count = template.CountMin == template.CountMax
                ? template.CountMin
                : template.CountMin + _rng.Next(template.CountMax - template.CountMin + 1);

            for (int i = 0; i < count; i++)
            {
                double angle    = _rng.NextDouble() * Math.Tau;
                double distance = template.InitialDistanceMin +
                                  _rng.NextDouble() * (template.InitialDistanceMax - template.InitialDistanceMin);

                // Anomaly 4: Geomagnetic — second blip symmetric (angle + π)
                if (template.Type == BlipType.Geomagnetic && i == 1 && list.Count > 0)
                    angle = list[^1].Angle + Math.PI;

                list.Add(new RadarBlip
                {
                    Type              = template.Type,
                    Angle             = angle,
                    Distance          = distance,
                    Intensity         = 0.0,
                    DriftAngularSpeed = template.DriftAngularSpeed,
                    DriftRadialSpeed  = template.DriftRadialSpeed,
                });
            }
        }
        return [.. list];
    }

    protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        int w = e.Info.Width;
        int h = e.Info.Height;
        float cx = w * 0.5f;
        float cy = h * 0.5f;
        float r  = MathF.Min(cx, cy) - 4f;

        _prevSweepAngle = _sweepAngle;
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

        // Rotating sweep line
        canvas.DrawLine(
            cx, cy,
            cx + r * MathF.Cos(_sweepAngle),
            cy + r * MathF.Sin(_sweepAngle),
            _sweepPaint);

        // Blips
        var blips = _blips;
        for (int i = 0; i < blips.Length; i++)
        {
            var blip = blips[i];

            // Apply per-frame drift (fixed blips skip)
            if (blip.DriftAngularSpeed != 0.0 || blip.DriftRadialSpeed != 0.0)
            {
                blip.Angle    = (blip.Angle + blip.DriftAngularSpeed * FrameTime) % Math.Tau;
                blip.Distance = Math.Clamp(blip.Distance + blip.DriftRadialSpeed * FrameTime, 0.0, 1.0);
            }

            // Detect if sweep just crossed this blip's angle
            if (SweepCrossed(_prevSweepAngle, _sweepAngle, (float)blip.Angle))
                blip.Intensity = 1.0;
            else
                blip.Intensity = Math.Max(0.0, blip.Intensity - FrameTime / IntensityDecayTime);

            if (blip.Intensity < 0.01)
                continue;

            float bx = cx + r * (float)(blip.Distance * Math.Cos(blip.Angle));
            float by = cy + r * (float)(blip.Distance * Math.Sin(blip.Angle));
            float bRadius = 5f + 2f * (float)blip.Intensity;

            SKColor baseColour = BlipColour(blip.Type);
            _blipPaint.Color = baseColour.WithAlpha((byte)(blip.Intensity * 230));
            canvas.DrawCircle(bx, by, bRadius, _blipPaint);
        }
    }

    private static bool SweepCrossed(float prev, float curr, float blipAngle)
    {
        // Handle wrap-around (prev > curr means wrap occurred)
        if (prev <= curr)
            return blipAngle > prev && blipAngle <= curr;
        // Wrapped: blip is either in [prev, 2π) or [0, curr]
        return blipAngle > prev || blipAngle <= curr;
    }

    private static SKColor BlipColour(BlipType type) => type switch
    {
        BlipType.EM          => ColourEM,
        BlipType.Radiation   => ColourRadiation,
        BlipType.Ionisation  => ColourIonisation,
        BlipType.Thermal     => ColourThermal,
        BlipType.Geomagnetic => ColourGeomagnetic,
        BlipType.Chrono      => ColourChrono,
        _                    => ColourEM,
    };
}
