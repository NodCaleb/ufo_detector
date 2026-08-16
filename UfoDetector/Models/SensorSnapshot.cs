using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

public record SensorSnapshot
{
    public double Value  { get; set; }
    public string Unit   { get; set; } = string.Empty;
    public SensorStatus Status { get; set; }
}
