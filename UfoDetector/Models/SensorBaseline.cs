using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

/// <summary>
/// Per-sensor thresholds for status colour coding (FR-012). Static instances match data-model.md.
/// </summary>
public record SensorBaseline(double BaselineValue, double NormalMax, double ElevatedMax, double CriticalThreshold)
{
    public static readonly SensorBaseline Neutron     = new(3.2,   5.0,  20.0,  20.0);
    public static readonly SensorBaseline Ionisation  = new(12.0,  25.0, 80.0,  80.0);
    public static readonly SensorBaseline Geomagnetic = new(47.0,  60.0, 100.0, 100.0);
    public static readonly SensorBaseline Thermal     = new(0.0,   1.0,  2.5,   2.5);
    public static readonly SensorBaseline Chrono      = new(0.003, 0.05, 0.3,   0.3);

    public SensorStatus Classify(double value) => value switch
    {
        _ when value <= NormalMax      => SensorStatus.Normal,
        _ when value <= ElevatedMax    => SensorStatus.Elevated,
        _                              => SensorStatus.Danger,
    };
}
