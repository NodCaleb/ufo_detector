using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

public record DetectorState
{
    public DetectorMode    Mode             { get; set; } = DetectorMode.Passive;
    public int             Sensitivity      { get; set; } = 50;
    public int             NoiseSuppression { get; set; } = 50;
    public Anomaly?        ActiveAnomaly    { get; set; }
    public TransitionPhase Phase            { get; set; } = TransitionPhase.Idle;
    public double          LerpProgress     { get; set; }
    public Anomaly?        PendingAnomaly   { get; set; }
}
