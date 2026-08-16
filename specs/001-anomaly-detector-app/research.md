# Research Notes: UFO Anomaly Detector

**Feature**: 001-anomaly-detector-app
**Date**: 2026-08-16

---

## R-001 — Vertical Slider in .NET MAUI

**Decision**: Use the standard MAUI `Slider` control with `Rotation = 270` and swapped
`WidthRequest`/`HeightRequest` to simulate vertical orientation.

**Rationale**: MAUI does not ship a native `VerticalSlider`. The `Rotation` transform is
the established community pattern, requires zero additional dependencies, and works with
MAUI's touch system on Android tablets.

**Alternatives considered**:
- Custom `SKCanvasView` touch-drag slider — more control but higher complexity; deferred
  unless the `Rotation` approach produces unacceptable touch precision on the target tablet
  (YAGNI until evidence shows the issue).
- Third-party controls (Syncfusion, Telerik) — adds unjustified NuGet dependency (violates
  Principle VI).

---

## R-002 — SkiaSharp 60 fps Animation Loop

**Decision**: Use `SKGLView` with `EnableRenderLoop = true` (GPU-accelerated, synchronized
to the Android display via Choreographer). All `SKPaint`, `SKPath`, and `SKRect` objects
MUST be pre-allocated as class fields and initialised before the render loop starts. Only
`SKPath.Reset()` is permitted inside `PaintSurface`; no `new` allocations allowed in the
draw loop.

**Rationale**: `SKGLView` uses the Ganesh OpenGL backend and Android Choreographer, which
guarantees synchronization with the display refresh rate and delivers stable 60 fps on
mid-range tablets. `SKCanvasView` uses software rasterization — adequate for static or
low-frequency redraws but insufficient for two concurrent animated canvases at 60 fps.
`IDispatcherTimer`-based polling is coarser and subject to thread-pool jitter.

**Pre-allocation pattern**:
```csharp
// Declared as fields, initialised once:
private readonly SKPaint _sweepPaint = new() { IsAntialias = true, StrokeWidth = 2 };
private readonly SKPath  _sweepPath  = new();

void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    _sweepPath.Reset(); // reuse — no allocation
    // ... draw ...
}
```

**Alternatives considered**:
- `SKCanvasView` + `IDispatcherTimer` — simpler setup, but software rendering risks
  dropped frames under concurrent animation load; rejected in favour of `SKGLView`.
- MAUI `Animation` class — drives XAML element transforms only; cannot call arbitrary
  SkiaSharp draw code.

---

## R-003 — Anomaly Transition Interpolation

**Decision**: Linear interpolation (lerp) per sensor channel, computed inside
`SensorTickService` at a ~100 ms tick interval. Formula: `current += (target − current) × α`
where `α = tickInterval / transitionDuration`. All arithmetic operates on pre-allocated
`double` fields — zero heap allocation inside the tick loop.

**Transition timings**:
- Appear: 2.5 s (within the 2–3 s spec range)
- Fade: 1.5 s (spec exact)
- Sequential gate: second anomaly's appear phase does NOT start until `LerpProgress < 0.02`
  (i.e., effectively back to baseline)

**Rationale**: Lerp produces the "instrument needle slowly settling" appearance described in
the spec. It is allocation-free, reverses naturally for fade-out, and is easy to unit-test
with deterministic tick counts.

**Alternatives considered**:
- Cubic/ease-in-out easing — smoother aesthetics but harder to test and predict; deferred
  (YAGNI — the spec does not require non-linear easing).
- MAUI `Animation` — drives only XAML-layer properties; cannot animate model values.

---

## R-004 — CommunityToolkit.Mvvm Source Generation

**Decision**: Use `[ObservableProperty]` on **partial properties** (C# 14 / .NET 10 /
Roslyn 4.12+) in a `partial class DetectorViewModel`. Sensor readings are exposed as a flat
set of typed observable properties (not `ObservableCollection<>`), keeping binding paths
simple and avoiding `CollectionChanged` overhead.

**Rationale**: Partial properties are the current recommended pattern for CT.Mvvm on .NET 10.
Flat properties allow XAML bindings like `{Binding NeutronValue}` without an index, which is
clearer and faster. Source generators are confirmed to work correctly with MAUI + Android
targeting in .NET 10.

**Alternatives considered**:
- `ObservableCollection<SensorReading>` — more flexible but adds refresh complexity and
  verbose XAML bindings for a fixed-sensor dashboard; rejected (YAGNI).
- Field-based `[ObservableProperty]` — deprecated; not used.

---

## R-005 — Android Portrait Orientation Lock

**Decision**: Declare `ScreenOrientation = ScreenOrientation.Portrait` on `MainActivity`
via the `[Activity]` attribute. This is the declarative MAUI-idiomatic approach and
generates the correct `AndroidManifest.xml` entry without manual XML editing.

```csharp
[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ScreenOrientation = ScreenOrientation.Portrait,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Density | ConfigChanges.UiMode)]
public class MainActivity : MauiAppCompatActivity { }
```

**Alternatives considered**:
- Manual `AndroidManifest.xml` edit — works but bypasses MAUI's attribute-driven manifest
  generation; fragile if the attribute and XML diverge.
- Runtime `RequestedOrientation = ScreenOrientation.Portrait` — late binding; orientation
  could flash briefly before lock. Rejected.

---

## R-006 — Anomaly Data Definition Storage

**Decision**: Define the 5 anomaly records as `static readonly IReadOnlyList<Anomaly>`
in `AnomalyDefinitions.cs`. The `AnomalyEvaluator` service receives this list via
constructor injection (DI).

**Rationale**: Satisfies FR-013 (data-driven, not hard-coded per UI element). A static
in-memory list avoids any I/O, serialisation, or file-access dependency at runtime, which
is correct for an offline visual-effects tool with a fixed small dataset. Five records do
not justify a JSON embedded resource or SQLite (both explicitly prohibited).

**Alternatives considered**:
- JSON embedded resource — adds a serialiser dependency; overkill for 5 records (YAGNI).
- SQLite — explicitly prohibited by the constitution.
- Per-anomaly hard-coded `if/else` blocks — violates FR-013.
