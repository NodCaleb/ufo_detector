<!--
  SYNC IMPACT REPORT
  Version change: 1.0.0 → 1.1.1 (PATCH)
  Bump rationale: Runtime updated to .NET 10 LTS / C# 14; build TFM updated
    to net10.0-android. No principle or governance changes.
  Modified principles:
    - Principle I: added tablet device profile constraint (768×1024 px, portrait)
    - Principle III: "Offline-Capable by Default" → "Visual Effects Domain —
      Zero External Connections" (fundamental scope clarification)
    - Principle V (was IV): TDD — SkiaSharp draw loops exempted from unit tests,
      manual test procedure added as compensating requirement
    - Principle VI (was V): Simplicity & YAGNI — rationale de-coupled from LARP
      context; reframed around visual effects domain
  Added sections:
    - Principle IV "Performance-First Rendering" (new)
  Removed sections: none
  Templates requiring updates:
    ✅ plan-template.md  — Constitution Check gate is generic; compatible
    ✅ spec-template.md  — no principle-specific language; compatible
    ✅ tasks-template.md — mobile path convention compatible; no changes needed
  Follow-up TODOs: none
-->

# UFO Detector Constitution

## Core Principles

### I. Android-First via .NET MAUI

The app MUST target Android as the sole platform, built with .NET MAUI as the UI
framework. All UI components MUST use MAUI controls and layouts. Platform-specific
customisations are permitted only via MAUI handlers, effects, or platform projects
— never by bypassing the MAUI abstraction layer in shared code.
Target device profile: tablet (≥768 × 1024 px), portrait orientation.

**Rationale**: Constraining the target platform eliminates cross-platform
trade-offs, keeps the build pipeline simple, and ensures the full MAUI Android
feature set is available without compromise.

### II. MVVM Clean Architecture (NON-NEGOTIABLE)

All features MUST follow the Model-View-ViewModel (MVVM) pattern:
- **Models** — plain data/domain objects; no UI or service dependencies.
- **Services** — business logic and effect orchestration; injected via interfaces.
- **ViewModels** — expose observable state and commands; no code-behind logic.
- **Views** — XAML only; zero application logic; bind exclusively to ViewModels.

Code-behind files MUST contain only constructor calls and navigation wiring.
Dependency injection (Microsoft.Extensions.DependencyInjection via
`MauiProgram.cs`) MUST be used for all services and ViewModels.

**Rationale**: MVVM decouples UI from logic, making ViewModels unit-testable
without a running Android device or emulator.

### III. Visual Effects Domain — Zero External Connections

The application is exclusively a visual effects simulator. It MUST NOT establish
any network connections — no REST calls, no WebSocket connections, no background
sync, no telemetry, and no SDK that initiates outbound communication. All
"sensor" readings MUST be generated algorithmically within the app. The
`INTERNET` permission MUST NOT be declared in `AndroidManifest.xml`.

**Rationale**: The app's value is entirely in its animations and interactive
visual effects. External connectivity adds attack surface, complicates testing,
and is architecturally unnecessary given the self-contained visual domain.

### IV. Performance-First Rendering

Visual effects MUST render at a stable 60 fps on a mid-range Android tablet.
All custom 2D graphics MUST use SkiaSharp (via `SkiaSharp.Views.Maui.Controls`).
Rendering work MUST remain off the UI thread where possible; only the final
draw call interacts with the UI thread. Allocations inside draw loops are
prohibited — `SKPaint`, `SKPath`, and other SkiaSharp objects MUST be
pre-allocated during surface initialisation. Frame time MUST be validated with
the Android GPU profiler before any rendering feature is considered complete.

**Rationale**: The entire user experience depends on smooth, convincing
animations. Dropped frames are a first-class defect, not a cosmetic issue.

### V. Test-Driven Development

Unit tests MUST be written for all ViewModels and Service implementations before
the implementation code is written. The Red-Green-Refactor cycle is mandatory:
tests MUST fail first, then pass only after the implementation is complete.
Test project MUST use NUnit or xUnit and run via `dotnet test` without requiring
a device or emulator. SkiaSharp draw-loop code is exempt from unit tests but MUST
have a documented manual test procedure per surface. Integration tests covering
navigation flows are encouraged but not mandatory.

**Rationale**: MVVM enables ViewModel tests to run fast in CI without Android
tooling. Exempting GPU draw code from mandatory unit tests is a pragmatic
concession; the manual test procedure compensates.

### VI. Simplicity & YAGNI

Every feature MUST be the minimum implementation that satisfies the acceptance
criteria. No speculative abstractions, premature optimisations, or unused
dependencies are permitted. NuGet packages MUST be justified in the PR description.
Complexity MUST be challenged at review; if a simpler approach exists it MUST be
preferred.

**Rationale**: The app is a self-contained visual effects tool. Over-engineering
wastes effort that is better spent on effect quality and UX polish.

## Technology Stack

- **Language**: C# 14 (.NET 10 LTS)
- **UI Framework**: .NET MAUI (Android target)
- **Target SDK**: Android API 26+ (Android 8.0 Oreo minimum)
- **Architecture pattern**: MVVM with CommunityToolkit.Mvvm
- **DI container**: Microsoft.Extensions.DependencyInjection (built-in MAUI)
- **Graphics/Rendering**: SkiaSharp (`SkiaSharp.Views.Maui.Controls`)
- **Animation helpers**: CommunityToolkit.Maui (optional; only when MAUI native
  animation APIs are insufficient — justify in PR)
- **Local storage**: `Preferences` only (slider/control state); SQLite is
  prohibited — no persistent structured data is required by the domain
- **Networking**: None; `INTERNET` permission MUST NOT appear in
  `AndroidManifest.xml`
- **Testing**: NUnit or xUnit + Moq, running via `dotnet test` (no device needed)
- **Build tool**: `dotnet build` / `dotnet publish` targeting Android
- **CI**: GitHub Actions (`dotnet-android` workflow)

No additional frameworks (e.g., Blazor Hybrid, Xamarin.Forms shims) are permitted
without a documented constitutional amendment.

## Development Workflow

- **Branching**: feature branches named `###-short-description` from `main`;
  short-lived; merged via PR only.
- **PR requirements**: all `dotnet test` checks pass; at least one reviewer
  approval; Constitution Check gate verified (see plan-template); 60 fps
  validation confirmed for any rendering changes.
- **Commit messages**: imperative mood, present tense, ≤72 chars subject line.
- **Build gate**: `dotnet build -f net10.0-android` MUST succeed with zero
  warnings treated as errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
- **Release**: APK produced via `dotnet publish` with Release configuration;
  versioned as MAJOR.MINOR.PATCH in `AndroidManifest.xml` and `.csproj`.

## Governance

This constitution supersedes all other practices and guidelines for the
UFO Detector project. Amendments require:
1. A written proposal describing the change and rationale.
2. Version bump following semantic versioning rules documented in this file.
3. Consistency propagation: all affected templates and docs updated in the
   same commit.

All PRs MUST include a "Constitution Check" confirming no principles are violated.
Any deviation from a principle MUST be escalated to a constitutional amendment —
not silently ignored.

**Version**: 1.1.1 | **Ratified**: 2026-08-12 | **Last Amended**: 2026-08-15
