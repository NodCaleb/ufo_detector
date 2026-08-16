namespace UfoDetector.Models;

public record Anomaly(
    int Id,
    string Name,
    string Narrative,
    AnomalyTrigger Trigger,
    SensorTargetValues SensorTargets,
    IReadOnlyList<RadarBlipTemplate> RadarBlips);
