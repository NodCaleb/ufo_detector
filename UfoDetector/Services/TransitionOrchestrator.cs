using UfoDetector.Models;
using UfoDetector.Models.Enums;

namespace UfoDetector.Services;

public class TransitionOrchestrator : ITransitionOrchestrator
{
    private const double AppearDuration = 2.5;   // seconds (2500 ms)
    private const double FadeDuration   = 1.5;   // seconds (1500 ms)
    private const double PromoteGate    = 0.02;  // FR-011: gate for pending promotion

    public void Step(DetectorState state, double elapsedSeconds)
    {
        switch (state.Phase)
        {
            case TransitionPhase.Idle:
                if (state.PendingAnomaly is not null)
                {
                    state.ActiveAnomaly  = state.PendingAnomaly;
                    state.PendingAnomaly = null;
                    state.Phase          = TransitionPhase.Appearing;
                    state.LerpProgress   = 0.0;
                }
                break;

            case TransitionPhase.Appearing:
                state.LerpProgress = Math.Clamp(
                    state.LerpProgress + elapsedSeconds / AppearDuration, 0.0, 1.0);
                if (state.LerpProgress >= 1.0)
                    state.Phase = TransitionPhase.Active;
                break;

            case TransitionPhase.Active:
                if (state.PendingAnomaly?.Id != state.ActiveAnomaly?.Id)
                    state.Phase = TransitionPhase.Fading;
                break;

            case TransitionPhase.Fading:
                state.LerpProgress = Math.Clamp(
                    state.LerpProgress - elapsedSeconds / FadeDuration, 0.0, 1.0);
                if (state.LerpProgress <= PromoteGate)
                {
                    if (state.PendingAnomaly is not null)
                    {
                        // FR-011: promote only once nearly faded
                        state.ActiveAnomaly  = state.PendingAnomaly;
                        state.PendingAnomaly = null;
                        state.Phase          = TransitionPhase.Appearing;
                        state.LerpProgress   = 0.0;
                    }
                    else
                    {
                        state.ActiveAnomaly = null;
                        state.Phase         = TransitionPhase.Idle;
                        state.LerpProgress  = 0.0;
                    }
                }
                break;
        }
    }
}

