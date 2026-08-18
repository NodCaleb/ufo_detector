using UfoDetector.Models;

namespace UfoDetector.Services;

/// <summary>Stub — replaced by AnomalyEvaluator in Phase 5 (T046).</summary>
public class NullAnomalyEvaluator : IAnomalyEvaluator
{
    public Anomaly? Evaluate(DetectorState state) => null;
}
