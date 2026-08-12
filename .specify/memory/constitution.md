<!--
  SYNC IMPACT REPORT
  Version change: (none — initial fill) → 1.0.0
  Modified principles: N/A (first-time template population)
  Added sections: Core Principles, Technology Stack, Development Workflow, Governance
  Removed sections: N/A
  Templates requiring updates:
    ✅ plan-template.md  — Option 3 (Mobile + API) already present; Constitution Check gate language is generic and compatible
    ✅ spec-template.md  — no principle-specific language; fully compatible
    ✅ tasks-template.md — mobile path convention (android/src/) already present; compatible
  Follow-up TODOs: none
-->

# UFO Detector Constitution

## Core Principles

### I. Android-First via .NET MAUI

The app MUST target Android as the sole platform, built with .NET MAUI as the UI
framework. All UI components MUST use MAUI controls and layouts. Platform-specific
customizations are permitted only via MAUI handlers, effects, or platform projects
— never by bypassing the MAUI abstraction layer in shared code.

**Rationale**: Constraining the target platform eliminates cross-platform
trade-offs, keeps the build pipeline simple, and ensures the full MAUI Android
feature set is available without compromise.

### II. MVVM Clean Architecture (NON-NEGOTIABLE)

All features MUST follow the Model-View-ViewModel (MVVM) pattern:
- **Models** — plain data/domain objects; no UI or service dependencies.
- **Services** — business logic and data access; injected via interfaces.
- **ViewModels** — expose observable state and commands; no code-behind logic.
- **Views** — XAML only; zero application logic; bind exclusively to ViewModels.

Code-behind files MUST contain only constructor calls and navigation wiring.
Dependency injection (Microsoft.Extensions.DependencyInjection via
`MauiProgram.cs`) MUST be used for all services and ViewModels.

**Rationale**: MVVM decouples UI from logic, making ViewModels unit-testable
without a running Android device or emulator.

### III. Offline-Capable by Default

Core app functionality MUST work without network connectivity. Persistent state
MUST use local storage (SQLite via sqlite-net-pcl, or `Preferences` for simple
key-value data). Network calls are additive enhancements, never prerequisites
for primary workflows. Graceful degradation MUST be implemented for all
network-dependent features.

**Rationale**: A LARP field environment cannot guarantee connectivity. The app
must be reliable regardless of signal quality.

### IV. Test-Driven Development

Unit tests MUST be written for all ViewModels and Service implementations before
the implementation code is written. The Red-Green-Refactor cycle is mandatory:
tests MUST fail first, then pass only after the implementation is complete.
Test project MUST use NUnit or xUnit and run via `dotnet test` without requiring
a device or emulator. Integration tests covering navigation flows are encouraged
but not mandatory.

**Rationale**: MVVM enables ViewModel tests to run fast in CI without Android
tooling. TDD ensures correctness is verified before UI integration.

### V. Simplicity & YAGNI

Every feature MUST be the minimum implementation that satisfies the acceptance
criteria. No speculative abstractions, premature optimisations, or unused
dependencies are permitted. NuGet packages MUST be justified in the PR description.
Complexity MUST be challenged at review; if a simpler approach exists it MUST be
preferred.

**Rationale**: The app is a fun LARP utility. Over-engineering wastes time that
is better spent on gameplay features.

## Technology Stack

- **Language**: C# 12 (.NET 8 or later)
- **UI Framework**: .NET MAUI (Android target)
- **Target SDK**: Android API 26+ (Android 8.0 Oreo minimum)
- **Architecture pattern**: MVVM with CommunityToolkit.Mvvm
- **DI container**: Microsoft.Extensions.DependencyInjection (built-in MAUI)
- **Local storage**: sqlite-net-pcl for structured data; `Preferences` for
  settings/flags
- **Testing**: NUnit or xUnit + Moq, running via `dotnet test` (no device needed)
- **Build tool**: `dotnet build` / `dotnet publish` targeting Android
- **CI**: GitHub Actions (`dotnet-android` workflow)

No additional frameworks (e.g., Blazor Hybrid, Xamarin.Forms shims) are permitted
without a documented constitutional amendment.

## Development Workflow

- **Branching**: feature branches named `###-short-description` from `main`;
  short-lived; merged via PR only.
- **PR requirements**: all `dotnet test` checks pass; at least one reviewer
  approval; Constitution Check gate verified (see plan-template).
- **Commit messages**: imperative mood, present tense, ≤72 chars subject line.
- **Build gate**: `dotnet build -f net8.0-android` MUST succeed with zero
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

**Version**: 1.0.0 | **Ratified**: 2026-08-12 | **Last Amended**: 2026-08-12
