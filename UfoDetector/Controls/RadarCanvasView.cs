using System.Diagnostics;
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
    // Full sweep rotation period
    private const float RotationPeriodSeconds = 5.0f;
    private const float AngularSpeed = MathF.Tau / RotationPeriodSeconds;
    // Raw per-frame dt is smoothed (EMA) rather than clamped-and-dropped: a human eye
    // is very sensitive to velocity jitter on a single rotating line, so absorbing a
    // hitch gradually over the next several frames looks far smoother than either
    // snapping forward to catch up or abruptly slowing down for one frame.
    private const float DtSmoothingFactor = 0.15f;
    // Still bounds truly extreme pauses (e.g. app backgrounded) to a sane ceiling
    private const float MaxRawFrameDelta = 1f;

    // ── State ────────────────────────────────────────────────────────────────
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private double _lastFrameSeconds;
    private float _smoothedDt = -1f;
    private float _sweepAngle; // radians, advanced by elapsed time
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

        double nowSeconds = _clock.Elapsed.TotalSeconds;
        float rawDt = _lastFrameSeconds == 0
            ? 0f
            : MathF.Min((float)(nowSeconds - _lastFrameSeconds), MaxRawFrameDelta);
        _lastFrameSeconds = nowSeconds;

        // EMA smoothing: a hitch's lost time gets absorbed gradually over the next
        // several frames instead of causing one visible velocity jump
        _smoothedDt = _smoothedDt < 0f ? rawDt : _smoothedDt + (rawDt - _smoothedDt) * DtSmoothingFactor;
        float dt = _smoothedDt;

        _prevSweepAngle = _sweepAngle;
        _sweepAngle = (_sweepAngle + AngularSpeed * dt) % MathF.Tau;

        // Transparent clear each frame (no opaque fill) so only lines/dots are visible
        canvas.Clear(SKColors.Transparent);

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

            // Apply drift scaled by elapsed time (fixed blips skip)
            if (blip.DriftAngularSpeed != 0.0 || blip.DriftRadialSpeed != 0.0)
            {
                blip.Angle    = (blip.Angle + blip.DriftAngularSpeed * dt) % Math.Tau;
                blip.Distance = Math.Clamp(blip.Distance + blip.DriftRadialSpeed * dt, 0.0, 1.0);
            }

            // Detect if sweep just crossed this blip's angle
            if (SweepCrossed(_prevSweepAngle, _sweepAngle, (float)blip.Angle))
                blip.Intensity = 1.0;
            else
                blip.Intensity = Math.Max(0.0, blip.Intensity - dt / IntensityDecayTime);

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
