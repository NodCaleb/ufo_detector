using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

public record AnomalyTrigger(
    DetectorMode Mode,
    int SensitivityMin,
    int SensitivityMax,
    int NoiseSuppMin,
    int NoiseSuppMax);
