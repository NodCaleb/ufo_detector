using UfoDetector.Models;

namespace UfoDetector.Services;

public class AnomalyEvaluator : IAnomalyEvaluator
{
    public Anomaly? Evaluate(DetectorState state)
    {
        foreach (var anomaly in AnomalyDefinitions.All)
        {
            var t = anomaly.Trigger;
            if (state.Mode             == t.Mode             &&
                state.Sensitivity      >= t.SensitivityMin   &&
                state.Sensitivity      <= t.SensitivityMax   &&
                state.NoiseSuppression >= t.NoiseSuppMin     &&
                state.NoiseSuppression <= t.NoiseSuppMax)
                return anomaly;
        }
        return null;
    }
}
