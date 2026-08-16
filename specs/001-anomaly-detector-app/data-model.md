# Data Model: UFO Anomaly Detector

**Feature**: 001-anomaly-detector-app
**Date**: 2026-08-16

---

## Entities

### 1. `Anomaly` *(immutable record)*

Root definition for a single hidden anomaly. Defined statically in `AnomalyDefinitions.cs`.
The `AnomalyEvaluator` service reads this list; no UI component may embed these values directly.

| Field | Type | Description |
|-------|------|-------------|
| `Id` | `int` | 1–5; unique per anomaly |
| `Name` | `string` | Display name (Russian), e.g. "Электромагнитный след" |
| `Narrative` | `string` | Flavour text shown during active state |
| `Trigger` | `AnomalyTrigger` | Activation conditions |
| `SensorTargets` | `SensorTargetValues` | Target sensor readings at full activation (lerp = 1.0) |
| `RadarBlips` | `IReadOnlyList<RadarBlipTemplate>` | Blip prototypes spawned on the radar when active |

---

### 2. `AnomalyTrigger` *(immutable record)*

Encodes the three conditions that must all be satisfied simultaneously.
Boundary values are **inclusive** on both ends (FR-008).

| Field | Type | Description |
|-------|------|-------------|
| `Mode` | `DetectorMode` | `Active` or `Passive` |
| `SensitivityMin` | `int` | Lower bound (inclusive), 0–100 |
| `SensitivityMax` | `int` | Upper bound (inclusive), 0–100 |
| `NoiseSuppMin` | `int` | Lower bound (inclusive), 0–100 |
| `NoiseSuppMax` | `int` | Upper bound (inclusive), 0–100 |

---

### 3. `SensorTargetValues` *(immutable record)*

Target readings per sensor channel when an anomaly is **fully active** (lerp progress = 1.0).
The `SensorTickService` interpolates from baseline toward these values during appear, and back
during fade.

| Field | Type | Unit | Baseline (normal) |
|-------|------|------|-------------------|
| `NeutronRadiation` | `double` | мЗв/ч | ~3.2 |
| `NeutronStatus` | `SensorStatus` | — | `Normal` |
| `Ionisation` | `double` | % | ~12 |
| `IonisationStatus` | `SensorStatus` | — | `Normal` |
| `GeomagneticField` | `double` | нТл | ~47 |
| `GeomagneticStatus` | `SensorStatus` | — | `Normal` |
| `ThermalAnomaly` | `double` | °C | ~0.0 |
| `ThermalStatus` | `SensorStatus` | — | `Normal` |
| `ChronoAnomaly` | `double` | Δt seconds | ~0.003 |
| `ChronoStatus` | `SensorStatus` | — | `Normal` |
| `InfrasoundBands` | `double[]` (20 elements) | relative amplitude 0–1 | ~0.05 per bin |

*Baseline values also defined in `SensorBaseline.cs` and used for ±3–5 % drift when no anomaly
is active. When an anomaly is active and `LerpProgress ≥ 0.5`, each sensor's status is taken
directly from the anomaly's `{Channel}Status` field rather than derived from `SensorBaseline`
thresholds. This allows Anomaly 1's geomagnetic target (55 нТл) to display `Anomaly` (amber)
while the identical-range baseline (~47 нТл) correctly displays `Normal`.*

---

### 4. `SensorBaseline` *(immutable record, static per-sensor)*

Thresholds controlling status colour coding (FR-012). One instance per sensor channel.

| Field | Type | Description |
|-------|------|-------------|
| `BaselineValue` | `double` | Centre of normal range; starting point for drift |
| `NormalMax` | `double` | Below → green / НОРМА |
| `ElevatedMax` | `double` | Below → amber / ПОВЫШЕН |
| `CriticalThreshold` | `double` | Above → red / ОПАСНОСТЬ or КРИТИЧНО |

Status thresholds per sensor:

| Sensor | Unit | НОРМА | ПОВЫШЕН | АНОМАЛИЯ | ОПАСНОСТЬ / КРИТИЧНО |
|--------|------|-------|---------|----------|----------------------|
| Neutron | мЗв/ч | ≤ 5 | 5–20 | — | > 20 |
| Ionisation | % | ≤ 25 | 25–80 | — | > 80 |
| Geomagnetic | нТл | ≤ 60 | 60–100 | *(anomaly-context only — see SensorTargetValues note)* | > 100 |
| Thermal | °C | ≤ 1.0 | 1.0–2.5 | — | > 2.5 |
| Chrono | Δt s | ≤ 0.05 | 0.05–0.3 | — | > 0.3 |

*`SensorBaseline` thresholds apply only during baseline (no active anomaly or `LerpProgress < 0.5`).
The АНОМАЛИЯ column is not reachable via thresholds alone — it is set explicitly in
`SensorTargetValues.GeomagneticStatus` when an anomaly requires it (e.g. Anomaly 1 → `Anomaly`).*

---

### 5. `SensorSnapshot` *(mutable; updated by SensorTickService)*

Instantaneous reading for one sensor channel, exposed as properties on `DetectorViewModel`.

| Field | Type | Description |
|-------|------|-------------|
| `Value` | `double` | Current displayed value (post-lerp + drift) |
| `Unit` | `string` | Display unit string, e.g. "мЗв/ч" |
| `Status` | `SensorStatus` | Current status level; drives gauge colour and label |

---

### 6. `DetectorState` *(mutable runtime state; owned by DetectorViewModel)*

Central mutable state object. Services mutate it via methods on `DetectorViewModel`
(all on the main thread, dispatched if needed).

| Field | Type | Description |
|-------|------|-------------|
| `Mode` | `DetectorMode` | Current mode, toggled by user |
| `Sensitivity` | `int` | 0–100 (%) |
| `NoiseSuppression` | `int` | 0–100 (%) |
| `ActiveAnomaly` | `Anomaly?` | Currently triggered anomaly; `null` when idle |
| `Phase` | `TransitionPhase` | Current phase of the transition state machine |
| `LerpProgress` | `double` | 0.0 → 1.0 during appear; 1.0 → 0.0 during fade |
| `PendingAnomaly` | `Anomaly?` | Next anomaly queued while first is still fading (FR-011) |

---

### 7. `RadarBlipTemplate` *(immutable; part of Anomaly definition)*

Prototype for blips that `RadarService` instantiates when an anomaly becomes active.

| Field | Type | Description |
|-------|------|-------------|
| `Type` | `BlipType` | Determines colour and symbol |
| `Count` | `int` | Number of blips to spawn (randomised within `CountMin`–`CountMax`) |
| `InitialDistanceMin` | `double` | Minimum distance from radar centre (0=centre, 1=edge) |
| `InitialDistanceMax` | `double` | Maximum distance |
| `DriftAngularSpeed` | `double` | Angular drift in rad/s (0 = stationary) |
| `DriftRadialSpeed` | `double` | Radial drift (0 = fixed distance) |
| `IsFixed` | `bool` | `true` for blips pinned to a specific position (e.g. Chrono blip at centre) |

---

### 8. `RadarBlip` *(mutable; managed by RadarCanvasView)*

Live blip instance on the radar canvas.

| Field | Type | Description |
|-------|------|-------------|
| `Type` | `BlipType` | Controls render colour |
| `Angle` | `double` | Current angle in radians |
| `Distance` | `double` | Current distance 0.0–1.0 |
| `Intensity` | `double` | 0.0–1.0; decays after radar sweep passes; resets when sweep revisits |
| `DriftAngularSpeed` | `double` | rad/s; applied each frame |
| `DriftRadialSpeed` | `double` | units/s; applied each frame |

---

## Enums

| Enum | Values |
|------|--------|
| `DetectorMode` | `Active`, `Passive` |
| `TransitionPhase` | `Idle`, `Appearing`, `Active`, `Fading` |
| `SensorStatus` | `Normal`, `Elevated`, `Anomaly`, `Danger`, `Critical`, `CriticalHigh` |
| `BlipType` | `EM`, `Radiation`, `Ionisation`, `Thermal`, `Geomagnetic`, `Chrono` |

---

## State Machine

```
Idle  ──(trigger matched)──►  Appearing  ──(lerp = 1.0)──►  Active
 ▲                                                             │
 │                                                  (trigger lost)
 │                                                             ▼
 └──────────(lerp = 0.0)───────────────────────────  Fading

Fading ──(new trigger while fading)──► store as PendingAnomaly; continue fading
       ──(lerp = 0.0 AND PendingAnomaly ≠ null)──► set ActiveAnomaly = PendingAnomaly;
                                                    PendingAnomaly = null; → Appearing
```

Sequential constraint (FR-011): a second anomaly's `Appearing` phase only begins after the
first anomaly's `Fading` phase reaches `LerpProgress ≤ 0.02` (effectively at baseline).
