using UfoDetector.Models.Enums;

namespace UfoDetector.Models;

public record RadarBlip
{
    public BlipType Type              { get; set; }
    public double   Angle             { get; set; }
    public double   Distance          { get; set; }
    public double   Intensity         { get; set; }
    public double   DriftAngularSpeed { get; set; }
    public double   DriftRadialSpeed  { get; set; }
}
