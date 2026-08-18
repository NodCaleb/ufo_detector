using UfoDetector.Models;
using UfoDetector.Models.Enums;
using UfoDetector.Services;

namespace UfoDetector.Tests.Services;

public class TransitionOrchestratorTests
{
    private readonly TransitionOrchestrator _sut = new();

    // 100 ms tick expressed as seconds (matches SensorTickService.TickInterval)
    private const double Tick = 0.1;

    private static Anomaly Anomaly(int id) =>
        AnomalyDefinitions.All.First(a => a.Id == id);

    // ── Appearing: LerpProgress reaches 1.0 in 2500 ms ─────────────────────

    [Fact]
    public void Appearing_LerpProgress_Reaches_1_After_2500ms_Steps()
    {
        var state = new DetectorState
        {
            Phase          = TransitionPhase.Idle,
            PendingAnomaly = Anomaly(1),
        };

        // One Idle step promotes PendingAnomaly and enters Appearing
        _sut.Step(state, Tick);
        Assert.Equal(TransitionPhase.Appearing, state.Phase);
        Assert.Equal(0.0, state.LerpProgress);

        // 25 Appearing steps = 2500 ms; LerpProgress should reach 1.0
        for (int i = 0; i < 25; i++)
            _sut.Step(state, Tick);

        Assert.Equal(TransitionPhase.Active, state.Phase);
        Assert.Equal(1.0, state.LerpProgress, precision: 5);
    }

    // ── Fading: LerpProgress reaches 0.0 in 1500 ms ─────────────────────────

    [Fact]
    public void Fading_LerpProgress_Reaches_0_After_1500ms_Steps()
    {
        var state = new DetectorState
        {
            Phase          = TransitionPhase.Active,
            LerpProgress   = 1.0,
            ActiveAnomaly  = Anomaly(1),
            PendingAnomaly = null,   // no match → triggers fading
        };

        // One Active step (no PendingAnomaly match) enters Fading
        _sut.Step(state, Tick);
        Assert.Equal(TransitionPhase.Fading, state.Phase);
        Assert.Equal(1.0, state.LerpProgress, precision: 5);

        // 15 Fading steps = 1500 ms; LerpProgress should reach 0.0 → Idle
        for (int i = 0; i < 15; i++)
            _sut.Step(state, Tick);

        Assert.Equal(TransitionPhase.Idle, state.Phase);
        Assert.Equal(0.0, state.LerpProgress, precision: 5);
    }

    // ── FR-011: PendingAnomaly not promoted until LerpProgress ≤ 0.02 ────────

    [Fact]
    public void FR011_PendingAnomaly_Not_Promoted_While_LerpProgress_Above_Gate()
    {
        var active  = Anomaly(1);
        var pending = Anomaly(2);
        var state = new DetectorState
        {
            Phase          = TransitionPhase.Fading,
            LerpProgress   = 0.5,
            ActiveAnomaly  = active,
            PendingAnomaly = pending,
        };

        // One step: LerpProgress drops to ~0.43, still above 0.02 gate
        _sut.Step(state, Tick);

        Assert.Equal(TransitionPhase.Fading, state.Phase);
        Assert.Equal(active, state.ActiveAnomaly);  // no promotion yet
    }

    [Fact]
    public void FR011_PendingAnomaly_Promoted_When_LerpProgress_Drops_To_Gate()
    {
        var active  = Anomaly(1);
        var pending = Anomaly(2);
        var state = new DetectorState
        {
            Phase          = TransitionPhase.Fading,
            LerpProgress   = 0.021,   // just above the 0.02 gate
            ActiveAnomaly  = active,
            PendingAnomaly = pending,
        };

        // One step: LerpProgress drops below 0.02 → promotion should occur
        _sut.Step(state, Tick);

        Assert.Equal(TransitionPhase.Appearing, state.Phase);
        Assert.Equal(pending, state.ActiveAnomaly);
        Assert.Equal(0.0, state.LerpProgress);
    }

    // ── Additional state machine checks ─────────────────────────────────────

    [Fact]
    public void Idle_Does_Nothing_Without_PendingAnomaly()
    {
        var state = new DetectorState { Phase = TransitionPhase.Idle };
        _sut.Step(state, Tick);
        Assert.Equal(TransitionPhase.Idle, state.Phase);
        Assert.Equal(0.0, state.LerpProgress);
    }

    [Fact]
    public void Active_Stays_Active_While_Same_Anomaly_Pending()
    {
        var anomaly = Anomaly(1);
        var state = new DetectorState
        {
            Phase          = TransitionPhase.Active,
            LerpProgress   = 1.0,
            ActiveAnomaly  = anomaly,
            PendingAnomaly = anomaly,   // same anomaly still matching
        };

        _sut.Step(state, Tick);
        Assert.Equal(TransitionPhase.Active, state.Phase);
    }
}
