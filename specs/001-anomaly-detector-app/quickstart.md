# Quickstart & Validation Guide: UFO Anomaly Detector

**Feature**: 001-anomaly-detector-app
**Date**: 2026-08-16

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0 LTS |
| MAUI + Android workload | `dotnet workload install maui-android` |
| Android emulator OR physical tablet | ≥768×1024 px, Android 8.0+ (API 26+) |
| IDE | Visual Studio 2022 17.12+, Rider 2024.3+, or VS Code + MAUI extension |

```powershell
# Install the required workload (run once)
dotnet workload install maui-android

# Verify
dotnet workload list
```

---

## Build

```powershell
cd d:\Sources\ufo_detector

# Debug build — emulator or USB-connected device
dotnet build UfoDetector/UfoDetector.csproj -f net10.0-android

# Release APK
dotnet publish UfoDetector/UfoDetector.csproj -f net10.0-android -c Release
```

Build gate: **zero compiler warnings** (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
is enforced). Any warning causes a CI failure.

---

## Run Unit Tests

```powershell
dotnet test UfoDetector.Tests/UfoDetector.Tests.csproj
```

All tests must pass without a device or emulator. Tests cover:
- `DetectorViewModel` — control state changes, anomaly property propagation
- `AnomalyEvaluator` — trigger matching (all 5 anomalies, boundary conditions, no-match)
- `SensorTickService` — drift within ±5 %, baseline return, update period
- `TransitionOrchestrator` — appear/fade lerp progress, sequential constraint (FR-011)

---

## Functional Validation Scenarios

### V-1: Live Sensor Dashboard (User Story 1, P1)

**Setup**: Launch the app on emulator or device.

**Steps**:
1. Confirm 6 sensor panels are visible: Neutron radiation, Ionisation, Geomagnetic field,
   Thermal anomaly, Chrono anomaly, Infrasound spectrum.
2. Observe for 5 seconds without touching controls.

**Expected**:
- All numeric gauge values fluctuate visibly every 1–2 s (±3–5 % drift).
- All gauges show green fill and "НОРМА" label.
- Radar sweep rotates continuously.
- Infrasound bar-graph animates each frame.
- No frozen values, no crashes.

---

### V-2: Control Inputs (User Story 2, P2)

**Setup**: App running, no anomaly active.

**Steps**:
1. Tap the mode toggle → label changes АКТИВ ↔ ПАССИВ; LED indicator colour updates.
2. Drag sensitivity slider to 75 % → numeric label reads exactly "75 %".
3. Drag noise-suppression slider to 15 % → numeric label reads exactly "15 %".
4. Release both sliders → values stay; no snap-back after release.
5. Set mode to АКТИВ, sensitivity to 75 %, noise suppression to 15 %. Confirm displayed
   values match exactly.

**Expected**: Displayed values always match input; no drift after release.

---

### V-3: Anomaly Trigger and Fade (User Story 3, P3)

**Setup**: App running.

**Steps** (Anomaly 1 — Электромагнитный след):
1. Set mode → ПАССИВ.
2. Set sensitivity → 65 %.
3. Set noise suppression → 20 %.
4. Wait up to 3 seconds.

**Expected (active)**:
- Radar shows 2–3 green EM blips that slowly drift.
- Geomagnetic field gauge rises to ~55 нТл with amber "АНОМАЛИЯ" label.
- Infrasound spectrum shows a low-amplitude hump at 2–5 Hz.
- Transition to active state completes within 3 seconds (gradual onset).

**Steps (fade)**:
5. Change noise suppression to 50 % (outside range).

**Expected (fade)**:
- All anomaly indicators return to baseline within 1.5 seconds (gradual fade, not instant).

---

### V-4: All Five Anomaly Signatures (User Story 4, P4)

Apply each combination from [contracts/anomaly-definitions.md](contracts/anomaly-definitions.md).
For each anomaly verify:

| Check | Pass criterion |
|-------|----------------|
| Activates within 3 s | All three conditions met simultaneously |
| Correct radar blip type and count | Matches contract table |
| Key sensor(s) at target value | Within ±5 % of documented target (drift tolerance) |
| Key sensor status label and colour | Matches contract table exactly |
| No two anomalies produce identical radar + gauge combination | Visual inspection |

**Sequential transition test**:
1. Activate Anomaly 1 (fade has started).
2. Immediately set Anomaly 2 controls while Anomaly 1 is still fading.
3. Expected: Anomaly 1 fully fades before Anomaly 2 begins to appear (FR-011).

---

### V-5: Boundary Values (Edge Cases)

| Test | Input | Expected |
|------|-------|----------|
| Lower boundary | Sensitivity = 60 %, Noise = 15 %, Mode = ПАССИВ | Anomaly 1 triggers |
| Upper boundary | Sensitivity = 70 %, Noise = 25 %, Mode = ПАССИВ | Anomaly 1 triggers |
| Just outside lower | Sensitivity = 59 %, Noise = 15 %, Mode = ПАССИВ | No anomaly |
| Just outside upper | Sensitivity = 71 %, Noise = 25 %, Mode = ПАССИВ | No anomaly |
| Rapid mode toggle | Toggle mode 10 times quickly | Last stable state used; no crash |
| Simultaneous slider drag | Drag both sliders at same time | Each slider operates independently; no crash |

---

## Performance Validation

### V-6: 60 fps Rendering (SC-002)

**Required before any rendering PR is considered complete (Constitution Principle IV).**

1. Connect Android Studio GPU Profiler (or Perfetto) to the running app.
2. Observe radar and infrasound canvas frame times for 30 continuous seconds during active
   anomaly state (most load).
3. **Pass**: All frames ≤ 16.7 ms; no sustained drop visible in profiler.

---

### V-7: 30-Minute Stability (SC-006)

1. Run the app continuously for 30 minutes on the target tablet.
2. Cycle through all five anomaly states periodically.
3. **Pass**: No crash; Memory Profiler shows flat heap (no growth); frame rate stays ≥ 45 fps.

---

## SkiaSharp Draw-Loop Manual Test Procedures

*(Required by Constitution Principle V — draw-loop code is exempt from unit tests.)*

### Radar canvas (`RadarCanvasView`)

| # | Procedure | Pass criterion |
|---|-----------|----------------|
| 1 | Observe sweep rotation | Full 360° cycle completes in ~2–4 s; no stutter |
| 2 | Observe blip fade | Blips dim to invisible within one sweep cycle after the sweep passes |
| 3 | Observe range rings | Three concentric rings render cleanly without artefacts at all zoom levels |
| 4 | Memory Profiler heap check | Heap is flat during active rendering (no per-frame allocations inside `PaintSurface`) |

### Infrasound canvas (`InfrasoundCanvasView`)

| # | Procedure | Pass criterion |
|---|-----------|----------------|
| 1 | Observe baseline animation | 20 bars animate continuously; heights vary per tick |
| 2 | Trigger Anomaly 3 or 5 | Bars above the dashed alarm threshold line turn red |
| 3 | Count bars | Exactly 20 bars visible (one per Hz, 0–20 Hz range) |
| 4 | Memory Profiler heap check | Heap flat; no allocations inside `PaintSurface` |

---

## Troubleshooting

| Symptom | Likely cause | Remedy |
|---------|-------------|--------|
| `dotnet build` fails with workload error | MAUI Android workload not installed | `dotnet workload install maui-android` |
| Frame rate drops below 60 fps | Allocations inside draw loop OR using `SKCanvasView` instead of `SKGLView` | Check pre-allocation; confirm `SKGLView` with `EnableRenderLoop = true` is used |
| Vertical slider touch imprecise | `Rotation = 270` on `Slider` has edge-case touch behaviour | Replace with custom `SKCanvasView` touch-drag control (see research.md R-001) |
| Anomaly does not trigger at exact boundary | Off-by-one in range check | Use `>=` and `<=` (inclusive); see [contracts/anomaly-definitions.md](contracts/anomaly-definitions.md) |
| Second anomaly appears before first fully fades | Sequential gate not enforced | Check `LerpProgress ≤ 0.02` guard in `TransitionOrchestrator` (FR-011) |
| App rotates to landscape | Portrait lock not applied | Confirm `[Activity(ScreenOrientation = ScreenOrientation.Portrait)]` on `MainActivity` |

---

## Validation Results Log

*Fill in during device/emulator testing. Requires Android emulator or physical device.*

### V-1 through V-5 Results (T057)

| Scenario | Criterion | Result | Notes |
|----------|-----------|--------|-------|
| V-1 | 6 sensor panels visible | ⬜ PENDING | Run on device |
| V-1 | All gauges drift ±3–5 % every 1–2 s | ⬜ PENDING | |
| V-1 | All gauges show green "НОРМА" | ⬜ PENDING | |
| V-1 | Radar sweep rotates continuously | ⬜ PENDING | |
| V-1 | Infrasound bar-graph animates each frame | ⬜ PENDING | |
| V-2 | Mode toggle АКТИВ ↔ ПАССИВ with LED update | ⬜ PENDING | |
| V-2 | Sensitivity slider shows exact value | ⬜ PENDING | |
| V-2 | Noise-suppression slider shows exact value | ⬜ PENDING | |
| V-2 | Values persist after slider release (no snap-back) | ⬜ PENDING | |
| V-3 | Anomaly 1 activates within 3 s | ⬜ PENDING | |
| V-3 | Correct blip type (EM), correct drift | ⬜ PENDING | |
| V-3 | Geomagnetic gauge rises to ~55 нТл, АНОМАЛИЯ label | ⬜ PENDING | |
| V-3 | Infrasound low hump at 2–5 Hz | ⬜ PENDING | |
| V-3 | Fade completes within 1.5 s after controls exit range | ⬜ PENDING | |
| V-4 | All 5 anomalies activate within 3 s | ⬜ PENDING | |
| V-4 | Each blip type/count matches contract | ⬜ PENDING | |
| V-4 | Key sensor values within ±5 % of target | ⬜ PENDING | |
| V-4 | Status label/colour matches contract exactly | ⬜ PENDING | |
| V-4 | FR-011 sequential transition respected | ⬜ PENDING | |
| V-5 | Lower boundary (sensitivity=60, noise=15, ПАССИВ) → Anomaly 1 | ⬜ PENDING | |
| V-5 | Upper boundary (sensitivity=70, noise=25, ПАССИВ) → Anomaly 1 | ⬜ PENDING | |
| V-5 | Just outside lower (sensitivity=59) → no anomaly | ⬜ PENDING | |
| V-5 | Just outside upper (sensitivity=71) → no anomaly | ⬜ PENDING | |
| V-5 | Rapid mode toggle (×10) → no crash | ⬜ PENDING | |
| V-5 | Simultaneous slider drag → independent, no crash | ⬜ PENDING | |

### V-6 GPU Profiler Results (T058)

*Run Android Studio GPU Profiler or Perfetto during active-anomaly state.*

| Criterion | Result | Notes |
|-----------|--------|-------|
| All frames ≤ 16.7 ms (both canvases) for 30 s | ⬜ PENDING | |
| No sustained frame-time spikes visible in profiler | ⬜ PENDING | |

### V-8 30-Minute Soak Test Results (T060)

*Run on emulator or device. Use Android Studio Memory Profiler.*

| Criterion | Result | Notes |
|-----------|--------|-------|
| No crash during 30-minute session | ⬜ PENDING | |
| Heap growth < 10 MB over session | ⬜ PENDING | |
| Frame rate ≥ 45 fps throughout | ⬜ PENDING | |
