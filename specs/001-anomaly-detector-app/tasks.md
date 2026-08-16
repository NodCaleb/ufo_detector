---
description: "Task list for UFO Anomaly Detector App implementation"
---

# Tasks: UFO Anomaly Detector Mobile App

**Feature**: `001-anomaly-detector-app`
**Input**: Design documents from `/specs/001-anomaly-detector-app/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/anomaly-definitions.md ✅, quickstart.md ✅
**Tests**: Included — Constitution Principle V mandates TDD for all ViewModels and Service implementations. Write tests first; confirm they FAIL before writing implementation code. SkiaSharp draw-loop classes (`RadarCanvasView`, `InfrasoundCanvasView`) are exempt from unit tests per Principle V.
**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description — file path`

- **[P]**: Can run in parallel (different files, no incomplete-task dependencies)
- **[Story]**: Which user story this task belongs to (US1=P1, US2=P2, US3=P3, US4=P4)
- Setup and Foundational phases carry no story label
- Include exact file paths in every description

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the .NET solution, configure both projects, and establish the folder structure so every subsequent task has a valid home.

- [X] T001 Create .NET solution file `UfoDetector.sln` and MAUI app project `UfoDetector/UfoDetector.csproj` targeting `net10.0-android`
- [X] T002 Create xUnit test project `UfoDetector.Tests/UfoDetector.Tests.csproj` and add project reference to `UfoDetector`
- [X] T003 [P] Configure `UfoDetector/UfoDetector.csproj`: add NuGet packages `CommunityToolkit.Mvvm` 8.x, `SkiaSharp.Views.Maui.Controls`, `CommunityToolkit.Maui`; set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- [X] T004 [P] Configure `UfoDetector.Tests/UfoDetector.Tests.csproj`: add `xunit`, `xunit.runner.visualstudio`, `Moq`, `Microsoft.NET.Test.Sdk`
- [X] T005 Create empty folder structure: `UfoDetector/Models/Enums/`, `UfoDetector/Services/`, `UfoDetector/ViewModels/`, `UfoDetector/Views/`, `UfoDetector/Controls/`, `UfoDetector/Resources/Fonts/`, `UfoDetector/Resources/Styles/`; mirror `UfoDetector.Tests/ViewModels/`, `UfoDetector.Tests/Services/`
- [X] T006 Add `ShareTechMono.ttf` (or closest monospace fallback) to `UfoDetector/Resources/Fonts/` and register in `UfoDetector/MauiProgram.cs`
- [X] T007 [P] Define phosphor-green palette and CRT scanline overlay colour in `UfoDetector/Resources/Styles/Colors.xaml`
- [X] T008 Configure `UfoDetector/Platforms/Android/MainActivity.cs` with `ScreenOrientation = ScreenOrientation.Portrait` and the full `[Activity]` attribute per R-005 (no manual `AndroidManifest.xml` edits)

**Checkpoint**: `dotnet build UfoDetector/UfoDetector.csproj -f net10.0-android` succeeds with zero warnings; `dotnet test UfoDetector.Tests/UfoDetector.Tests.csproj` succeeds (no tests yet).

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: All model types, enums, anomaly data, and service interfaces that every user story depends on. **No user story work can begin until this phase is complete.**

**⚠️ CRITICAL**: These tasks must be finished before any Phase 3+ task starts.

### Enums (all parallelisable)

- [ ] T009 [P] Create `DetectorMode` enum (`Active`, `Passive`) in `UfoDetector/Models/Enums/DetectorMode.cs`
- [ ] T010 [P] Create `TransitionPhase` enum (`Idle`, `Appearing`, `Active`, `Fading`) in `UfoDetector/Models/Enums/TransitionPhase.cs`
- [ ] T011 [P] Create `SensorStatus` enum (`Normal`, `Elevated`, `Anomaly`, `Danger`, `Critical`, `CriticalHigh`) in `UfoDetector/Models/Enums/SensorStatus.cs`
- [ ] T012 [P] Create `BlipType` enum (`EM`, `Radiation`, `Ionisation`, `Thermal`, `Geomagnetic`, `Chrono`) in `UfoDetector/Models/Enums/BlipType.cs`

### Immutable Records (parallelisable)

- [ ] T013 [P] Create `AnomalyTrigger` immutable record (Mode, SensitivityMin/Max, NoiseSuppMin/Max) in `UfoDetector/Models/AnomalyTrigger.cs`
- [ ] T014 [P] Create `SensorTargetValues` immutable record (NeutronRadiation, NeutronStatus, Ionisation, IonisationStatus, GeomagneticField, GeomagneticStatus, ThermalAnomaly, ThermalStatus, ChronoAnomaly, ChronoStatus, InfrasoundBands double[20]) in `UfoDetector/Models/SensorTargetValues.cs` — per-channel `SensorStatus` fields used by SensorTickService when LerpProgress ≥ 0.5
- [ ] T015 [P] Create `RadarBlipTemplate` immutable record (Type, Count, InitialDistanceMin/Max, DriftAngularSpeed, DriftRadialSpeed, IsFixed) in `UfoDetector/Models/RadarBlipTemplate.cs`
- [ ] T016 [P] Create `Anomaly` immutable record (Id, Name, Narrative, Trigger, SensorTargets, RadarBlips) in `UfoDetector/Models/Anomaly.cs`
- [ ] T017 [P] Create `SensorBaseline` immutable record (BaselineValue, NormalMax, ElevatedMax, CriticalThreshold) with all per-sensor threshold constants from data-model.md in `UfoDetector/Models/SensorBaseline.cs`
- [ ] T018 [P] Create `SensorSnapshot` mutable record (Value, Unit, Status) in `UfoDetector/Models/SensorSnapshot.cs`
- [ ] T019 [P] Create `RadarBlip` mutable record (Type, Angle, Distance, Intensity, DriftAngularSpeed, DriftRadialSpeed) in `UfoDetector/Models/RadarBlip.cs`
- [ ] T020 Create `DetectorState` mutable record (Mode, Sensitivity, NoiseSuppression, ActiveAnomaly, Phase, LerpProgress, PendingAnomaly) in `UfoDetector/Models/DetectorState.cs` — depends on T009, T010, T016

### Anomaly Data

- [ ] T021 Implement `AnomalyDefinitions.cs` as `static readonly IReadOnlyList<Anomaly>` with all 5 anomaly definitions from `contracts/anomaly-definitions.md` (triggers, sensor targets including explicit `SensorStatus` per channel, radar blip templates) in `UfoDetector/Models/AnomalyDefinitions.cs` — depends on T013–T016

### Service Interfaces

- [ ] T022 [P] Create `IAnomalyEvaluator` interface (`Anomaly? Evaluate(DetectorState state)`) in `UfoDetector/Services/IAnomalyEvaluator.cs`
- [ ] T023 [P] Create `ISensorTickService` interface (`StartAsync()`, `Stop()`) in `UfoDetector/Services/ISensorTickService.cs`
- [ ] T024 [P] Create `ITransitionOrchestrator` interface (`void Step(DetectorState state, double elapsedSeconds)`) in `UfoDetector/Services/ITransitionOrchestrator.cs`

**Checkpoint**: `dotnet build` succeeds; all model types and interfaces exist with correct namespaces; `dotnet test` still passes (0 tests).

---

## Phase 3: User Story 1 — Viewing Live Sensor Readings (Priority: P1) 🎯 MVP

**Goal**: Launch the app and see all 6 sensor panels animated with real-time baseline drift, the radar sweep rotating at 60 fps, and the infrasound bar-graph animating — all showing НОРМА status.

**Independent Test**: Launch the app; confirm all 6 gauges (neutron radiation, air ionisation, geomagnetic field, thermal anomaly, chrono anomaly, infrasound spectrum) are visible, animate continuously with ±3–5 % drift every 1–2 s, and display green "НОРМА" labels. Demonstrable to stakeholders with no controls interaction.

### Tests for User Story 1 (Write FIRST — must FAIL before implementation)

- [ ] T025 [P] [US1] Write `SensorTickServiceTests`: assert drift stays within ±5 % of baseline, update period ~100 ms tick interval, baseline values correct per `SensorBaseline` constants in `UfoDetector.Tests/Services/SensorTickServiceTests.cs`
- [ ] T026 [P] [US1] Write `DetectorViewModelTests`: assert initial sensor snapshot values match baselines, `SensorStatus` is `Normal` for all channels at startup in `UfoDetector.Tests/ViewModels/DetectorViewModelTests.cs`

### Implementation for User Story 1

- [ ] T027 [US1] Implement `SensorTickService`: ~100 ms `IDispatcherTimer` loop; apply ±3–5 % random drift per sensor; notify `DetectorViewModel` via callback; implement `StartAsync()`/`Stop()` from `ISensorTickService` in `UfoDetector/Services/SensorTickService.cs`
- [ ] T028 [US1] Implement `DetectorViewModel` as `partial class` using `[ObservableProperty]`: flat observable properties for all 6 sensor values, units, and status labels; accepts `ISensorTickService` and `ITransitionOrchestrator` via constructor injection in `UfoDetector/ViewModels/DetectorViewModel.cs`
- [ ] T029 [US1] Register `SensorTickService`, `TransitionOrchestrator` (stub), and `DetectorViewModel` in DI in `UfoDetector/MauiProgram.cs`
- [ ] T030 [US1] Implement `RadarCanvasView` (`SKGLView` subclass, `EnableRenderLoop = true`): rotating sweep line, 3 concentric range rings, blip fade after sweep — all `SKPaint`/`SKPath` pre-allocated as fields, zero allocations inside `PaintSurface` per Constitution Principle IV in `UfoDetector/Controls/RadarCanvasView.cs`
- [ ] T031 [US1] Implement `InfrasoundCanvasView` (`SKGLView` subclass, `EnableRenderLoop = true`): 20-bin animated bar graph, dashed alarm threshold line — all `SKPaint` pre-allocated, zero allocations inside `PaintSurface` per Constitution Principle IV in `UfoDetector/Controls/InfrasoundCanvasView.cs`
- [ ] T032 [US1] Create `DetectorPage.xaml`: 6 sensor gauge panels (numeric label, unit label, status label, fill bar), `RadarCanvasView`, `InfrasoundCanvasView`; bind all sensor properties to `DetectorViewModel` in `UfoDetector/Views/DetectorPage.xaml`
- [ ] T033 [US1] Wire `DetectorPage.xaml.cs`: constructor (resolve `DetectorViewModel` via DI, set `BindingContext`), `Loaded` handler calls `SensorTickService.StartAsync()` in `UfoDetector/Views/DetectorPage.xaml.cs`
- [ ] T034 [US1] Configure `AppShell.xaml` to register and navigate to `DetectorPage` as the root route in `UfoDetector/AppShell.xaml`

**Checkpoint**: `dotnet test` passes (T025, T026 green). Launch app on emulator: all gauges animate, radar sweeps, infrasound bars move, all labels read "НОРМА" in green. Story 1 demonstrable independently.

---

## Phase 4: User Story 2 — Adjusting Mode, Sensitivity, and Noise Suppression (Priority: P2)

**Goal**: The controls panel is fully interactive — mode toggle switches АКТИВ/ПАССИВ with LED update, both sliders update their numeric labels in 1 % increments, values persist on app restart.

**Independent Test**: Set mode to АКТИВ, sensitivity to 75 %, noise suppression to 15 %. Confirm displayed values match exactly. No anomaly needed.

### Tests for User Story 2 (Write FIRST — must FAIL before implementation)

- [ ] T035 [P] [US2] Extend `DetectorViewModelTests`: assert `ToggleModeCommand` switches `Mode` between `Active`/`Passive`; assert `Sensitivity` and `NoiseSuppression` observable properties update to exact input values (0, 50, 100, boundaries); assert values are saved to `Preferences` on change in `UfoDetector.Tests/ViewModels/DetectorViewModelTests.cs`

### Implementation for User Story 2

- [ ] T036 [US2] Add `[RelayCommand] ToggleMode()` and `[ObservableProperty] DetectorMode Mode` to `DetectorViewModel`; on mode change call `IAnomalyEvaluator.Evaluate()` stub; update LED indicator property in `UfoDetector/ViewModels/DetectorViewModel.cs`
- [ ] T037 [US2] Add `[ObservableProperty] int Sensitivity` and `[ObservableProperty] int NoiseSuppression` to `DetectorViewModel`; persist both via `Microsoft.Maui.Storage.Preferences` on set; restore from `Preferences` in constructor in `UfoDetector/ViewModels/DetectorViewModel.cs`
- [ ] T038 [US2] Add controls panel to `DetectorPage.xaml`: mode toggle `Button` with LED indicator, sensitivity `Slider` (0–100, rotated 270° with swapped Width/Height per spec.md Assumptions — if MAUI Slider touch events mis-map on Android, replace with custom SkiaSharp touch-drag control), noise-suppression `Slider` (0–100, rotated 270°), numeric labels bound to `Sensitivity` and `NoiseSuppression` in `UfoDetector/Views/DetectorPage.xaml`
- [ ] T039 [US2] Style the mode toggle LED indicator (green = АКТИВ, amber/off = ПАССИВ) using `Colors.xaml` palette entries and a `Style` trigger in `UfoDetector/Views/DetectorPage.xaml`

**Checkpoint**: `dotnet test` passes (T035 green). On device: toggle cycles correctly, sliders update labels, values survive app restart. Story 2 demonstrable independently of anomaly logic.

---

## Phase 5: User Story 3 — Triggering an Anomaly (Priority: P3)

**Goal**: When all three conditions (mode, sensitivity, noise suppression) simultaneously match an anomaly's trigger, the detector transitions into that anomaly's visual state within 2–3 s. Moving any condition outside range fades the anomaly in 1.5 s. A second anomaly cannot start appearing until the first has fully faded (FR-011).

**Independent Test**: Set controls to Anomaly 1 combination (ПАССИВ, sensitivity 65 %, noise 20 %); radar, geomagnetic gauge, and infrasound panel must transition into Anomaly 1 state within 3 seconds.

### Tests for User Story 3 (Write FIRST — must FAIL before implementation)

- [ ] T040 [P] [US3] Write `AnomalyEvaluatorTests`: assert each of the 5 anomalies returns its correct `Anomaly` record when conditions match; assert `null` returned for no-match; assert boundary values are inclusive (sensitivity = 60 % triggers Anomaly 1; sensitivity = 59 % does not); assert no two anomalies match simultaneously for any (Mode, Sensitivity, NoiseSuppression) triple in `UfoDetector.Tests/Services/AnomalyEvaluatorTests.cs`
- [ ] T041 [P] [US3] Write `TransitionOrchestratorTests`: assert `LerpProgress` reaches 1.0 after `2500 ms / tickInterval` steps (appear); assert `LerpProgress` reaches 0.0 after `1500 ms / tickInterval` steps (fade); assert `PendingAnomaly` is not promoted to `ActiveAnomaly` until `LerpProgress ≤ 0.02` (FR-011) in `UfoDetector.Tests/Services/TransitionOrchestratorTests.cs`

### Implementation for User Story 3

- [ ] T042 [US3] Implement `AnomalyEvaluator.Evaluate(DetectorState state)`: iterate `AnomalyDefinitions.All`; return first `Anomaly` whose `Trigger` conditions are all satisfied (inclusive boundaries); return `null` if none match in `UfoDetector/Services/AnomalyEvaluator.cs`
- [ ] T043 [US3] Implement `TransitionOrchestrator.Step(DetectorState state, double elapsedSeconds)`: state machine `Idle→Appearing→Active→Fading`; lerp formula `progress += elapsedSeconds / duration`; sequential gate `LerpProgress ≤ 0.02` before promoting `PendingAnomaly`; clamp progress to [0.0, 1.0] in `UfoDetector/Services/TransitionOrchestrator.cs`
- [ ] T044 [US3] Integrate anomaly evaluation into `SensorTickService`: on each tick call `IAnomalyEvaluator.Evaluate()`; compare result to `DetectorState.ActiveAnomaly`; update `Phase` accordingly; call `ITransitionOrchestrator.Step()` with elapsed time in `UfoDetector/Services/SensorTickService.cs`
- [ ] T045 [US3] Add `[ObservableProperty] Anomaly? ActiveAnomaly`, `[ObservableProperty] TransitionPhase Phase`, and `[ObservableProperty] double LerpProgress` to `DetectorViewModel`; propagate state changes from tick service on main thread in `UfoDetector/ViewModels/DetectorViewModel.cs`
- [ ] T046 [US3] Register `AnomalyEvaluator` (replacing stub) in DI in `UfoDetector/MauiProgram.cs`

**Checkpoint**: `dotnet test` passes (T040, T041 green). On device: Anomaly 1 combination triggers visible state change within 3 s; exiting range fades within 1.5 s; rapid anomaly switching obeys FR-011 sequential constraint.

---

## Phase 6: User Story 4 — Anomaly-Specific Sensor Signatures (Priority: P4)

**Goal**: Each of the 5 anomalies produces a distinct, recognisable pattern: correct sensor values at target levels with correct status colour, correct radar blip type/count/behaviour, and correct infrasound spectrum shape.

**Independent Test**: Cycle through all 5 anomaly combinations; verify each activates only its documented sensors, radar blips match the contract (type, count, zone), and no two anomalies produce identical radar + gauge combinations.

### Tests for User Story 4 (Write FIRST — must FAIL before implementation)

- [ ] T047 [P] [US4] Extend `AnomalyEvaluatorTests`: for each of the 5 anomalies assert the returned `Anomaly` record's `SensorTargets` and `RadarBlips` match `contracts/anomaly-definitions.md` exactly (values, units, blip types, counts, and per-channel `SensorStatus` fields) in `UfoDetector.Tests/Services/AnomalyEvaluatorTests.cs`

### Implementation for User Story 4

- [ ] T048 [US4] Update `SensorTickService`: when `Phase` is `Appearing` or `Active`, lerp each sensor channel from `SensorBaseline` toward `SensorTargetValues` using `LerpProgress`; derive `SensorStatus` from `SensorBaseline` thresholds when `LerpProgress < 0.5`, and use `SensorTargetValues.{Channel}Status` directly when `LerpProgress ≥ 0.5` in `UfoDetector/Services/SensorTickService.cs`
- [ ] T049 [US4] Update `RadarCanvasView`: on `ActiveAnomaly` change, instantiate `RadarBlip` list from `RadarBlipTemplate.Count` range; apply per-frame angular/radial drift; intensity decays after sweep passes and resets on revisit; clear blips when `Phase` returns to `Idle` in `UfoDetector/Controls/RadarCanvasView.cs`
- [ ] T050 [US4] Update `InfrasoundCanvasView`: lerp the 20-band amplitude array from baseline (~0.05 per bin) toward `SensorTargetValues.InfrasoundBands` using `LerpProgress`; bars exceeding the alarm threshold render in red per quickstart.md V-7 procedure in `UfoDetector/Controls/InfrasoundCanvasView.cs`
- [ ] T051 [US4] Add `Narrative` label to `DetectorPage.xaml` bound to `ActiveAnomaly?.Narrative`; show only when `Phase` is `Active` or `Appearing`; hide (collapsed) during `Idle` and `Fading` in `UfoDetector/Views/DetectorPage.xaml`
- [ ] T052 [US4] Bind gauge fill colour and status label colour to `SensorStatus` via `IValueConverter` (Normal→green, Elevated→amber, Anomaly→amber, Danger→red, Critical→red, CriticalHigh→red) in `UfoDetector/Views/DetectorPage.xaml`

**Checkpoint**: `dotnet test` passes (T047 green). On device: cycle all 5 anomaly combinations; each produces the correct sensor readouts, status colours, radar blip type/count, and infrasound pattern per quickstart.md V-4.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Build integrity, manual validation, and performance sign-off.

- [ ] T053 [P] Verify `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` is enforced: run `dotnet build UfoDetector/UfoDetector.csproj -f net10.0-android` and confirm zero warnings across all source files
- [ ] T054 Run `dotnet test UfoDetector.Tests/UfoDetector.Tests.csproj` and confirm 100 % pass — covers `DetectorViewModelTests`, `AnomalyEvaluatorTests`, `SensorTickServiceTests`, `TransitionOrchestratorTests`
- [ ] T055 [P] Audit all `SKPaint`, `SKPath`, and `SKRect` usages in `UfoDetector/Controls/RadarCanvasView.cs` and `UfoDetector/Controls/InfrasoundCanvasView.cs`: confirm every allocation is a class field; confirm `new` is absent from `PaintSurface`/`OnPaintSurface` bodies per Constitution Principle IV
- [ ] T056 [P] Verify `UfoDetector/Platforms/Android/AndroidManifest.xml` does not contain `android.permission.INTERNET` (FR-014)
- [ ] T057 Execute quickstart.md scenarios V-1 through V-5 on emulator or device; record pass/fail for each acceptance criterion in `specs/001-anomaly-detector-app/quickstart.md`
- [ ] T058 Execute quickstart.md V-6 GPU profiler validation: connect Android GPU Profiler; confirm all frames ≤ 16.7 ms for both canvases during active-anomaly state for 30 continuous seconds (SC-002, Constitution Principle IV)
- [ ] T059 [P] Confirm all services (`AnomalyEvaluator`, `SensorTickService`, `TransitionOrchestrator`) and `DetectorViewModel` are registered in `UfoDetector/MauiProgram.cs`; confirm no logic exists in `DetectorPage.xaml.cs` beyond constructor and `Loaded` hook (MVVM Principle II)
- [ ] T060 Execute 30-minute soak test on emulator or device (SC-006): launch app, enter each of the 5 anomaly combinations in sequence, then leave running at baseline; use Android Studio Memory Profiler to confirm heap growth < 10 MB over the session; confirm frame rate remains ≥ 45 fps throughout; record results in `specs/001-anomaly-detector-app/quickstart.md` as scenario V-8

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — start immediately
- **Phase 2 (Foundational)**: Requires Phase 1 — **blocks all user story phases**
- **Phase 3 (US1)**: Requires Phase 2 — no dependency on US2, US3, US4
- **Phase 4 (US2)**: Requires Phase 2 — no dependency on US1 (can run in parallel with US3 if staffed; builds on DetectorViewModel from US1 in practice)
- **Phase 5 (US3)**: Requires Phase 2 + Phase 3 (needs `DetectorViewModel` observable state, `SensorTickService` tick loop)
- **Phase 6 (US4)**: Requires Phase 5 (needs active anomaly + lerp state from US3)
- **Phase 7 (Polish)**: Requires all user story phases

### User Story Dependencies

- **US1 (P1)**: Foundational complete → can start; independently testable
- **US2 (P2)**: Foundational complete → can start; independently testable (controls only)
- **US3 (P3)**: US1 + US2 functional → start; depends on ViewModel observable state and tick loop
- **US4 (P4)**: US3 functional → start; depends on anomaly transition state machine

### Within Each User Story

- **⚠️ TDD rule (Constitution V)**: Test tasks MUST be written before implementation tasks; tests MUST fail before implementation code exists
- Models/records → service interfaces → service implementation → ViewModel properties → View bindings
- Story complete and checkpoint passed before advancing to next priority

### Parallel Opportunities Per Phase

**Phase 1**: T003 ‖ T004 ‖ T006 ‖ T007 can run in parallel; T001→T002→T005 must sequence first

**Phase 2**: T009–T015 all parallelisable; T016 needs T013–T015; T017–T019 parallelisable; T020 needs T009+T010+T016; T021 needs T013–T016; T022–T024 parallelisable

**Phase 3**: T025 ‖ T026 (tests written first); then T027 ‖ T030 ‖ T031 once tests exist; T028 after T027; T032–T034 after T028

**Phase 4**: T035 (test written first); T036 ‖ T037 once test exists; T038 → T039

**Phase 5**: T040 ‖ T041 (tests written first); then T042 ‖ T043 in parallel; T044 after T042+T043; T045 after T044; T046 after T042

**Phase 6**: T047 (test written first); T048 ‖ T049 ‖ T050 in parallel after T047; T051 ‖ T052 in parallel after T048

**Phase 7**: T053 ‖ T055 ‖ T056 ‖ T059 in parallel; T054 sequential; T057 after T054; T058 after T057; T060 after T057

---

## Parallel Example: Phase 2 (Foundational Enums + Records)

```
# These can all launch simultaneously:
T009  Create DetectorMode enum
T010  Create TransitionPhase enum
T011  Create SensorStatus enum
T012  Create BlipType enum
T013  Create AnomalyTrigger record
T014  Create SensorTargetValues record
T015  Create RadarBlipTemplate record
T017  Create SensorBaseline record
T018  Create SensorSnapshot record
T019  Create RadarBlip record
T022  Create IAnomalyEvaluator interface
T023  Create ISensorTickService interface
T024  Create ITransitionOrchestrator interface

# After the above complete:
T016  Create Anomaly record       (needs T013–T015)
T020  Create DetectorState record (needs T009, T010, T016)
T021  Create AnomalyDefinitions   (needs T013–T016)
```

## Parallel Example: Phase 3 (US1 Implementation)

```
# Write tests first (both can be written in parallel):
T025  SensorTickServiceTests — write and confirm FAIL
T026  DetectorViewModelTests — write and confirm FAIL

# Then implement in parallel where files differ:
T027  SensorTickService.cs     (implements ISensorTickService)
T030  RadarCanvasView.cs       (independent SKGLView subclass)
T031  InfrasoundCanvasView.cs  (independent SKGLView subclass)

# After T027:
T028  DetectorViewModel.cs

# After T028:
T029  MauiProgram.cs (DI registration)
T032  DetectorPage.xaml
T033  DetectorPage.xaml.cs
T034  AppShell.xaml
```

---

## Implementation Strategy

### MVP Scope (User Story 1 Only — Phases 1–3)

1. Complete Phase 1: Setup (T001–T008)
2. Complete Phase 2: Foundational (T009–T024) — CRITICAL gating step
3. Write US1 tests first (T025–T026); confirm they FAIL
4. Complete Phase 3 implementation (T027–T034)
5. **STOP and VALIDATE**: `dotnet test` green; launch on emulator, confirm quickstart.md V-1

**Deliverable**: A fully animated sensor dashboard with live baseline readings, 60 fps SkiaSharp canvases, and НОРМА status across all gauges. No controls, no anomalies.

### Incremental Delivery

| Step | Phases | Deliverable |
|------|--------|-------------|
| MVP | 1 + 2 + 3 | Animated sensor dashboard (US1) |
| +Controls | + 4 | Interactive mode + sliders (US2) |
| +Triggers | + 5 | Anomaly appear/fade state machine (US3) |
| +Signatures | + 6 | All 5 distinct anomaly signatures (US4) |
| +Polish | + 7 | Build clean, GPU profiler pass, tests 100 % |

Each increment leaves the app in a demonstrable state that satisfies its completed user stories independently of future increments.

### Parallel Team Strategy

With multiple developers, after Phase 2 completes:

- **Developer A**: Phase 3 (US1) — sensor dashboard + SkiaSharp canvases
- **Developer B**: Phase 4 (US2) — controls panel (mode toggle + sliders)
- Stories converge before Phase 5 begins (US3 needs both ViewModel properties and controls)

---

## Notes

- `[P]` tasks operate on different files with no dependencies on incomplete tasks in the same phase
- `[Story]` label maps every task to a specific user story for independent traceability
- Tests marked `[P] [USn]` can be written in parallel with each other within the same story phase
- **SkiaSharp draw-loop tasks** (T030, T031, T049, T050) are explicitly exempt from mandatory unit tests per Constitution Principle V; validate using manual procedures in quickstart.md
- Commit after each completed task or logical group; use task ID as commit prefix (e.g. `T027 Implement SensorTickService`)
- Stop at each phase checkpoint to validate the story increment before advancing
- Boundaries are **inclusive** on both ends for all anomaly trigger ranges (FR-008, data-model.md)
