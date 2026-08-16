# Implementation Plan: UFO Anomaly Detector Mobile App

**Branch**: `001-anomaly-detector-app` | **Date**: 2026-08-16 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-anomaly-detector-app/spec.md`

## Summary

Single-screen .NET MAUI Android app (C# 14 / .NET 10 LTS) that displays a real-time animated
sensor dashboard with 5 hidden anomaly trigger combinations. Two SKGLView canvases (radar sweep
+ infrasound spectrum) render at 60 fps via GPU-accelerated SkiaSharp; four gauge panels and a
chrono bar animate at 1–2 s intervals. Anomaly detection uses pure in-memory logic driven by
three controls (mode toggle + two sliders). All transitions interpolate smoothly over 2–3 s
(appear) / 1.5 s (fade). No network access; all sensor values are algorithmically generated.

## Technical Context

**Language/Version**: C# 14 (.NET 10 LTS)

**Primary Dependencies**:
- `Microsoft.Maui.Controls` — built-in; UI framework
- `CommunityToolkit.Mvvm` 8.x — `[ObservableProperty]` partial properties, `[RelayCommand]`
- `SkiaSharp.Views.Maui.Controls` — `SKGLView` for radar and infrasound canvases (GPU-accelerated)
- `CommunityToolkit.Maui` — optional; deferred unless MAUI native animation APIs prove insufficient

**Storage**: `Microsoft.Maui.Storage.Preferences` — persist last slider values and mode between sessions

**Testing**: xUnit + Moq; `dotnet test` (no device or emulator required)

**Target Platform**: Android API 26+ (Android 8.0 Oreo minimum), tablet ≥768×1024 px, portrait-locked

**Project Type**: Mobile app (Android, single-screen)

**Performance Goals**: Stable 60 fps on both SkiaSharp canvases; sensor value ticks at ~100 ms (interpolation), display update at 1–2 s

**Constraints**: No `INTERNET` permission; portrait orientation locked; offline-only; no SQLite; no Blazor Hybrid; `TreatWarningsAsErrors = true`

**Scale/Scope**: 1 screen, 6 sensor panels, 5 anomaly definitions, 2 SkiaSharp canvases, 1 ViewModel

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Android-First via .NET MAUI | ✅ PASS | Single Android target; all UI via MAUI controls; `[Activity(ScreenOrientation = Portrait)]` on `MainActivity` |
| II | MVVM Clean Architecture | ✅ PASS | Models / Services / ViewModels / Views strictly separated; all services injected via interfaces in `MauiProgram.cs`; zero logic in code-behind |
| III | Zero External Connections | ✅ PASS | All sensor values algorithmic; `INTERNET` permission absent from `AndroidManifest.xml` |
| IV | Performance-First Rendering | ✅ PASS | `SKGLView` with `EnableRenderLoop = true` for both canvases; all `SKPaint`/`SKPath` pre-allocated; GPU Profiler validation mandatory before rendering feature is merged |
| V | TDD | ✅ PASS | ViewModels and all Service implementations covered by xUnit tests written before implementation code; SkiaSharp draw-loop classes (`RadarCanvasView`, `InfrasoundCanvasView`) exempt with documented manual test procedures in [quickstart.md](quickstart.md) |
| VI | Simplicity & YAGNI | ✅ PASS | Single MAUI project + one test project; one ViewModel; anomalies as static data records; `CommunityToolkit.Maui` deferred until evidence shows need |

**Post-design re-check**: All gates still PASS after Phase 1 design. No violations; Complexity Tracking section omitted.

## Project Structure

### Documentation (this feature)

```text
specs/001-anomaly-detector-app/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/
│   └── anomaly-definitions.md   # Phase 1 output
└── tasks.md             # Phase 2 output (created by /speckit.tasks — not yet)
```

### Source Code (repository root)

```text
UfoDetector/                              # .NET MAUI Android app (net10.0-android)
├── MauiProgram.cs                        # DI registration for all services + VM
├── App.xaml / App.xaml.cs
├── AppShell.xaml / AppShell.xaml.cs
├── Platforms/
│   └── Android/
│       ├── AndroidManifest.xml           # No INTERNET permission; portrait orientation
│       └── MainActivity.cs              # [Activity(ScreenOrientation = Portrait)]
├── Models/
│   ├── Anomaly.cs                        # Immutable anomaly definition record
│   ├── AnomalyTrigger.cs                 # Mode + sensitivity range + noise range
│   ├── SensorTargetValues.cs             # Per-sensor target reading when anomaly is active
│   ├── SensorBaseline.cs                 # Normal-range thresholds for status colour coding
│   ├── SensorSnapshot.cs                 # Instantaneous value + unit + status
│   ├── DetectorState.cs                  # Runtime mutable state (mode, sliders, phase, lerp)
│   ├── RadarBlip.cs                      # Live blip (type, angle, distance, intensity)
│   ├── RadarBlipTemplate.cs              # Blip prototype from anomaly definition
│   ├── AnomalyDefinitions.cs             # Static readonly list of 5 Anomaly records
│   └── Enums/
│       ├── DetectorMode.cs               # Active | Passive
│       ├── TransitionPhase.cs            # Idle | Appearing | Active | Fading
│       ├── SensorStatus.cs               # Normal | Elevated | Anomaly | Danger | Critical | CriticalHigh
│       └── BlipType.cs                   # EM | Radiation | Ionisation | Thermal | Geomagnetic | Chrono
├── Services/
│   ├── IAnomalyEvaluator.cs              # Interface: Anomaly? Evaluate(DetectorState)
│   ├── AnomalyEvaluator.cs               # Checks trigger conditions from AnomalyDefinitions
│   ├── ISensorTickService.cs             # Interface: StartAsync() / Stop()
│   ├── SensorTickService.cs              # ~100 ms tick; drives lerp + drift; notifies ViewModel
│   ├── ITransitionOrchestrator.cs        # Interface: step appear/fade lerp progress
│   └── TransitionOrchestrator.cs         # LerpProgress update; phase state machine
├── ViewModels/
│   └── DetectorViewModel.cs              # Single VM; all observable sensor + control state
├── Views/
│   ├── DetectorPage.xaml                 # Entire UI; binds exclusively to DetectorViewModel
│   └── DetectorPage.xaml.cs             # Constructor + Loaded hook only
├── Controls/
│   ├── RadarCanvasView.cs                # SKGLView subclass — rotating sweep + blips
│   └── InfrasoundCanvasView.cs           # SKGLView subclass — 20-bin bar spectrum
└── Resources/
    ├── Fonts/                            # ShareTechMono.ttf or closest monospace fallback
    └── Styles/
        └── Colors.xaml                   # Phosphor-green palette + CRT scanline overlay colour

UfoDetector.Tests/                        # xUnit test project (dotnet test, no device)
├── ViewModels/
│   └── DetectorViewModelTests.cs
└── Services/
    ├── AnomalyEvaluatorTests.cs
    ├── SensorTickServiceTests.cs
    └── TransitionOrchestratorTests.cs
```

**Structure Decision**: Single MAUI project (`UfoDetector/`) with a companion xUnit test project
(`UfoDetector.Tests/`). The app has no backend; all logic is in-process. `Controls/` holds
`SKGLView` subclasses that are view-layer code and therefore excluded from mandatory unit tests
under Constitution Principle V — manual test procedures are documented in [quickstart.md](quickstart.md).

## Complexity Tracking

> No Constitution violations — section not required.
