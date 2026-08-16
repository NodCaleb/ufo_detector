using UfoDetector.Models;

namespace UfoDetector.Services;

public interface IAnomalyEvaluator
{
    Anomaly? Evaluate(DetectorState state);
}
