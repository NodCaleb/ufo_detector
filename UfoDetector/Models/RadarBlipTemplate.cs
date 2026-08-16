using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

public record RadarBlipTemplate(
    BlipType Type,
    int CountMin,
    int CountMax,
    double InitialDistanceMin,
    double InitialDistanceMax,
    double DriftAngularSpeed,
    double DriftRadialSpeed,
    bool IsFixed);
